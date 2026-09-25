---

description: "Task list for feature implementation"
---

# Tasks: Separación de TimeClockSystem en Backend WebAPI y Frontend MVC

**Input**: Design documents from `/specs/002-split-backend-frontend/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: No se generan tareas de TDD (tests-first) porque no fueron solicitadas explícitamente en
la spec. Sí se incluyen tareas para mantener compilando/pasando la suite de pruebas ya existente
(regresión) y para el smoke-test del nuevo host de la API, dado que ambos son trabajo de migración
necesario, no cobertura nueva especulativa.

**Organization**: Las tareas están agrupadas por historia de usuario (US1, US2, US3, según
`spec.md`) para permitir implementación y prueba independientes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: Historia de usuario a la que pertenece (US1, US2, US3)
- Cada tarea incluye la ruta de archivo exacta

## Path Conventions

Arquitectura Web (frontend + backend), según `plan.md`:
- `src/TimeClockSystem.Domain/`, `src/TimeClockSystem.Application/`, `src/TimeClockSystem.Infrastructure/`, `src/TimeClockSystem.Api/` (Backend, Clean Architecture)
- `src/TimeClockSystem.Web/` (Frontend MVC existente)
- `tests/TimeClockSystem.Api.Tests/`, `tests/TimeClockSystem.Web.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Crear los proyectos nuevos y ajustar la solución antes de mover código.

- [ ] T001 Agregar los 4 proyectos nuevos del Backend a `TimeClockSystem.slnx` con las referencias de dependencia Domain ← Application ← Infrastructure ← Api
- [ ] T002 [P] Scaffold `src/TimeClockSystem.Domain/TimeClockSystem.Domain.csproj` (net10.0, class library, sin dependencias de paquete)
- [ ] T003 [P] Scaffold `src/TimeClockSystem.Application/TimeClockSystem.Application.csproj` (net10.0, class library, referencia a Domain)
- [ ] T004 [P] Scaffold `src/TimeClockSystem.Infrastructure/TimeClockSystem.Infrastructure.csproj` (net10.0, referencia a Application; agrega `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`)
- [ ] T005 Scaffold `src/TimeClockSystem.Api/TimeClockSystem.Api.csproj` (`Microsoft.NET.Sdk.Web`, referencia a Application + Infrastructure; agrega `Swashbuckle.AspNetCore`)
- [ ] T006 [P] Renombrar `tests/TimeClockSystem.Tests` a `tests/TimeClockSystem.Web.Tests` (csproj y namespace), conservando su referencia a `TimeClockSystem.Web`
- [ ] T007 [P] Crear `tests/TimeClockSystem.Api.Tests/TimeClockSystem.Api.Tests.csproj` (xUnit + `Microsoft.AspNetCore.Mvc.Testing`, referencia a `TimeClockSystem.Api`)
- [ ] T008 Actualizar `TimeClockSystem.slnx` para incluir los proyectos nuevos/renombrados en las carpetas `/src/` y `/tests/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Construir el Backend completo (Domain, Application, Infrastructure, Api) — ninguna
historia de usuario puede probarse sin esto, ya que las 3 dependen de que la API exista y funcione.

**⚠️ CRITICAL**: Ninguna tarea de historia de usuario puede iniciar hasta terminar esta fase.

### Domain

- [ ] T009 [P] Mover `Empleado`, `CentroTrabajo`, `Turno`, `AsignacionTurno`, `DiaFestivo`, `Marca`, `CredencialDeMarcaje` desde `src/TimeClockSystem.Web/Domain/` a `src/TimeClockSystem.Domain/`, actualizando el namespace a `TimeClockSystem.Domain` (data-model.md)
- [ ] T010 [P] Mover `PuntualidadCalculator`, `GeofenceValidator`, `ConsentimientoValidator`, `IBiometricVerificationProvider` y `NullBiometricVerificationProvider` a `src/TimeClockSystem.Domain/`, sin cambiar su comportamiento (FR-005)
- [ ] T011 [P] Crear `RegistroAuditoria` (enum `EventoAuditoria`: `InicioSesionExitoso`, `InicioSesionFallido`, `MarcajeRechazado`, `AccesoDenegadoPorRol`) en `src/TimeClockSystem.Domain/RegistroAuditoria.cs` (data-model.md, FR-011)

### Application

- [ ] T012 Definir las interfaces de repositorio/servicio (`IEmpleadoRepository`, `ICentroTrabajoRepository`, `ITurnoRepository`, `IAsignacionTurnoRepository`, `IDiaFestivoRepository`, `IMarcaRepository`, `ICredencialRepository`, `ITokenService`, `IAuditLogService`) en `src/TimeClockSystem.Application/Abstractions/`
- [ ] T013 Implementar `AutenticarUseCase` en `src/TimeClockSystem.Application/Auth/AutenticarUseCase.cs` (contracts/auth.md; registra `InicioSesionExitoso`/`InicioSesionFallido` vía `IAuditLogService`; incluye el claim `empleadoId` en el token cuando el `ApplicationUser` está vinculado a un Empleado — data-model.md, Usuario/Rol)
- [ ] T014 Adaptar `MarcajeService` existente a `RegistrarMarcaUseCase` en `src/TimeClockSystem.Application/Marcaje/RegistrarMarcaUseCase.cs`, conservando sus reglas (geofence, consentimiento, entrada duplicada) y registrando `MarcajeRechazado` en cada rechazo (contracts/marcaje.md, FR-011)
- [ ] T015 [P] Implementar los casos de uso de Empleados (listar, obtener, crear, actualizar, credencial) en `src/TimeClockSystem.Application/Empleados/` (contracts/empleados.md)
- [ ] T016 [P] Implementar los casos de uso CRUD de CentrosTrabajo en `src/TimeClockSystem.Application/CentrosTrabajo/` (contracts/centros-trabajo.md)
- [ ] T017 [P] Implementar los casos de uso CRUD de Turnos y AsignacionesTurno en `src/TimeClockSystem.Application/Turnos/` (contracts/turnos-y-asignaciones.md)
- [ ] T018 [P] Implementar los casos de uso CRUD de DiasFestivos en `src/TimeClockSystem.Application/DiasFestivos/` (contracts/dias-festivos.md)
- [ ] T019 Implementar `ConsultarAsistenciasUseCase` en `src/TimeClockSystem.Application/ConsultaAsistencias/ConsultarAsistenciasUseCase.cs` (valida que un Empleado solo consulte su propio historial; registra `AccesoDenegadoPorRol` en caso contrario — contracts/consulta-asistencias.md)
- [ ] T020 Implementar `ConsultarAuditoriaUseCase` (paginado, filtro por fecha/usuario) en `src/TimeClockSystem.Application/Auditoria/ConsultarAuditoriaUseCase.cs` (contracts/auditoria.md, FR-011a)

### Infrastructure

- [ ] T021 Mover `ApplicationDbContext`, `DbSeeder` y `Migrations/` desde `src/TimeClockSystem.Web/Infrastructure/Data/` a `src/TimeClockSystem.Infrastructure/Data/`, agregando `DbSet<RegistroAuditoria>`
- [ ] T022 Agregar la migración EF Core aditiva para la tabla `RegistrosAuditorias` en `src/TimeClockSystem.Infrastructure/Data/Migrations/` (research.md #7, FR-010: cero pérdida de datos existentes)
- [ ] T023 Mover `ApplicationUser`, `Roles` e `IdentityConfiguration` desde `src/TimeClockSystem.Web/Infrastructure/Identity/` a `src/TimeClockSystem.Infrastructure/Identity/`
- [ ] T024 [P] Implementar las clases de `src/TimeClockSystem.Infrastructure/Repositories/` que satisfacen las interfaces de T012 usando `ApplicationDbContext`
- [ ] T025 Implementar `JwtTokenService` (`ITokenService`) en `src/TimeClockSystem.Infrastructure/Auth/JwtTokenService.cs`: emite un token de expiración fija con claims de rol, sin renovación silenciosa (FR-002, research.md #2)
- [ ] T026 Implementar `AuditLogService` (`IAuditLogService`) en `src/TimeClockSystem.Infrastructure/Auditoria/AuditLogService.cs`, persistiendo `RegistroAuditoria` vía `ApplicationDbContext` (FR-011)
- [ ] T027 [P] Mover la implementación de `IBiometricVerificationProvider` a `src/TimeClockSystem.Infrastructure/Biometria/`, sin cambiar su comportamiento (FR-005)

### Api (host)

- [ ] T028 Configurar `src/TimeClockSystem.Api/Program.cs`: DI de repositorios/casos de uso, `AddDbContext`, Identity core (sin cookies), `AddAuthentication().AddJwtBearer(...)`, políticas de autorización por rol, Swashbuckle, migración de BD al iniciar
- [ ] T029 Implementar `AuthController` (`POST /api/auth/login`, `POST /api/auth/logout`) en `src/TimeClockSystem.Api/Controllers/AuthController.cs` (contracts/auth.md)
- [ ] T030 [P] Implementar `EmpleadosController` en `src/TimeClockSystem.Api/Controllers/EmpleadosController.cs` (contracts/empleados.md)
- [ ] T031 [P] Implementar `CentrosTrabajoController` en `src/TimeClockSystem.Api/Controllers/CentrosTrabajoController.cs` (contracts/centros-trabajo.md)
- [ ] T032 [P] Implementar `TurnosController` y `AsignacionesTurnoController` en `src/TimeClockSystem.Api/Controllers/` (contracts/turnos-y-asignaciones.md)
- [ ] T033 [P] Implementar `DiasFestivosController` en `src/TimeClockSystem.Api/Controllers/DiasFestivosController.cs` (contracts/dias-festivos.md)
- [ ] T034 [P] Implementar `MarcajeController` (`POST /api/marcaje`, `POST /api/marcaje/pin`) en `src/TimeClockSystem.Api/Controllers/MarcajeController.cs`; para el canal `PortalWeb` resuelve el Empleado leyendo el claim `empleadoId` del token, no por nombre de usuario (contracts/marcaje.md, contracts/auth.md)
- [ ] T035 [P] Implementar `ConsultaAsistenciasController` en `src/TimeClockSystem.Api/Controllers/ConsultaAsistenciasController.cs`; para rol Empleado, resuelve y valida el `empleadoId` solicitado contra el claim `empleadoId` del token (contracts/consulta-asistencias.md, contracts/auth.md)
- [ ] T036 [P] Implementar `AuditoriaController` (`GET /api/auditoria`, solo Administrador) en `src/TimeClockSystem.Api/Controllers/AuditoriaController.cs` (contracts/auditoria.md)
- [ ] T037 Aplicar `[Authorize(Roles = ...)]` en cada controller según lo definido en `contracts/`, asegurando que el Backend valide los permisos de forma independiente del Frontend (FR-003)
- [ ] T038 Configurar la clave de firma del token y la cadena de conexión vía `appsettings.Development.json` (fuera de control de versiones) y variables de entorno en producción, sin secretos en el código (Principio V) en `src/TimeClockSystem.Api/appsettings*.json`

**Checkpoint**: El Backend (`TimeClockSystem.Api`) es funcional de forma independiente — puede
probarse con Swagger sin el Frontend. Todas las historias de usuario pueden comenzar.

---

## Phase 3: User Story 1 - Continuidad de uso sin cambios para los usuarios finales (Priority: P1) 🎯 MVP

**Goal**: El Frontend deja de usar EF Core directamente y consume el Backend por HTTP, sin que
Administrador ni Empleado perciban ningún cambio de pantallas o flujos.

**Independent Test**: Ejecutar los escenarios existentes de cada Area (Empleados, CentrosTrabajo,
Turnos, AsignacionTurno, DiasFestivos, Marcaje, ConsultaAsistencias) contra la nueva arquitectura y
confirmar que el resultado observado por el usuario es idéntico al de antes de la separación
(quickstart.md sección 2).

### Implementation for User Story 1

- [ ] T039 [US1] Quitar los paquetes y el uso de EF Core/Identity de `src/TimeClockSystem.Web/TimeClockSystem.Web.csproj` y `Program.cs`
- [ ] T040 [US1] Agregar `IHttpClientFactory` + cliente base tipado (`BaseAddress` desde configuración) en `src/TimeClockSystem.Web/Infrastructure/ApiClients/ApiClientBase.cs`
- [ ] T041 [US1] Implementar el puente de autenticación en `src/TimeClockSystem.Web/Infrastructure/Auth/`: al iniciar sesión llama a `POST /api/auth/login`, guarda el token recibido como claim cifrado dentro de la cookie de autenticación existente, y un `DelegatingHandler` lo adjunta como `Authorization: Bearer` en cada llamada saliente (FR-002a)
- [ ] T042 [US1] Actualizar el Controller de Cuenta (`IniciarSesion`/`CerrarSesion`) para usar el puente de autenticación de T041 en vez de `SignInManager`
- [ ] T043 [P] [US1] Reconectar los Controllers del Area Empleados a `EmpleadosApiClient` en `src/TimeClockSystem.Web/Areas/Empleados/Controllers/`
- [ ] T044 [P] [US1] Reconectar los Controllers del Area CentrosTrabajo a `CentrosTrabajoApiClient` en `src/TimeClockSystem.Web/Areas/CentrosTrabajo/Controllers/`
- [ ] T045 [P] [US1] Reconectar los Controllers de las Areas Turnos y AsignacionTurno a `TurnosApiClient`/`AsignacionesTurnoApiClient` en `src/TimeClockSystem.Web/Areas/Turnos/Controllers/`
- [ ] T046 [P] [US1] Reconectar el Controller del Area DiasFestivos a `DiasFestivosApiClient` en `src/TimeClockSystem.Web/Areas/DiasFestivos/Controllers/`
- [ ] T047 [US1] Reconectar `MarcajeController` y `PinMarcajeController` a `MarcajeApiClient`, con un único intento sin reintento automático (FR-012) en `src/TimeClockSystem.Web/Areas/Marcaje/Controllers/`
- [ ] T048 [US1] Reconectar el Controller del Area ConsultaAsistencias a `ConsultaAsistenciasApiClient` en `src/TimeClockSystem.Web/Areas/ConsultaAsistencias/Controllers/`
- [ ] T049 [US1] Agregar manejo de errores claro y no técnico ante fallas o indisponibilidad del Backend en todos los Controllers reconectados (FR-009)
- [ ] T050 [US1] Agregar la nueva Area `Auditoria` (Controller + vista de solo lectura, solo Administrador, filtro por fecha/usuario) consumiendo `AuditoriaApiClient` en `src/TimeClockSystem.Web/Areas/Auditoria/` (FR-011a)
- [ ] T051 [US1] Confirmar que la suite `tests/TimeClockSystem.Web.Tests` sigue compilando y pasando contra los Controllers reconectados (regresión, FR-005)
- [ ] T052 [US1] Validar manualmente `quickstart.md` sección 2 (Administrador y Empleado, las 7 Areas) (SC-001)
- [ ] T053 [US1] Validar manualmente `quickstart.md` secciones 4 y 5 (auditoría y cero pérdida de datos históricos) (SC-006, FR-010)

**Checkpoint**: User Story 1 completamente funcional y probable de forma independiente — MVP listo.

---

## Phase 4: User Story 2 - Despliegue y operación independiente (Priority: P2)

**Goal**: Poder actualizar y redesplegar el Backend y el Frontend por separado.

**Independent Test**: Redesplegar solo el Backend (o solo el Frontend) y confirmar que el sistema
completo sigue funcionando de extremo a extremo (quickstart.md sección 3).

### Implementation for User Story 2

- [ ] T054 [US2] Externalizar la URL base del Backend (en `TimeClockSystem.Web`) y la clave de firma del token + cadena de conexión (en `TimeClockSystem.Api`) vía configuración/variables de entorno, sin acoplamiento fijo entre ambos
- [ ] T055 [US2] Verificar que `TimeClockSystem.Web` no tiene ninguna referencia de proyecto a `TimeClockSystem.Domain`, `Application` ni `Infrastructure` (solo cliente HTTP) (FR-007)
- [ ] T056 [US2] Validar manualmente `quickstart.md` sección 3: reiniciar solo la Api y luego solo el Web, confirmando que el otro sigue funcionando sin recompilar (SC-002)

**Checkpoint**: Backend y Frontend se despliegan y actualizan de forma independiente.

---

## Phase 5: User Story 3 - Verificación independiente del Backend (Priority: P3)

**Goal**: Poder explorar y probar todas las capacidades del Backend directamente, sin el Frontend.

**Independent Test**: Completar un flujo de negocio de extremo a extremo usando únicamente Swagger
(quickstart.md sección 1).

### Implementation for User Story 3

- [ ] T057 [P] [US3] Agregar comentarios XML y atributos `[ProducesResponseType]` a todos los Controllers de `src/TimeClockSystem.Api/Controllers/` para que Swagger documente cada contrato (FR-006)
- [ ] T058 [US3] Confirmar que el `DbSeeder` ya provee una cuenta Administrador de prueba utilizable directamente desde Swagger
- [ ] T059 [US3] Validar manualmente `quickstart.md` sección 1: crear empleado → asignar turno → marcar asistencia → consultar asistencia usando solo Swagger, en menos de 15 minutos (SC-003)

**Checkpoint**: Todas las historias de usuario funcionan de forma independiente.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Mejoras que abarcan varias historias de usuario.

- [ ] T060 [P] Actualizar el manual de usuario/README para describir la nueva arquitectura de dos proyectos y cómo ejecutar cada uno por separado
- [ ] T061 [P] Eliminar las carpetas `Domain/` e `Infrastructure/` ya no usadas de `src/TimeClockSystem.Web/` una vez confirmada la migración completa
- [ ] T062 Ejecutar `quickstart.md` completo de punta a punta como pase de regresión final sobre las 3 historias de usuario
- [ ] T063 [P] Revisar todos los `appsettings*.json` de ambos proyectos para confirmar que ningún secreto (clave del token, cadenas de conexión) quedó en texto plano (Principio V)
- [ ] T064 Medir manualmente el tiempo de respuesta percibido al registrar una marca y al guardar un cambio de empleado/turno desde el navegador (herramientas de desarrollador, pestaña Network) y confirmar que se mantiene bajo 5 segundos (SC-007, quickstart.md sección 6)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias — puede iniciar de inmediato.
- **Foundational (Phase 2)**: Depende de Setup — BLOQUEA a las 3 historias de usuario (ninguna puede probarse sin un Backend funcional).
- **User Stories (Phase 3-5)**: Todas dependen de Foundational.
  - US1 (P1) es el único bloqueante real del MVP; US2 y US3 pueden avanzar en paralelo a US1 una vez terminado Foundational, ya que no modifican los mismos archivos del Frontend.
- **Polish (Phase 6)**: Depende de que las historias que se quieran entregar ya estén completas.

### User Story Dependencies

- **US1 (P1)**: Depende solo de Foundational.
- **US2 (P2)**: Depende solo de Foundational; usa el resultado de US1 para su validación de extremo a extremo (T056), pero sus tareas de configuración (T054-T055) no dependen de US1.
- **US3 (P3)**: Depende solo de Foundational; independiente de US1/US2.

### Parallel Opportunities

- Dentro de Setup: T002-T004 y T006-T007 en paralelo.
- Dentro de Foundational: T009-T011 (Domain) en paralelo; T015-T018 (Application, distintos recursos) en paralelo; T024 y T027 en paralelo; T030-T036 (Controllers, distintos archivos) en paralelo.
- Dentro de US1: T043-T046 (distintas Areas) en paralelo, después de T039-T042.
- US2 y US3 pueden trabajarse en paralelo con US1 una vez terminado Foundational (equipos distintos).

---

## Parallel Example: Foundational — Controllers de la Api

```bash
Task: "Implementar EmpleadosController en src/TimeClockSystem.Api/Controllers/EmpleadosController.cs"
Task: "Implementar CentrosTrabajoController en src/TimeClockSystem.Api/Controllers/CentrosTrabajoController.cs"
Task: "Implementar TurnosController y AsignacionesTurnoController en src/TimeClockSystem.Api/Controllers/"
Task: "Implementar DiasFestivosController en src/TimeClockSystem.Api/Controllers/DiasFestivosController.cs"
Task: "Implementar MarcajeController en src/TimeClockSystem.Api/Controllers/MarcajeController.cs"
Task: "Implementar ConsultaAsistenciasController en src/TimeClockSystem.Api/Controllers/ConsultaAsistenciasController.cs"
Task: "Implementar AuditoriaController en src/TimeClockSystem.Api/Controllers/AuditoriaController.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Completar Fase 1: Setup
2. Completar Fase 2: Foundational (CRÍTICO — bloquea todo lo demás)
3. Completar Fase 3: User Story 1
4. **Detenerse y validar**: ejecutar `quickstart.md` secciones 2, 4 y 5
5. Desplegar/demostrar si está listo (MVP)

### Incremental Delivery

1. Setup + Foundational → Backend funcional y verificable por Swagger
2. Agregar US1 → validar → Frontend a la par del Backend (MVP)
3. Agregar US2 → validar despliegue independiente
4. Agregar US3 → validar documentación/pruebas directas de la API
5. Polish final

### Notes

- [P] = archivos distintos, sin dependencias pendientes
- [Story] mapea cada tarea a su historia de usuario para trazabilidad
- Verificar que la suite existente sigue pasando tras cada bloque de reubicación de código (T009-T027)
- Detenerse en cada checkpoint para validar la historia correspondiente de forma independiente
