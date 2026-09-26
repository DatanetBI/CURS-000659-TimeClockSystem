---

description: "Task list for TimeClockSystem v1.0 (6 módulos)"
---

# Tasks: TimeClockSystem v1.0 — Gestión de Asistencias

**Input**: Design documents from `/specs/001-timeclock-spec-v1/` (`plan.md`, `spec.md`, `research.md`,
`data-model.md`, `contracts/`, `quickstart.md`)

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/web-endpoints.md`,
`quickstart.md`

**Alcance**: Esta lista cubre únicamente los **6 módulos de v1.0** decididos en `plan.md`
(§"Alcance de v1.0 vs. v1.1"). Cada módulo se etiqueta como `[M1]`...`[M6]`, en el mismo orden de
construcción del plan — deliberadamente **no** se usa el prefijo `US` para evitar que colisione con la
numeración `US1`-`US7` de las historias de usuario de `spec.md` (que describe el producto completo, no
los módulos de esta entrega). Incidencias, Exportación/Importación, offline, motor de pre-nómina, panel
de presencia en vivo, reportes/KPIs y auditoría quedan fuera de esta lista (diferidos a v1.1).

**Tests**: Se incluyen tareas de prueba ligeras (una prueba de integración por módulo, más algunas
unitarias) porque `plan.md`/`research.md` ya comprometen xUnit + `WebApplicationFactory` como parte de la
solución; no es un enfoque TDD estricto de "test primero" para cada tarea.

## Format: `[ID] [P?] [Módulo?] Description`

- **[P]**: Se puede ejecutar en paralelo (archivos distintos, sin dependencias pendientes)
- **[Módulo]**: `M1` a `M6`, según la tabla de `plan.md`
- Cada descripción incluye la ruta de archivo exacta

## Path Conventions

Proyecto único (`plan.md` §Project Structure):

- `src/TimeClockSystem.Web/` — proyecto ASP.NET Core (Areas, Domain, Views, Infrastructure, wwwroot)
- `tests/TimeClockSystem.Tests/` — Unit, Integration

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialización del proyecto.

- [X] T001 Crear la estructura de carpetas de `src/TimeClockSystem.Web/` y `tests/TimeClockSystem.Tests/` según `plan.md` §Project Structure
- [X] T002 Inicializar el proyecto ASP.NET Core 10 (MVC) con las dependencias de `plan.md` (EF Core SQLite, ASP.NET Core Identity, Bootstrap 5 vía CDN) en `src/TimeClockSystem.Web/TimeClockSystem.Web.csproj`
- [X] T003 [P] Configurar `.editorconfig` y `dotnet format` en la raíz del repositorio
- [X] T004 [P] Inicializar el proyecto de pruebas xUnit + `Microsoft.AspNetCore.Mvc.Testing` en `tests/TimeClockSystem.Tests/TimeClockSystem.Tests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura común que TODOS los módulos necesitan.

**⚠️ CRÍTICO**: Ningún módulo (Fase 3+) puede empezar hasta que esta fase esté completa.

- [X] T005 Crear `ApplicationDbContext` (EF Core, proveedor SQLite) y la migración inicial en `src/TimeClockSystem.Web/Infrastructure/Data/ApplicationDbContext.cs`
- [X] T006 [P] Configurar ASP.NET Core Identity con los roles `Empleado` y `Administrador` en `src/TimeClockSystem.Web/Infrastructure/Identity/IdentityConfiguration.cs` (research.md §3)
- [X] T007 [P] Crear el layout base Razor con Bootstrap 5 (CDN), navegación responsiva y cultura español (México) en `src/TimeClockSystem.Web/Views/Shared/_Layout.cshtml`
- [X] T008 Configurar manejo global de errores y logging en `src/TimeClockSystem.Web/Program.cs`
- [X] T009 [P] Configurar `appsettings.json` y variables de entorno para la cadena de conexión (sin secretos en código — Principio V) en `src/TimeClockSystem.Web/appsettings.json` y `Program.cs`
- [X] T010 Crear el orquestador `DbSeeder` (se ejecuta al iniciar si la base está vacía) en `src/TimeClockSystem.Web/Infrastructure/Data/DbSeeder.cs`, sembrando primero los 2 usuarios Administrador de ejemplo (`plan.md` §Datos mock)

**Checkpoint**: Con la Fase 2 completa, el Módulo 1 puede empezar.

---

## Phase 3: Módulo 1 — Centros de trabajo (M1) 🎯 Primer incremento probable

**Goal**: El administrador puede crear, editar y listar centros de trabajo (geofence).

**Independent Test**: Iniciar sesión como Administrador, crear un centro de trabajo (nombre, coordenadas,
radio) y verlo aparecer en la lista — `quickstart.md` §Módulo 1.

- [X] T011 [P] [M1] Crear la entidad `CentroTrabajo` en `src/TimeClockSystem.Web/Domain/CentroTrabajo.cs` (data-model.md)
- [X] T012 [M1] Agregar el `DbSet<CentroTrabajo>` y su migración a `ApplicationDbContext` (depende de T011, T005)
- [X] T013 [M1] Extender `DbSeeder` con los 3 centros de trabajo de ejemplo (depende de T012, T010)
- [X] T014 [M1] Implementar `CentrosTrabajoController` (CRUD) en `src/TimeClockSystem.Web/Areas/CentrosTrabajo/CentrosTrabajoController.cs`
- [X] T015 [P] [M1] Crear las vistas Razor de CRUD (Index/Create/Edit) en `src/TimeClockSystem.Web/Areas/CentrosTrabajo/Views/`
- [X] T016 [M1] Agregar validación (radio > 0, coordenadas requeridas) en `CentrosTrabajoController`
- [X] T017 [M1] Restringir el acceso al rol `Administrador` en `CentrosTrabajoController`
- [X] T018 [P] [M1] Prueba de integración: crear/editar/listar un centro de trabajo en `tests/TimeClockSystem.Tests/Integration/CentrosTrabajoTests.cs`

**Checkpoint**: Módulo 1 funcional y probable de forma independiente.

---

## Phase 4: Módulo 2 — Empleados y credenciales de marcaje (M2)

**Goal**: El administrador puede dar de alta empleados, asociarlos a un centro de trabajo y asignarles un
PIN de marcaje. Este módulo solo gestiona los datos del empleado y del PIN (FR-004); la validación del PIN
al momento de marcar (FR-002/FR-003) se implementa en el Módulo 5.

**Independent Test**: Crear un empleado, asociarlo a un centro de trabajo del Módulo 1, asignarle un PIN —
`quickstart.md` §Módulo 2.

- [X] T019 [P] [M2] Crear la entidad `Empleado` en `src/TimeClockSystem.Web/Domain/Empleado.cs` (referencia a `CentroTrabajo`, incluye `ConsentimientoGeolocalizacion`; depende de T011; data-model.md)
- [X] T020 [P] [M2] Crear la entidad `CredencialDeMarcaje` (PinHash) en `src/TimeClockSystem.Web/Domain/CredencialDeMarcaje.cs`
- [X] T021 [M2] Agregar los `DbSet` de `Empleado` y `CredencialDeMarcaje` y su migración (depende de T019, T020)
- [X] T022 [M2] Extender `DbSeeder` con los ~12 empleados de ejemplo, cada uno con PIN ya asignado y consentimiento de geolocalización otorgado (depende de T021, T013)
- [X] T023 [M2] Implementar `EmpleadosController` (CRUD) en `src/TimeClockSystem.Web/Areas/Empleados/EmpleadosController.cs`
- [X] T024 [P] [M2] Crear las vistas Razor de CRUD de empleados en `src/TimeClockSystem.Web/Areas/Empleados/Views/`
- [X] T025 [M2] Implementar la acción "asignar/restablecer PIN" (hash vía `PasswordHasher<T>`) en `src/TimeClockSystem.Web/Areas/Empleados/PinController.cs` (FR-004)
- [X] T026 [M2] Agregar validación: `NumeroEmpleado` único, PIN numérico de 4 a 6 dígitos
- [X] T027 [M2] Restringir el acceso al rol `Administrador` en los controladores del módulo
- [X] T028 [P] [M2] Prueba de integración: alta de empleado + asignación de PIN en `tests/TimeClockSystem.Tests/Integration/EmpleadosTests.cs`

**Checkpoint**: Módulos 1 y 2 funcionan de forma independiente y juntos.

---

## Phase 5: Módulo 3 — Turnos y asignación de turnos (M3)

**Goal**: El administrador puede crear turnos y asignarlos a empleados, individual o masivamente. La
exclusión de empleados con una "restricción horaria individual aprobada" (FR-014) no tiene efecto en
v1.0: esa aprobación depende del módulo de Incidencias, diferido a v1.1; el resumen de asignados/excluidos
existe (T036), pero en v1.0 nunca excluirá a nadie por esa causa.

**Independent Test**: Crear un turno con tolerancia, asignarlo a un empleado del Módulo 2 y a un grupo —
`quickstart.md` §Módulo 3.

- [X] T029 [P] [M3] Crear la entidad `Turno` en `src/TimeClockSystem.Web/Domain/Turno.cs`
- [X] T030 [P] [M3] Crear la entidad `AsignacionTurno` en `src/TimeClockSystem.Web/Domain/AsignacionTurno.cs` (referencia a `Empleado`, depende de T019)
- [X] T031 [M3] Agregar los `DbSet` de `Turno` y `AsignacionTurno` y su migración (depende de T029, T030)
- [X] T032 [M3] Extender `DbSeeder` con 2-3 turnos de ejemplo y sus asignaciones (depende de T031, T022)
- [X] T033 [M3] Implementar `TurnosController` (CRUD de turno) en `src/TimeClockSystem.Web/Areas/Turnos/TurnosController.cs`
- [X] T034 [P] [M3] Crear las vistas Razor de CRUD de turnos en `src/TimeClockSystem.Web/Areas/Turnos/Views/`
- [X] T035 [M3] Implementar la asignación individual de turno a un empleado (acción + vista)
- [X] T036 [M3] Implementar la asignación masiva a un grupo de empleados, con resumen de asignados/excluidos (FR-013/FR-014 — ver nota de alcance arriba: en v1.0 el conteo de excluidos siempre será cero)
- [X] T037 [M3] Agregar validación: `ToleranciaMinutos` ≥ 0 y menor que la duración del turno (FR-015)
- [X] T038 [P] [M3] Prueba de integración: crear turno + asignación masiva en `tests/TimeClockSystem.Tests/Integration/TurnosTests.cs`

**Checkpoint**: Módulos 1 a 3 funcionan de forma independiente y juntos.

---

## Phase 6: Módulo 4 — Días festivos (M4)

**Goal**: El administrador mantiene un calendario de días festivos.

**Independent Test**: Dar de alta una fecha festiva y verla en el catálogo — `quickstart.md` §Módulo 4.

- [X] T039 [P] [M4] Crear la entidad `DiaFestivo` en `src/TimeClockSystem.Web/Domain/DiaFestivo.cs`
- [X] T040 [M4] Agregar el `DbSet<DiaFestivo>` y su migración (depende de T039)
- [X] T041 [M4] Extender `DbSeeder` con 2-3 fechas festivas de ejemplo (depende de T040)
- [X] T042 [M4] Implementar `DiasFestivosController` (CRUD) en `src/TimeClockSystem.Web/Areas/DiasFestivos/DiasFestivosController.cs`
- [X] T043 [P] [M4] Crear las vistas Razor de CRUD de días festivos en `src/TimeClockSystem.Web/Areas/DiasFestivos/Views/`
- [X] T044 [P] [M4] Prueba de integración: alta de un día festivo en `tests/TimeClockSystem.Tests/Integration/DiasFestivosTests.cs`

**Checkpoint**: Módulos 1 a 4 funcionan de forma independiente y juntos.

---

## Phase 7: Módulo 5 — Registro de asistencias (M5)

**Goal**: Un empleado registra su marca de entrada/salida/receso en línea (portal o PIN de kiosco), con
validación de geofence y de consentimiento de geolocalización. Este módulo implementa la validación real
de FR-002/FR-003 (el Módulo 2 solo gestiona el dato del PIN).

**Independent Test**: Marcar entrada dentro del geofence, verla confirmada de inmediato; repetir fuera del
geofence, con PIN incorrecto, y sin consentimiento de geolocalización registrado — `quickstart.md`
§Módulo 5.

- [X] T045 [P] [M5] Crear la entidad `Marca` en `src/TimeClockSystem.Web/Domain/Marca.cs` (referencia a `Empleado`, depende de T019)
- [X] T046 [M5] Agregar el `DbSet<Marca>` y su migración (depende de T045)
- [X] T047 [M5] Crear la interfaz `IBiometricVerificationProvider` con una implementación por defecto sin validación real, en `src/TimeClockSystem.Web/Domain/IBiometricVerificationProvider.cs` (FR-007 — punto de integración diferido)
- [X] T048 [M5] Implementar `GeofenceValidator` (distancia Haversine contra `CentroTrabajo`) en `src/TimeClockSystem.Web/Domain/GeofenceValidator.cs` (depende de T011)
- [X] T049 [M5] Implementar `ConsentimientoValidator`: bloquear una marca con geolocalización si el empleado no tiene `ConsentimientoGeolocalizacion` en `true` en `src/TimeClockSystem.Web/Domain/ConsentimientoValidator.cs` (depende de T019) (FR-046, CL9)
- [X] T050 [M5] Implementar `MarcajeController`: registrar marca desde una sesión de portal autenticada (depende de T046, T048, T049)
- [X] T051 [M5] Implementar `PinMarcajeController`: registrar marca por número de empleado + PIN, modo kiosco sin sesión completa (depende de T025, T046) (FR-002/FR-003)
- [X] T052 [M5] Agregar la regla de rechazo de entrada duplicada si ya existe una entrada abierta (FR-005)
- [X] T053 [M5] Registrar un evento de seguridad cuando una marca se rechaza por geofence (FR-010)
- [X] T054 [P] [M5] Crear las vistas Razor de marcaje (portal y kiosco), responsivas, en `src/TimeClockSystem.Web/Areas/Marcaje/Views/`
- [X] T055 [M5] Extender `DbSeeder` con marcas de ejemplo de días anteriores (depende de T046, T041)
- [X] T056 [P] [M5] Prueba unitaria de `GeofenceValidator` (dentro/fuera del radio) en `tests/TimeClockSystem.Tests/Unit/GeofenceValidatorTests.cs`
- [X] T057 [P] [M5] Prueba de integración: marcar entrada/salida, entrada duplicada, fuera de geofence, PIN inválido, sin consentimiento de geolocalización en `tests/TimeClockSystem.Tests/Integration/MarcajeTests.cs`

**Checkpoint**: Módulos 1 a 5 funcionan de forma independiente y juntos — este es el primer punto donde
un empleado real puede usar la aplicación de principio a fin.

---

## Phase 8: Módulo 6 — Portal de consulta de asistencias (M6)

**Goal**: El administrador filtra y consulta las marcas registradas, viendo si cada una fue puntual/tardía
y si cayó en día festivo.

**Independent Test**: Filtrar por empleado, fecha y centro de trabajo, y verificar los indicadores —
`quickstart.md` §Módulo 6.

- [X] T058 [M6] Implementar `PuntualidadCalculator` (compara `Marca` contra el `Turno` asignado ese día + tolerancia) en `src/TimeClockSystem.Web/Domain/PuntualidadCalculator.cs` (depende de T045, T031)
- [X] T059 [M6] Implementar `ConsultaAsistenciasController` con filtros por empleado, fecha y centro de trabajo en `src/TimeClockSystem.Web/Areas/ConsultaAsistencias/ConsultaAsistenciasController.cs` (depende de T058)
- [X] T060 [P] [M6] Crear la vista Razor de resultados (tabla filtrable, responsiva) en `src/TimeClockSystem.Web/Areas/ConsultaAsistencias/Views/`
- [X] T061 [M6] Señalar en los resultados si el día correspondiente es festivo (depende de T039)
- [X] T062 [M6] Restringir el acceso al rol `Administrador`
- [X] T063 [P] [M6] Prueba de integración: filtros por empleado/fecha/centro y verificación de indicadores en `tests/TimeClockSystem.Tests/Integration/ConsultaAsistenciasTests.cs`

**Checkpoint**: Los 6 módulos de v1.0 funcionan de forma independiente y en conjunto.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Mejoras que afectan a los 6 módulos.

- [X] T064 [P] Revisar que toda la interfaz esté en español de México en los 6 módulos (Principio II de la constitución)
- [X] T065 [P] Ejecutar `quickstart.md` módulo por módulo y confirmar manualmente cada criterio de aceptación (Principio IV)
- [X] T066 Revisar que ningún secreto ni cadena de conexión esté en el código fuente; confirmar el uso de variables de entorno (Principio V)
- [X] T067 [P] Pulir mensajes de validación y manejo de errores en los 6 módulos
- [X] T068 Ejecutar `dotnet test` completo y confirmar que toda la suite pasa
- [X] T069 [P] Prueba de carga ligera: sembrar 500 empleados de ejemplo y medir el tiempo de respuesta del marcaje y de la consulta (SC-010) en `tests/TimeClockSystem.Tests/Integration/EscalaTests.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Fase 1)**: sin dependencias.
- **Foundational (Fase 2)**: depende de Setup — bloquea todos los módulos.
- **Módulos (Fase 3-8)**: cada uno depende de la Fase 2 y, específicamente, de los módulos anteriores que
  proveen sus entidades base (ver tabla abajo) — a diferencia de historias de usuario típicamente
  independientes entre sí, aquí los módulos son **secuenciales por diseño** (así lo pidió el negocio, para
  poder probar la app paso a paso).
- **Polish (Fase 9)**: depende de que los 6 módulos estén completos.

### Dependencias entre módulos

| Módulo | Depende de |
|---|---|
| M1 — Centros de trabajo | Fase 2 (Foundational) |
| M2 — Empleados y credenciales | M1 (el empleado necesita un centro de trabajo) |
| M3 — Turnos y asignación | M2 (se asignan turnos a empleados existentes) |
| M4 — Días festivos | Fase 2 (independiente de M1-M3, pero se construye en este punto por orden de negocio) |
| M5 — Registro de asistencias | M1, M2, M3 (geofence, empleado+PIN+consentimiento, turno asignado) |
| M6 — Portal de consulta | M3, M4, M5 (necesita turnos, festivos y marcas ya existentes) |

### Dentro de cada módulo

- Entidades (modelos) antes que el `DbContext`/migración.
- `DbContext`/migración antes que el `DbSeeder` del módulo.
- Controlador antes que las vistas que lo consumen (aunque las vistas están marcadas `[P]` porque son
  archivos distintos).
- Validaciones y autorización al final de la implementación del módulo.
- Pruebas de integración al final de cada módulo (confirman el módulo completo).

### Parallel Opportunities

- Todas las tareas de Setup marcadas `[P]` se pueden hacer en paralelo.
- Dentro de la Fase 2, T006, T007 y T009 se pueden hacer en paralelo.
- Dentro de cada módulo, las entidades marcadas `[P]` (cuando hay más de una) y las vistas se pueden
  hacer en paralelo entre sí, pero no en paralelo con el módulo siguiente (por las dependencias de la
  tabla anterior).

---

## Parallel Example: Módulo 2 (Empleados y credenciales)

```bash
# Las dos entidades del módulo se pueden crear en paralelo:
Task: "Crear la entidad Empleado en src/TimeClockSystem.Web/Domain/Empleado.cs"
Task: "Crear la entidad CredencialDeMarcaje en src/TimeClockSystem.Web/Domain/CredencialDeMarcaje.cs"

# Una vez implementado el controlador, las vistas y la prueba de integración son independientes entre sí:
Task: "Crear las vistas Razor de CRUD de empleados en src/TimeClockSystem.Web/Areas/Empleados/Views/"
Task: "Prueba de integración: alta de empleado + asignación de PIN en tests/TimeClockSystem.Tests/Integration/EmpleadosTests.cs"
```

---

## Implementation Strategy

### Primer incremento demostrable

1. Completar Fase 1 (Setup) y Fase 2 (Foundational).
2. Completar Módulo 1 (Centros de trabajo) — ya es demostrable, pero de valor limitado por sí solo (solo
   catálogo, sin empleados ni marcaje todavía).
3. **El primer incremento con valor de negocio real es completar hasta el Módulo 5** (Registro de
   asistencias): en ese punto un empleado ya puede marcar su asistencia de principio a fin. Validar y, si
   se desea, publicar/demostrar en ese punto.
4. Agregar el Módulo 6 (Portal de consulta) para que el administrador pueda ver lo que los empleados ya
   están marcando.

### Entrega incremental

1. Setup + Foundational → base lista.
2. Módulo 1 → probar → (opcional) desplegar.
3. Módulo 2 → probar → (opcional) desplegar.
4. Módulo 3 → probar → (opcional) desplegar.
5. Módulo 4 → probar → (opcional) desplegar.
6. Módulo 5 → probar → **primer hito de valor real** → desplegar/demostrar.
7. Módulo 6 → probar → desplegar/demostrar.

Cada módulo suma valor sin romper los anteriores, siguiendo exactamente el orden que pidió el negocio en
`plan.md` §"Orden de construcción incremental".

---

## Notes

- `[P]` = archivos distintos, sin dependencias pendientes.
- La etiqueta de módulo (`[M1]`...`[M6]`) mapea cada tarea a la tabla de alcance de `plan.md`; se usa el
  prefijo `M` en vez de `US` deliberadamente, para no colisionar con la numeración `US1`-`US7` de las
  historias de usuario de `spec.md`.
- A diferencia del patrón típico de historias independientes entre sí, estos 6 módulos son
  intencionalmente secuenciales (dependencia real de datos: turnos necesitan empleados, marcaje necesita
  turnos y empleados, consulta necesita marcas) — así lo pidió el negocio para poder probar la app paso a
  paso.
- Hacer commit después de cada tarea o grupo lógico de tareas.
- Verificar en cada checkpoint que el módulo recién completado es usable de forma independiente antes de
  continuar con el siguiente.
- Evitar: tareas vagas, conflictos de archivo entre tareas paralelas, y construir un módulo antes que el
  módulo del que depende.
