# Specification Analysis Report: Separación de TimeClockSystem en Backend WebAPI y Frontend MVC

**Feature**: `002-split-backend-frontend`
**Date**: 2026-09-24
**Scope**: Análisis de consistencia entre `spec.md`, `plan.md` y `tasks.md` (más `research.md`,
`data-model.md`, `contracts/`, `quickstart.md` como contexto de apoyo), ejecutado antes de
`/speckit-implement`.

## Specification Analysis Report

| ID | Category | Severity | Location(s) | Summary | Recommendation |
|----|----------|----------|-------------|---------|----------------|
| I1 | Inconsistency | HIGH | spec.md:75 (Edge Case) vs spec.md:82 (FR-002) / research.md #2 / tasks.md T025, T037 | El Edge Case exige que "el Backend valide el rol vigente en cada operación en lugar de confiar indefinidamente en un dato de rol obsoleto", pero el diseño elegido (JWT con claims de rol, expiración fija, **sin** renovación silenciosa) hace que `[Authorize(Roles=...)]` (T037) solo verifique el rol capturado al emitir el token (T025). Si un Administrador es reasignado a Empleado a mitad de sesión, conserva permisos de Administrador hasta que el token expire — justo lo que el Edge Case dice que no debe pasar. | Decidir explícitamente uno de dos caminos y reflejarlo en spec.md/plan.md: (a) aceptar que la revalidación de rol ocurre solo al expirar el token (ajustar la redacción del Edge Case para que diga esto), o (b) agregar una tarea que revalide el rol contra la base de datos en cada request sensible (no solo confiar en el claim del JWT). |
| U1 | Underspecification | HIGH | src/TimeClockSystem.Web/Areas/Marcaje/Controllers/MarcajeController.cs:71-76 (código actual) vs data-model.md ("Usuario/Rol") vs contracts/marcaje.md y contracts/consulta-asistencias.md vs tasks.md (T013, T029, T034, T035) | Hoy, `ApplicationUser.EmpleadoId` vincula la cuenta de Identity con el `Empleado` (usado por `ObtenerEmpleadoActualAsync`). Ni `data-model.md` ni `research.md` ni las tareas T013 (login/token) o T034/T035 (Marcaje/ConsultaAsistencias) mencionan cómo el Backend resuelve "cuál Empleado soy" a partir del JWT (¿claim `empleadoId` en el token? ¿lookup por el id del `ApplicationUser`?). Sin esto, no se puede implementar correctamente el marcaje por canal `PortalWeb` ni la restricción de auto-consulta en `ConsultaAsistenciasController`. | Agregar en `data-model.md` (Usuario/Rol) y en `contracts/auth.md` que el token incluye un claim `empleadoId` (cuando el usuario está vinculado a un Empleado), y una nota en T013/T034/T035 de tasks.md indicando que deben leer ese claim en vez de reconsultar por nombre de usuario. |
| A1 | Ambiguity / Untestable criterion | HIGH | spec.md:113 (SC-002) vs tasks.md T056 / quickstart.md sección 3 | SC-002 dice "en al menos el 95% de los cambios rutinarios", pero la única validación existente (T056 / quickstart.md sección 3) ejecuta un único redespliegue de prueba. Un solo caso no puede demostrar un porcentaje estadístico del 95%. | Reformular SC-002 en términos binarios y verificables en una sola corrida (p. ej. "el Backend puede redesplegarse sin que el Frontend requiera recompilación o reinicio, y viceversa, en cualquier cambio que no altere el contrato HTTP") o aclarar explícitamente que el 95% es una expectativa cualitativa, no medida en esta iteración. |
| G1 | Coverage Gap | MEDIUM | spec.md:118 (SC-007) — sin tarea ni sección en tasks.md/quickstart.md | SC-007 ("95% de las acciones típicas en menos de 5 segundos") no tiene ninguna tarea asociada en `tasks.md` ni ningún paso de medición en `quickstart.md`. Es el único criterio de éxito sin cobertura alguna. | Agregar una tarea en Fase 6 (Polish) o al final de US1, p. ej. "Medir manualmente el tiempo de respuesta de registrar marca / guardar cambios en el navegador y confirmar que se mantiene bajo 5s", y una sección correspondiente en `quickstart.md`. |
| G2 | Coverage Gap | LOW | spec.md:89 (FR-008) — sin tarea dedicada en tasks.md | FR-008 (comunicación directa Frontend→Backend, sin gateway/BFF) no tiene una tarea que la verifique explícitamente; solo se infiere de la ausencia de un componente de gateway y de T055 (que verifica referencias de proyecto, no enrutamiento en tiempo de ejecución). | Bajo impacto: opcionalmente añadir una nota en T055 o T060 confirmando que ninguna llamada del Frontend pasa por un componente intermedio. |

## Coverage Summary Table

| Requirement Key | Has Task? | Task IDs | Notes |
|---|---|---|---|
| FR-001 | Yes | T005, T028-T036 | |
| FR-002 | Yes | T013, T025, T028 | Ver I1 |
| FR-002a | Yes | T041, T042 | |
| FR-003 | Yes | T037 | |
| FR-004 | Yes | T039-T048 | |
| FR-005 | Yes | T009, T010, T014, T027, T051 | |
| FR-006 | Yes | T005, T057 | |
| FR-007 | Yes | T001, T008, T054, T055 | |
| FR-008 | Partial | T040, T055 (indirecto) | Ver G2 |
| FR-009 | Yes | T049, T052 | |
| FR-010 | Yes | T021, T022, T053 | |
| FR-011 | Yes | T011, T013, T014, T019, T020, T026, T036 | |
| FR-011a | Yes | T050 | |
| FR-012 | Yes | T014, T047 | |
| SC-001 | Yes | T052 | |
| SC-002 | Partial | T056 | Ver A1 |
| SC-003 | Yes | T059 | |
| SC-004 | Yes | T056 (quickstart.md sección 3, paso 3) | No mencionado explícitamente en la descripción de T056 |
| SC-005 | Yes | T022, T053 | |
| SC-006 | Yes | T053 | |
| SC-007 | No | — | Ver G1 |

## Constitution Alignment Issues

Ninguno nuevo. Las dos desviaciones del Principio I (Simplicidad) — múltiples proyectos de Backend y
la pantalla de Auditoría — ya están identificadas y justificadas explícitamente en `plan.md` →
Complexity Tracking, tal como exige la constitución.

## Unmapped Tasks

Ninguna. Las 63 tareas de `tasks.md` referencian al menos un FR/SC o son de infraestructura
compartida (Setup/Polish) directamente derivada del plan.

## Metrics

- Total Requirements: 21 (14 FR + 7 SC)
- Total Tasks: 63
- Coverage %: ~95% (20/21 con al menos una tarea; solo SC-007 sin ninguna)
- Ambiguity Count: 1 (SC-002)
- Duplication Count: 0
- Critical Issues Count: 0 (los 3 hallazgos más severos son HIGH, no CRITICAL — ninguno viola un MUST de la constitución ni deja una funcionalidad base sin ninguna cobertura)

## Next Actions

No hay issues CRITICAL que bloqueen `/speckit-implement`, pero se recomienda resolver **I1** y **U1**
antes de implementar, ya que ambos afectan directamente el diseño de autenticación/autorización
(T025, T028, T034, T035, T037) y podrían causar retrabajo si se descubren a mitad de la
implementación. **A1** y **G1** son ajustes rápidos de redacción/cobertura que pueden resolverse en
paralelo. **G2** puede posponerse sin riesgo.

Sugerencias concretas:
- Editar `spec.md` (Edge Case + FR-002) y `research.md` (#2) para resolver I1.
- Editar `data-model.md` y `contracts/auth.md` para documentar el claim `empleadoId` (U1).
- Ajustar la redacción de SC-002 en `spec.md` (A1).
- Agregar una tarea de medición de rendimiento en `tasks.md` (G1).
