---

description: "Task list template for feature implementation"
---

# Tasks: Migración a SQL Server y Contenerización con Docker

**Input**: Design documents from `/specs/003-sqlserver-docker-migration/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/environment-variables.md](./contracts/environment-variables.md), [quickstart.md](./quickstart.md)

**Tests**: La spec no solicita explícitamente pruebas automatizadas nuevas (TDD); las tareas de
validación de cada historia se apoyan en `quickstart.md` (verificación manual/exploratoria de
extremo a extremo) y en la suite xUnit existente (para confirmar ausencia de regresiones), sin
crear nuevos archivos de test.

**Organization**: Las tareas están agrupadas por historia de usuario para permitir
implementación y validación independientes de cada una.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Se puede ejecutar en paralelo (archivos distintos, sin dependencias)
- **[Story]**: A qué historia de usuario pertenece la tarea (US1, US2, US3)
- Cada descripción incluye la ruta de archivo exacta

## Path Conventions

Aplicación web ya separada en Backend + Frontend (funcionalidad 002): `src/TimeClockSystem.Api/`,
`src/TimeClockSystem.Infrastructure/`, `src/TimeClockSystem.Web/`, `tests/`. Los artefactos nuevos
de esta funcionalidad viven junto a cada proyecto (`Dockerfile`) y en la raíz del repositorio
(`docker-compose.yml`, `.env.example`, `.dockerignore`).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Higiene de repositorio necesaria antes de contenerizar, independiente del cambio de proveedor de datos.

- [X] T001 [P] Crear `.dockerignore` en la raíz del repositorio, excluyendo `**/bin/`, `**/obj/`, `.git/`, `.vs/`, `.vscode/`, `*.db`, `*.db-shm`, `*.db-wal`, `tests/` y `specs/`, para que el build de las imágenes no copie artefactos innecesarios ni el archivo `.env`. (SC-005)
- [X] T002 [P] Agregar `.env` (pero no `.env.example`) a `.gitignore` en la raíz del repositorio, para evitar que cualquier secreto real completado localmente se versione accidentalmente (FR-009).

**Checkpoint**: Repositorio listo para agregar artefactos de contenerización sin riesgo de filtrar secretos ni inflar las imágenes.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Cambiar el motor de base de datos del Backend de SQLite a SQL Server. Es prerrequisito bloqueante para las tres historias de usuario: sin esto, ni el arranque contenedorizado (US1), ni la continuidad de comportamiento (US2), ni la persistencia en SQL Server (US3) tienen sentido.

**⚠️ CRITICAL**: Ninguna historia de usuario puede completarse hasta terminar esta fase.

- [X] T003 En `src/TimeClockSystem.Infrastructure/TimeClockSystem.Infrastructure.csproj`, reemplazar el `PackageReference` de `Microsoft.EntityFrameworkCore.Sqlite` por `Microsoft.EntityFrameworkCore.SqlServer`, versión `10.0.12` (misma versión que los demás paquetes EF Core del proyecto) (FR-001).
- [X] T004 En `src/TimeClockSystem.Api/Program.cs`, cambiar `options.UseSqlite(connectionString)` por `options.UseSqlServer(connectionString)` en el registro de `ApplicationDbContext` (FR-001). Depende de T003.
- [X] T005 Eliminar todos los archivos de `src/TimeClockSystem.Infrastructure/Data/Migrations/` (las 6 migraciones existentes atadas a SQLite y su `ApplicationDbContextModelSnapshot.cs`), conforme a la decisión de `research.md` §1 (no se preservan datos existentes — Clarification de la spec) (FR-014). Depende de T004.
- [X] T006 Generar una única migración inicial contra el proveedor SQL Server (`dotnet ef migrations add InitialSqlServer --project src/TimeClockSystem.Infrastructure --startup-project src/TimeClockSystem.Api --output-dir Data/Migrations`), verificando que se crean los nuevos archivos en `src/TimeClockSystem.Infrastructure/Data/Migrations/` y que el esquema resultante cubre todas las entidades listadas en `data-model.md` (FR-014). Depende de T005.
- [X] T007 En `src/TimeClockSystem.Api/Program.cs`, envolver la llamada a `db.Database.MigrateAsync()` en un bucle de reintento con una ventana acotada de tiempo/intentos (p. ej. hasta ~2 minutos, con espera entre intentos y registro de cada intento fallido vía `ILogger`), dejando que la excepción se propague sin capturarla si se agota la ventana o si la migración en sí falla por un motivo distinto a "base de datos no lista" (FR-007, FR-010, research.md §4 y §5). Depende de T004.
- [X] T008 [P] Actualizar el manejo de `ConnectionStrings:DefaultConnection`: quitar el valor `"Data Source=timeclock.db"` de `src/TimeClockSystem.Api/appsettings.json` (dejar solo el `_comment` indicando que MUST suministrarse por configuración/variable de entorno, igual que ya ocurre con `Jwt:SigningKey`), y agregar un valor de marcador de posición solo para desarrollo en `src/TimeClockSystem.Api/appsettings.Development.json` con el mismo patrón que el `Jwt:SigningKey` de ese archivo (p. ej. `Server=localhost,1433;Database=TimeClockSystem;User Id=sa;Password=dev-only-CHANGE-ME;TrustServerCertificate=True`), habilitando que `dotnet run` siga funcionando sin Docker apuntando a una instancia SQL Server local (FR-013).

**Checkpoint**: El Backend compila y (contra una instancia SQL Server accesible) aplica la migración inicial y siembra datos correctamente fuera de Docker. Las tres historias de usuario pueden comenzar.

---

## Phase 3: User Story 1 - Levantar todo el sistema con un solo comando (Priority: P1) 🎯 MVP

**Goal**: Un desarrollador o responsable de QA con Docker instalado ejecuta un único comando y obtiene los tres componentes (SQL Server, Backend, Frontend) funcionando y comunicados entre sí, sin pasos manuales adicionales.

**Independent Test**: Clonar el repositorio en una máquina limpia con Docker, copiar `.env.example` a `.env` con valores propios, ejecutar `docker compose up --build`, y confirmar que los tres servicios quedan accesibles y operativos (quickstart.md pasos 1–2).

### Implementation for User Story 1

- [X] T009 [P] [US1] Crear `src/TimeClockSystem.Api/Dockerfile` multi-stage: etapa de build con `mcr.microsoft.com/dotnet/sdk:10.0` (restore + build + publish de `TimeClockSystem.Api.csproj`) y etapa final con `mcr.microsoft.com/dotnet/aspnet:10.0` que copia el resultado publicado y expone el puerto `8080` (research.md §6).
- [X] T010 [P] [US1] Crear `src/TimeClockSystem.Web/Dockerfile` multi-stage con el mismo patrón que T009, para `TimeClockSystem.Web.csproj`, exponiendo el puerto `8080` (research.md §6).
- [X] T011 [P] [US1] Crear `.env.example` en la raíz del repositorio con las claves `MSSQL_SA_PASSWORD` y `JWT_SIGNING_KEY` y valores de marcador de posición (nunca secretos reales), documentando en un comentario la política de complejidad de contraseña de SQL Server, conforme a `contracts/environment-variables.md` (FR-009).
- [X] T012 [US1] Crear `docker-compose.yml` en la raíz del repositorio con el servicio `sqlserver` (imagen `mcr.microsoft.com/mssql/server:2022-latest`, `ACCEPT_EULA=Y`, `MSSQL_PID=Developer`, `MSSQL_SA_PASSWORD=${MSSQL_SA_PASSWORD}`, puerto `1433` publicado al host, y un healthcheck que verifique con `sqlcmd` que el servidor acepta conexiones) (FR-004, research.md §2).
- [X] T013 [US1] Agregar el servicio `api` a `docker-compose.yml`: `build: src/TimeClockSystem.Api` (usando el Dockerfile de T009), variables de entorno `ConnectionStrings__DefaultConnection` (compuesta con `${MSSQL_SA_PASSWORD}` apuntando al host `sqlserver`), `Jwt__SigningKey=${JWT_SIGNING_KEY}`, `ASPNETCORE_ENVIRONMENT=Development`, `depends_on: sqlserver: condition: service_healthy`, y puerto publicado al host (FR-002, FR-006, FR-007). Depende de T009, T012.
- [X] T014 [US1] Agregar el servicio `web` a `docker-compose.yml`: `build: src/TimeClockSystem.Web` (usando el Dockerfile de T010), variables de entorno `Api__BaseUrl=http://api:8080/`, `ASPNETCORE_ENVIRONMENT=Development`, y puerto publicado al host (FR-003, FR-008). Depende de T010, T013.
- [X] T015 [US1] Validar de extremo a extremo siguiendo `quickstart.md` pasos 1–2: ejecutar `docker compose up --build`, confirmar en los logs que `api` espera/reintenta hasta que `sqlserver` está lista, aplica migraciones y siembra datos, y que `web` es accesible y se comunica con `api` por nombre de servicio (SC-001, SC-006). Depende de T013, T014.

**Checkpoint**: User Story 1 completamente funcional y validable de forma independiente — el sistema se levanta con un solo comando.

---

## Phase 4: User Story 2 - Continuidad de comportamiento tras el cambio de motor de base de datos (Priority: P2)

**Goal**: Confirmar que iniciar sesión, marcar asistencia y administrar empleados/turnos/centros de trabajo funciona exactamente igual que antes, ahora que el Backend contenedorizado usa SQL Server.

**Independent Test**: Con el sistema levantado (US1), ejecutar los flujos existentes de cada módulo contra el Backend contenedorizado y confirmar que el resultado observado es idéntico al que se obtenía con SQLite (quickstart.md paso 3).

### Implementation for User Story 2

- [X] T016 [P] [US2] Ejecutar la suite de pruebas existente (`dotnet test tests/TimeClockSystem.Api.Tests` y `dotnet test tests/TimeClockSystem.Web.Tests`) y confirmar que pasan sin cambios, verificando que el cambio de proveedor de producción no afectó la infraestructura de pruebas (que sigue usando SQLite en archivo temporal vía `CustomWebApiFactory`). **Hallazgo real durante la ejecución**: research.md §9 asumía "sin cambios" pero se detectaron 2 incompatibilidades reales al mezclar proveedores SqlServer (prod)/Sqlite (test) en el mismo contenedor de DI, corregidas en `tests/TimeClockSystem.Api.Tests/TimeClockSystem.Api.Tests.csproj` (referencia directa a `Microsoft.EntityFrameworkCore.Sqlite`, ya no transitiva vía Infrastructure), `CustomWebApiFactory.cs` (quitar también `IDbContextOptionsConfiguration<ApplicationDbContext>`, no solo `DbContextOptions<>`, y usar el entorno "Testing"), `Program.cs` (rama `IsEnvironment("Testing")` que usa `EnsureCreatedAsync()` en vez de `MigrateAsync()`, porque las migraciones SQL Server no son sintaxis válida en SQLite) y `appsettings.Testing.json` (nuevo, valores de marcador de posición). Resultado final: 23/23 pruebas de Api.Tests y 9/9 de Web.Tests pasan.
- [X] T017 [US2] Validar siguiendo `quickstart.md` paso 3: iniciar sesión, registrar una marca de asistencia, consultar asistencias, y crear/editar un Turno o Centro de Trabajo contra el sistema contenedorizado, confirmando los mismos resultados que antes de la migración y que las acciones típicas se completan en menos de 5 segundos (SC-002, SC-007). Depende de T015.
- [X] T018 [US2] Validar el manejo de fallo de migración (edge case de la spec): provocar intencionalmente una migración incompatible (p. ej. una copia corrupta de la migración generada en T006), reconstruir y levantar el contenedor `api`, confirmar en `docker compose logs api` que el contenedor se detiene sin quedar escuchando peticiones y con un error claro (FR-010), y luego revertir el cambio intencional. Depende de T015.

**Checkpoint**: User Stories 1 y 2 funcionan de forma independiente — el comportamiento del sistema no cambió para el usuario final.

---

## Phase 5: User Story 3 - Persistencia de datos entre reinicios (Priority: P3)

**Goal**: Los datos registrados en el sistema sobreviven cuando se detiene y reinicia el contenedor de base de datos.

**Independent Test**: Registrar datos a través del sistema contenedorizado, reiniciar el contenedor de la base de datos (o el conjunto completo sin eliminar volúmenes), y confirmar que los datos siguen presentes (quickstart.md paso 4).

### Implementation for User Story 3

- [X] T019 [US3] En `docker-compose.yml`, agregar un volumen nombrado `mssql-data` montado en `/var/opt/mssql` del servicio `sqlserver`, y declararlo bajo la clave de nivel superior `volumes:` (FR-005). Depende de T012.
- [X] T020 [US3] Validar siguiendo `quickstart.md` paso 4: ejecutar `docker compose restart sqlserver` y confirmar que los datos previamente registrados (T017) siguen presentes; luego ejecutar `docker compose down` seguido de `docker compose up --build` (sin `-v`) y confirmar que los datos también sobreviven, dado que el volumen nombrado no se elimina (SC-003). Depende de T019, T017.

**Checkpoint**: Las tres historias de usuario funcionan de forma independiente y en conjunto.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentación y validaciones finales que abarcan varias historias.

- [X] T021 [P] Actualizar `README.md`: reemplazar la mención de "EF Core + SQLite" por "EF Core + SQL Server", agregar una sección "Ejecutar con Docker" que documente `cp .env.example .env` seguido de `docker compose up --build` como la forma de un solo comando, conservar la sección existente de `dotnet run` como alternativa sin Docker (FR-013), y enlazar a `specs/003-sqlserver-docker-migration/quickstart.md`.
- [X] T022 [P] Validar el manejo de secretos siguiendo `quickstart.md` paso 5 (`docker-compose.yml` solo referencia variables, `.env` ignorado por Git, `docker history` de ambas imágenes sin el valor real), confirmando que ningún secreto real aparece en archivos versionados ni en el historial de las imágenes (SC-005).
- [X] T023 [P] Validar siguiendo `quickstart.md` paso 6 que `dotnet run --project src/TimeClockSystem.Api` y `dotnet run --project src/TimeClockSystem.Web` siguen funcionando sin Docker, apuntando a una instancia SQL Server accesible localmente (por ejemplo, la misma expuesta por `docker compose` en el puerto del host) (FR-013).
- [X] T024 Validar siguiendo `quickstart.md` paso 7 (`docker compose run --rm -e Jwt__SigningKey= api`) que el contenedor falla de forma explícita y clara ante un secreto requerido faltante, en vez de arrancar con un valor inseguro embebido (FR-009, edge case de la spec). **Hallazgo real durante la ejecución**: la validación original en `Program.cs` (`?? throw`) solo cubría el caso `null`, no una variable de entorno presente pero vacía — con `Jwt__SigningKey=""` el Backend arrancaba igual, escuchando peticiones, con una clave de firma vacía. Corregido en `Program.cs` usando `string.IsNullOrWhiteSpace(...)` tanto para `Jwt:SigningKey` como para `ConnectionStrings:DefaultConnection`; confirmado que ahora falla explícitamente con `InvalidOperationException` y el contenedor se detiene sin quedar arriba.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias — puede iniciar de inmediato.
- **Foundational (Phase 2)**: Depende de Setup — BLOQUEA las tres historias de usuario.
- **User Story 1 (Phase 3)**: Depende de Foundational. No depende de otras historias.
- **User Story 2 (Phase 4)**: Depende de Foundational y de que User Story 1 esté arriba y corriendo (T015) para poder validar contra el sistema contenedorizado.
- **User Story 3 (Phase 5)**: Depende de Foundational, de `docker-compose.yml` creado en User Story 1 (T012) y de datos registrados en User Story 2 (T017) para validar persistencia.
- **Polish (Phase 6)**: Depende de que las historias deseadas estén completas (T021/T023 pueden adelantarse tras Foundational; T022/T024 requieren el sistema contenedorizado de User Story 1).

### Dentro de cada historia

- User Story 1: Dockerfiles y `.env.example` (T009–T011, en paralelo) → ensamblar `docker-compose.yml` servicio por servicio (T012 → T013 → T014, mismo archivo, secuencial) → validación (T015).
- User Story 2: pruebas automatizadas existentes (T016, en paralelo con cualquier otra cosa) → validación funcional (T017) → validación de fallo de migración (T018).
- User Story 3: agregar volumen (T019) → validación de persistencia (T020).

### Parallel Opportunities

- Fase 1: T001 y T002 en paralelo (archivos distintos).
- Fase 2: T008 en paralelo con T005/T006 (archivos distintos); T003, T004, T005, T006, T007 son secuenciales (mismo archivo o dependencia directa de estado).
- Fase 3: T009, T010, T011 en paralelo (archivos distintos); T012–T014 son secuenciales (mismo archivo `docker-compose.yml`).
- Fase 6: T021, T022, T023 en paralelo entre sí (T024 puede correr después de cualquiera, ya que reutiliza el mismo stack sin modificarlo permanentemente).

---

## Parallel Example: User Story 1

```bash
# Lanzar juntas las tareas independientes de archivo de User Story 1:
Task: "Crear src/TimeClockSystem.Api/Dockerfile multi-stage (T009)"
Task: "Crear src/TimeClockSystem.Web/Dockerfile multi-stage (T010)"
Task: "Crear .env.example en la raíz del repositorio (T011)"

# Luego, secuencialmente sobre el mismo docker-compose.yml:
Task: "Agregar servicio sqlserver (T012)"
Task: "Agregar servicio api (T013)"
Task: "Agregar servicio web (T014)"
```

---

## Implementation Strategy

### MVP First (User Story 1 solamente)

1. Completar Fase 1: Setup
2. Completar Fase 2: Foundational (CRÍTICO — bloquea las tres historias)
3. Completar Fase 3: User Story 1
4. **DETENERSE y VALIDAR**: probar User Story 1 de forma independiente (quickstart.md pasos 1–2)
5. Demostrar el sistema levantándose con un solo comando

### Incremental Delivery

1. Setup + Foundational → motor SQL Server funcionando fuera de Docker
2. + User Story 1 → probar de forma independiente → demo del arranque con un solo comando (MVP)
3. + User Story 2 → probar de forma independiente → demo de continuidad de comportamiento
4. + User Story 3 → probar de forma independiente → demo de persistencia entre reinicios
5. + Polish → documentación y validaciones finales de seguridad/compatibilidad

## Notes

- [P] = archivos distintos, sin dependencias entre sí
- [Story] mapea cada tarea a su historia de usuario para trazabilidad
- No se crean archivos de test nuevos: la validación se apoya en `quickstart.md` y en la suite xUnit existente
- Confirmar cada checkpoint antes de avanzar a la siguiente fase
- Evitar: mezclar cambios de varias historias en una misma tarea, editar `docker-compose.yml` en paralelo desde distintas tareas
