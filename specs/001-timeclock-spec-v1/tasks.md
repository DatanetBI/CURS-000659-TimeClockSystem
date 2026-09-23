---

description: "Task list for TimeClockSystem v1.0 (6 módulos)"
---

# Tasks: TimeClockSystem v1.0 — Gestión de Asistencias

**Input**: Design documents from `/specs/001-timeclock-spec-v1/` (`plan.md`, `spec.md`, `research.md`,
`data-model.md`, `contracts/`, `quickstart.md`)

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/web-endpoints.md`,
`quickstart.md`

**Alcance**: Esta lista cubre únicamente los **6 módulos de v1.0** decididos en `plan.md`
(§"Alcance de v1.0 vs. v1.1"). Cada módulo se etiqueta como una "historia" (`[US1]`...`[US6]`) en el
mismo orden de construcción del plan — no es el mismo orden P1-P7 de `spec.md`, que describe el producto
completo. Incidencias, Exportación/Importación, offline, motor de pre-nómina, panel de presencia en vivo,
reportes/KPIs y auditoría quedan fuera de esta lista (diferidos a v1.1).

**Tests**: Se incluyen tareas de prueba ligeras (una prueba de integración por módulo, más algunas
unitarias) porque `plan.md`/`research.md` ya comprometen xUnit + `WebApplicationFactory` como parte de la
solución; no es un enfoque TDD estricto de "test primero" para cada tarea.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Se puede ejecutar en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: Módulo al que pertenece (US1 a US6, según la tabla de `plan.md`)
- Cada descripción incluye la ruta de archivo exacta

## Path Conventions

Proyecto único (`plan.md` §Project Structure):

- `src/TimeClockSystem.Web/` — proyecto ASP.NET Core (Areas, Domain, Infrastructure, wwwroot)
- `tests/TimeClockSystem.Tests/` — Unit, Integration

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialización del proyecto.

- [ ] T001 Crear la estructura de carpetas de `src/TimeClockSystem.Web/` y `tests/TimeClockSystem.Tests/` según `plan.md` §Project Structure
- [ ] T002 Inicializar el proyecto ASP.NET Core 10 (MVC) con las dependencias de `plan.md` (EF Core SQLite, ASP.NET Core Identity, Bootstrap 5 vía CDN) en `src/TimeClockSystem.Web/TimeClockSystem.Web.csproj`
- [ ] T003 [P] Configurar `.editorconfig` y `dotnet format` en la raíz del repositorio
- [ ] T004 [P] Inicializar el proyecto de pruebas xUnit + `Microsoft.AspNetCore.Mvc.Testing` en `tests/TimeClockSystem.Tests/TimeClockSystem.Tests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura común que TODOS los módulos necesitan.

**⚠️ CRÍTICO**: Ningún módulo (Fase 3+) puede empezar hasta que esta fase esté completa.

- [ ] T005 Crear `ApplicationDbContext` (EF Core, proveedor SQLite) y la migración inicial en `src/TimeClockSystem.Web/Infrastructure/Data/ApplicationDbContext.cs`
- [ ] T006 [P] Configurar ASP.NET Core Identity con los roles `Empleado` y `Administrador` en `src/TimeClockSystem.Web/Infrastructure/Identity/IdentityConfiguration.cs` (research.md §3)
- [ ] T007 [P] Crear el layout base Razor con Bootstrap 5 (CDN), navegación responsiva y cultura español (México) en `src/TimeClockSystem.Web/Views/Shared/_Layout.cshtml`
- [ ] T008 Configurar manejo global de errores y logging en `src/TimeClockSystem.Web/Program.cs`
- [ ] T009 [P] Configurar `appsettings.json` y variables de entorno para la cadena de conexión (sin secretos en código — Principio V) en `src/TimeClockSystem.Web/appsettings.json` y `Program.cs`
- [ ] T010 Crear el orquestador `DbSeeder` (se ejecuta al iniciar si la base está vacía) en `src/TimeClockSystem.Web/Infrastructure/Data/DbSeeder.cs`, sembrando primero los 2 usuarios Administrador de ejemplo (`plan.md` §Datos mock)

**Checkpoint**: Con la Fase 2 completa, el Módulo 1 puede empezar.

---

## Phase 3: Módulo 1 — Centros de trabajo (US1) 🎯 Primer incremento probable

**Goal**: El administrador puede crear, editar y listar centros de trabajo (geofence).

**Independent Test**: Iniciar sesión como Administrador, crear un centro de trabajo (nombre, coordenadas,
radio) y verlo aparecer en la lista — `quickstart.md` §Módulo 1.

- [ ] T011 [P] [US1] Crear la entidad `CentroTrabajo` en `src/TimeClockSystem.Web/Domain/CentroTrabajo.cs` (data-model.md)
- [ ] T012 [US1] Agregar el `DbSet<CentroTrabajo>` y su migración a `ApplicationDbContext` (depende de T011, T005)
- [ ] T013 [US1] Extender `DbSeeder` con los 3 centros de trabajo de ejemplo (depende de T012, T010)
- [ ] T014 [US1] Implementar `CentrosTrabajoController` (CRUD) en `src/TimeClockSystem.Web/Areas/CentrosTrabajo/CentrosTrabajoController.cs`
- [ ] T015 [P] [US1] Crear las vistas Razor de CRUD (Index/Create/Edit) en `src/TimeClockSystem.Web/Areas/CentrosTrabajo/Views/`
- [ ] T016 [US1] Agregar validación (radio > 0, coordenadas requeridas) en `CentrosTrabajoController`
- [ ] T017 [US1] Restringir el acceso al rol `Administrador` en `CentrosTrabajoController`
- [ ] T018 [P] [US1] Prueba de integración: crear/editar/listar un centro de trabajo en `tests/TimeClockSystem.Tests/Integration/CentrosTrabajoTests.cs`

**Checkpoint**: Módulo 1 funcional y probable de forma independiente.

---

## Phase 4: Módulo 2 — Empleados y credenciales de marcaje (US2)

**Goal**: El administrador puede dar de alta empleados, asociarlos a un centro de trabajo y asignarles un
PIN de marcaje.

**Independent Test**: Crear un empleado, asociarlo a un centro de trabajo del Módulo 1, asignarle un PIN —
`quickstart.md` §Módulo 2.

- [ ] T019 [P] [US2] Crear la entidad `Empleado` en `src/TimeClockSystem.Web/Domain/Empleado.cs` (referencia a `CentroTrabajo`, depende de T011)
- [ ] T020 [P] [US2] Crear la entidad `CredencialDeMarcaje` (PinHash) en `src/TimeClockSystem.Web/Domain/CredencialDeMarcaje.cs`
- [ ] T021 [US2] Agregar los `DbSet` de `Empleado` y `CredencialDeMarcaje` y su migración (depende de T019, T020)
- [ ] T022 [US2] Extender `DbSeeder` con los ~12 empleados de ejemplo, cada uno con PIN ya asignado (depende de T021, T013)
- [ ] T023 [US2] Implementar `EmpleadosController` (CRUD) en `src/TimeClockSystem.Web/Areas/Empleados/EmpleadosController.cs`
- [ ] T024 [P] [US2] Crear las vistas Razor de CRUD de empleados en `src/TimeClockSystem.Web/Areas/Empleados/Views/`
- [ ] T025 [US2] Implementar la acción "asignar/restablecer PIN" (hash vía `PasswordHasher<T>`) en `src/TimeClockSystem.Web/Areas/Empleados/PinController.cs` (FR-004)
- [ ] T026 [US2] Agregar validación: `NumeroEmpleado` único, PIN numérico de 4 a 6 dígitos
- [ ] T027 [US2] Restringir el acceso al rol `Administrador` en los controladores del módulo
- [ ] T028 [P] [US2] Prueba de integración: alta de empleado + asignación de PIN en `tests/TimeClockSystem.Tests/Integration/EmpleadosTests.cs`

**Checkpoint**: Módulos 1 y 2 funcionan de forma independiente y juntos.

---

## Phase 5: Módulo 3 — Turnos y asignación de turnos (US3)

**Goal**: El administrador puede crear turnos y asignarlos a empleados, individual o masivamente.

**Independent Test**: Crear un turno con tolerancia, asignarlo a un empleado del Módulo 2 y a un grupo —
`quickstart.md` §Módulo 3.

- [ ] T029 [P] [US3] Crear la entidad `Turno` en `src/TimeClockSystem.Web/Domain/Turno.cs`
- [ ] T030 [P] [US3] Crear la entidad `AsignacionTurno` en `src/TimeClockSystem.Web/Domain/AsignacionTurno.cs` (referencia a `Empleado`, depende de T019)
- [ ] T031 [US3] Agregar los `DbSet` de `Turno` y `AsignacionTurno` y su migración (depende de T029, T030)
- [ ] T032 [US3] Extender `DbSeeder` con 2-3 turnos de ejemplo y sus asignaciones (depende de T031, T022)
- [ ] T033 [US3] Implementar `TurnosController` (CRUD de turno) en `src/TimeClockSystem.Web/Areas/Turnos/TurnosController.cs`
- [ ] T034 [P] [US3] Crear las vistas Razor de CRUD de turnos en `src/TimeClockSystem.Web/Areas/Turnos/Views/`
- [ ] T035 [US3] Implementar la asignación individual de turno a un empleado (acción + vista)
- [ ] T036 [US3] Implementar la asignación masiva a un grupo de empleados, con resumen de asignados/excluidos (FR-013/FR-014)
- [ ] T037 [US3] Agregar validación: `ToleranciaMinutos` ≥ 0 y menor que la duración del turno (FR-015)
- [ ] T038 [P] [US3] Prueba de integración: crear turno + asignación masiva en `tests/TimeClockSystem.Tests/Integration/TurnosTests.cs`

**Checkpoint**: Módulos 1 a 3 funcionan de forma independiente y juntos.

---

## Phase 6: Módulo 4 — Días festivos (US4)

**Goal**: El administrador mantiene un calendario de días festivos.

**Independent Test**: Dar de alta una fecha festiva y verla en el catálogo — `quickstart.md` §Módulo 4.

- [ ] T039 [P] [US4] Crear la entidad `DiaFestivo` en `src/TimeClockSystem.Web/Domain/DiaFestivo.cs`
- [ ] T040 [US4] Agregar el `DbSet<DiaFestivo>` y su migración (depende de T039)
- [ ] T041 [US4] Extender `DbSeeder` con 2-3 fechas festivas de ejemplo (depende de T040)
- [ ] T042 [US4] Implementar `DiasFestivosController` (CRUD) en `src/TimeClockSystem.Web/Areas/DiasFestivos/DiasFestivosController.cs`
- [ ] T043 [P] [US4] Crear las vistas Razor de CRUD de días festivos en `src/TimeClockSystem.Web/Areas/DiasFestivos/Views/`
- [ ] T044 [P] [US4] Prueba de integración: alta de un día festivo en `tests/TimeClockSystem.Tests/Integration/DiasFestivosTests.cs`

**Checkpoint**: Módulos 1 a 4 funcionan de forma independiente y juntos.

---

## Phase 7: Módulo 5 — Registro de asistencias (US5)

**Goal**: Un empleado registra su marca de entrada/salida/receso en línea (portal o PIN de kiosco), con
validación de geofence.

**Independent Test**: Marcar entrada dentro del geofence, verla confirmada de inmediato; repetir fuera del
geofence y con PIN incorrecto — `quickstart.md` §Módulo 5.

- [ ] T045 [P] [US5] Crear la entidad `Marca` en `src/TimeClockSystem.Web/Domain/Marca.cs` (referencia a `Empleado`, depende de T019)
- [ ] T046 [US5] Agregar el `DbSet<Marca>` y su migración (depende de T045)
- [ ] T047 [US5] Crear la interfaz `IBiometricVerificationProvider` con una implementación por defecto sin validación real, en `src/TimeClockSystem.Web/Domain/IBiometricVerificationProvider.cs` (FR-007 — punto de integración diferido)
- [ ] T048 [US5] Implementar `GeofenceValidator` (distancia Haversine contra `CentroTrabajo`) en `src/TimeClockSystem.Web/Domain/GeofenceValidator.cs` (depende de T011)
- [ ] T049 [US5] Implementar `MarcajeController`: registrar marca desde una sesión de portal autenticada (depende de T046, T048)
- [ ] T050 [US5] Implementar `PinMarcajeController`: registrar marca por número de empleado + PIN, modo kiosco sin sesión completa (depende de T025, T046) (FR-002/FR-003)
- [ ] T051 [US5] Agregar la regla de rechazo de entrada duplicada si ya existe una entrada abierta (FR-005)
- [ ] T052 [US5] Registrar un evento de seguridad cuando una marca se rechaza por geofence (FR-010)
- [ ] T053 [P] [US5] Crear las vistas Razor de marcaje (portal y kiosco), responsivas, en `src/TimeClockSystem.Web/Areas/Marcaje/Views/`
- [ ] T054 [US5] Extender `DbSeeder` con marcas de ejemplo de días anteriores (depende de T046, T041)
- [ ] T055 [P] [US5] Prueba unitaria de `GeofenceValidator` (dentro/fuera del radio) en `tests/TimeClockSystem.Tests/Unit/GeofenceValidatorTests.cs`
- [ ] T056 [P] [US5] Prueba de integración: marcar entrada/salida, entrada duplicada, fuera de geofence, PIN inválido en `tests/TimeClockSystem.Tests/Integration/MarcajeTests.cs`

**Checkpoint**: Módulos 1 a 5 funcionan de forma independiente y juntos — este es el primer punto donde
un empleado real puede usar la aplicación de principio a fin.

---

## Phase 8: Módulo 6 — Portal de consulta de asistencias (US6)

**Goal**: El administrador filtra y consulta las marcas registradas, viendo si cada una fue puntual/tardía
y si cayó en día festivo.

**Independent Test**: Filtrar por empleado, fecha y centro de trabajo, y verificar los indicadores —
`quickstart.md` §Módulo 6.

- [ ] T057 [US6] Implementar `PuntualidadCalculator` (compara `Marca` contra el `Turno` asignado ese día + tolerancia) en `src/TimeClockSystem.Web/Domain/PuntualidadCalculator.cs` (depende de T045, T031)
- [ ] T058 [US6] Implementar `ConsultaAsistenciasController` con filtros por empleado, fecha y centro de trabajo en `src/TimeClockSystem.Web/Areas/ConsultaAsistencias/ConsultaAsistenciasController.cs` (depende de T057)
- [ ] T059 [P] [US6] Crear la vista Razor de resultados (tabla filtrable, responsiva) en `src/TimeClockSystem.Web/Areas/ConsultaAsistencias/Views/`
- [ ] T060 [US6] Señalar en los resultados si el día correspondiente es festivo (depende de T039)
- [ ] T061 [US6] Restringir el acceso al rol `Administrador`
- [ ] T062 [P] [US6] Prueba de integración: filtros por empleado/fecha/centro y verificación de indicadores en `tests/TimeClockSystem.Tests/Integration/ConsultaAsistenciasTests.cs`

**Checkpoint**: Los 6 módulos de v1.0 funcionan de forma independiente y en conjunto.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Mejoras que afectan a los 6 módulos.

- [ ] T063 [P] Revisar que toda la interfaz esté en español de México en los 6 módulos (Principio II de la constitución)
- [ ] T064 [P] Ejecutar `quickstart.md` módulo por módulo y confirmar manualmente cada criterio de aceptación (Principio IV)
- [ ] T065 Revisar que ningún secreto ni cadena de conexión esté en el código fuente; confirmar el uso de variables de entorno (Principio V)
- [ ] T066 [P] Pulir mensajes de validación y manejo de errores en los 6 módulos
- [ ] T067 Ejecutar `dotnet test` completo y confirmar que toda la suite pasa

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
| US1 — Centros de trabajo | Fase 2 (Foundational) |
| US2 — Empleados y credenciales | US1 (el empleado necesita un centro de trabajo) |
| US3 — Turnos y asignación | US2 (se asignan turnos a empleados existentes) |
| US4 — Días festivos | Fase 2 (independiente de US1-US3, pero se construye en este punto por orden de negocio) |
| US5 — Registro de asistencias | US1, US2, US3 (geofence, empleado+PIN, turno asignado) |
| US6 — Portal de consulta | US3, US4, US5 (necesita turnos, festivos y marcas ya existentes) |

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
- La etiqueta de módulo (`[US1]`...`[US6]`) mapea cada tarea a la tabla de alcance de `plan.md`, no a la
  numeración original de historias de usuario de `spec.md`.
- A diferencia del patrón típico de historias independientes entre sí, estos 6 módulos son
  intencionalmente secuenciales (dependencia real de datos: turnos necesitan empleados, marcaje necesita
  turnos y empleados, consulta necesita marcas) — así lo pidió el negocio para poder probar la app paso a
  paso.
- Hacer commit después de cada tarea o grupo lógico de tareas.
- Verificar en cada checkpoint que el módulo recién completado es usable de forma independiente antes de
  continuar con el siguiente.
- Evitar: tareas vagas, conflictos de archivo entre tareas paralelas, y construir un módulo antes que el
  módulo del que depende.
