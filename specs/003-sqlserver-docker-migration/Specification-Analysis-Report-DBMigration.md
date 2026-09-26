# Specification Analysis Report: Migración a SQL Server y Contenerización con Docker

**Feature**: `003-sqlserver-docker-migration`
**Date**: 2026-09-24
**Scope**: Análisis de consistencia entre `spec.md`, `plan.md` y `tasks.md` (más `research.md`,
`data-model.md`, `contracts/`, `quickstart.md` como contexto de apoyo), ejecutado antes de
`/speckit-implement`.

## Specification Analysis Report

| ID | Category | Severity | Location(s) | Summary | Recommendation |
|----|----------|----------|-------------|---------|----------------|
| C1 | Coverage Gap | MEDIUM | spec.md FR-011 · tasks.md T015 | FR-011 (Backend contenedorizado debe sembrar DbSeeder) no tiene ninguna tarea que lo cite explícitamente; solo queda cubierto de forma implícita por la descripción de validación de T015. | Agregar `(FR-011)` a la cita de T015, o desdoblar una línea de validación específica para la siembra. |
| C2 | Coverage Gap | MEDIUM | spec.md FR-012 · tasks.md T012 | FR-012 (entorno exclusivamente dev/demo local: sin HTTPS interno, sin edición de producción, sin límites de recursos) no está citado en ninguna tarea; solo se refleja implícitamente en las elecciones de diseño de T012 (edición Developer, HTTP). | Agregar `(FR-012)` a la cita de T012 para dejar trazable la decisión. |
| C3 | Coverage Gap | MEDIUM | spec.md FR-014 · tasks.md T005, T006 | FR-014 (la base de datos SQL Server inicia vacía, sin migrar datos de SQLite) no está citado en ninguna tarea; se satisface solo por omisión (no existe tarea de importación) y por el reemplazo de migraciones en T005/T006. | Agregar `(FR-014)` a T005 o T006 para dejar constancia explícita de la decisión de no migrar datos. |
| C4 | Coverage Gap | MEDIUM | spec.md SC-004 · tasks.md T015 | SC-004 (sistema utilizable con datos de ejemplo inmediatamente tras el primer arranque, sin carga manual) no aparece citado en ninguna tarea, a diferencia de SC-001/SC-006 que sí están citados en la misma T015. | Agregar `SC-004` a la lista de criterios citados en T015. |
| I1 | Inconsistency | MEDIUM | tasks.md T018 · quickstart.md | Todas las demás tareas de validación (T015, T017, T020, T022, T023, T024) citan un paso numerado de `quickstart.md`; T018 (validar fallo de migración) no tiene un paso correspondiente en `quickstart.md` — ese archivo llega hasta el paso 7 (secreto faltante) sin cubrir el escenario de migración corrupta. | Agregar un paso 8 a `quickstart.md` ("Confirmar fallo explícito ante una migración incompatible") que documente el procedimiento que T018 ya describe, para que la guía de validación quede completa. |
| A1 | Ambiguity | LOW | spec.md FR-007, SC-006 · tasks.md T007 | La "ventana acotada de tiempo/reintentos" para que el Backend espere a la base de datos no tiene un valor concreto en la spec; el único número ("~2 minutos") aparece en tasks.md marcado explícitamente como ejemplo no vinculante ("p. ej."). | Si el valor exacto importa para la aceptación (p. ej. para un test automatizado futuro), fijarlo en FR-007 o SC-006; si no, dejarlo como está y aceptar que es una decisión de implementación (consistente con Principio I de simplicidad). |
| U1 | Underspecification | LOW | tasks.md T001, T016 | T001 (`.dockerignore`) y T016 (correr la suite de pruebas existente) no citan ningún FR/SC, a diferencia del resto de las tareas, aunque apoyan indirectamente SC-005 y la Historia 2 respectivamente. | Agregar `(SC-005)` a T001 y una referencia explícita a la Historia 2 (US2) en el texto de T016, para consistencia de trazabilidad con el resto de las tareas. |

## Coverage Summary Table

| Requirement Key | Has Task? | Task IDs | Notes |
|---|---|---|---|
| FR-001 | Yes | T003, T004 | Cambio de proveedor EF Core |
| FR-002 | Yes | T009 (vía T013) | Dockerfile Backend |
| FR-003 | Yes | T010 (vía T014) | Dockerfile Frontend |
| FR-004 | Yes | T012 | Contenedor SQL Server, imagen oficial |
| FR-005 | Yes | T019 | Volumen `mssql-data` |
| FR-006 | Yes | T012–T014 (citado en T013) | Un solo comando |
| FR-007 | Yes | T007, T012/T013 (healthcheck) | Espera acotada; ver A1 |
| FR-008 | Yes | T014 | Frontend → Backend por nombre de servicio |
| FR-009 | Yes | T002, T008, T011, T013, T022, T024 | Secretos vía env vars |
| FR-010 | Yes | T007, T018 | Auto-migración + fallo explícito |
| FR-011 | Partial | T015 (implícito) | Ver C1 |
| FR-012 | Partial | T012 (implícito) | Ver C2 |
| FR-013 | Yes | T008, T021, T023 | `dotnet run` sin Docker |
| FR-014 | Partial | T005, T006 (implícito) | Ver C3 |
| SC-001 | Yes | T015 | |
| SC-002 | Yes | T017 | |
| SC-003 | Yes | T020 | |
| SC-004 | Partial | T015 (implícito) | Ver C4 |
| SC-005 | Yes | T022, T024 | |
| SC-006 | Yes | T015 | |
| SC-007 | Yes | T017 | |

## Constitution Alignment Issues

Ninguno. Los 5 principios se revisaron contra `spec.md`, `plan.md` y `tasks.md` sin encontrar
conflictos con ninguna declaración MUST. El plan no registra ninguna desviación en su sección
Complexity Tracking (no aplica).

## Unmapped Tasks

Ninguno. Las 24 tareas de `tasks.md` se relacionan con al menos un requisito o historia de
usuario, aunque 3 FR y 1 SC carecen de cita explícita (ver C1–C4).

## Metrics

- Total Requirements: 21 (14 FR + 7 SC)
- Total Tasks: 24
- Coverage %: 81% (17/21 con cita explícita; el 100% de los requisitos están funcionalmente
  cubiertos, la brecha es de trazabilidad/cita, no de implementación faltante)
- Ambiguity Count: 1 (A1)
- Duplication Count: 0
- Critical Issues Count: 0

## Next Actions

No hay hallazgos CRITICAL ni HIGH — se puede proceder a `/speckit-implement` sin bloqueos. Los 7
hallazgos son MEDIUM/LOW y son mejoras de trazabilidad/documentación, no defectos funcionales:

- Recomendado antes de implementar (rápido, ediciones de una línea): resolver C1–C4 agregando las
  citas `(FR-011)`, `(FR-012)`, `(FR-014)`, `SC-004` a las tareas correspondientes en `tasks.md`.
- Recomendado en paralelo: agregar el paso 8 a `quickstart.md` (I1) para que la guía de validación
  cubra el escenario que T018 ya implementa.
- Opcional: A1 y U1 son mejoras cosméticas, no bloquean la implementación.
