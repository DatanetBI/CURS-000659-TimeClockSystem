# Specification Analysis Report — RC1

**Feature**: TimeClockSystem v1.0 — Gestión de Asistencias (`001-timeclock-spec-v1`)
**Fecha**: 2026-09-22
**Artefactos analizados**: `spec.md`, `plan.md`, `tasks.md`, `data-model.md`, `.specify/memory/constitution.md`
**Tipo de análisis**: Consistencia cruzada, ambigüedad y cobertura, previo a `/speckit-implement` (solo lectura — ningún artefacto fue modificado por este análisis)

## Hallazgos

| ID | Category | Severity | Location(s) | Summary | Recommendation |
|----|----------|----------|-------------|---------|----------------|
| I1 | Inconsistency | HIGH | plan.md:47 vs tasks.md:162,168 | La tabla de alcance de `plan.md` atribuye FR-002/FR-003 (marcaje por número de empleado + PIN) al **Módulo 2**, pero la lógica real (validar PIN al marcar, rechazar credenciales inválidas) está implementada en **T050/T056, Módulo 5** (`PinMarcajeController`). Módulo 2 solo cubre FR-004 (asignar/restablecer el PIN, T025). | Corregir la fila del Módulo 2 en `plan.md` para que solo liste FR-004; mover FR-002/FR-003 a la fila del Módulo 5. |
| I2 | Inconsistency | HIGH | tasks.md:15,27,293 vs spec.md:45,88,124,158,189,212,237 | `tasks.md` reutiliza las etiquetas `[US1]`–`[US6]` para los 6 módulos de `plan.md`, pero `spec.md` ya usa `US1`–`US7` para sus propias historias de usuario (con contenido distinto: p. ej. `[US1]` en tasks.md = Centros de Trabajo; "User Story 1" en spec.md = marcaje con offline). Ya está aclarado en el propio `tasks.md`, pero el riesgo de confundir una referencia cruzada durante la implementación o revisión es real. | Renombrar las etiquetas de módulo a algo sin colisión, p. ej. `[M1]`–`[M6]`, en vez de `[US1]`–`[US6]`. |
| G1 | Coverage Gap | HIGH | spec.md:433-435 (FR-046), spec.md:287-289 (CL9) | FR-046 exige solicitar y registrar el consentimiento explícito antes de capturar geolocalización — y v1.0 **sí captura geolocalización** para el geofence (T048). Ni `data-model.md` ni `tasks.md` tienen ningún campo o tarea para este consentimiento, y no aparece en la lista de "Diferido explícitamente a v1.1" de `plan.md` (a diferencia del consentimiento biométrico, que sí está correctamente diferido). CL9 (bloquear marcaje sin consentimiento) tampoco tiene tarea. | Agregar un campo `ConsentimientoGeolocalizacion` a `Empleado` en `data-model.md` y una tarea en el Módulo 5 que lo valide antes de aceptar una marca con geolocalización — o, si se decide diferirlo, agregarlo explícitamente a la lista de diferidos de `plan.md`. |
| I3 | Inconsistency | MEDIUM | data-model.md:100 vs spec.md:312-313 (FR-001) | FR-001 exige marcaje desde RFID, portal web, app móvil o terminal físico. `data-model.md` reduce silenciosamente los canales de v1.0 a solo `portal_web` y `pin` (excluyendo RFID y terminal físico) en una nota de tabla, sin que esta reducción aparezca en la lista explícita de "Diferido explícitamente a v1.1" de `plan.md` (que sí menciona la ausencia de app nativa, pero no la de RFID/terminal físico). | Agregar "Marcaje por RFID/terminal físico" a la lista de diferidos de `plan.md`, para que quede al mismo nivel de visibilidad que las demás decisiones de alcance. |
| U1 | Underspecification | MEDIUM | plan.md:44-51 (fila Módulo 3) vs data-model.md:75-77 | `plan.md` lista FR-014 (respetar restricciones horarias individuales aprobadas en una asignación masiva) como cubierto por el Módulo 3, pero `data-model.md` aclara que esa exclusión depende de Incidencias (diferido) y por tanto no hay ninguna fuente de datos de "restricción aprobada" en v1.0. `tasks.md` (T036) implementa el resumen de asignados/excluidos citando FR-013/014, pero sin ningún mecanismo real de exclusión. | Aclarar en `plan.md` que FR-014 solo se satisface parcialmente (el resumen existe, pero nunca habrá excluidos por restricción en v1.0 al no existir Incidencias), o quitar FR-014 de la fila del Módulo 3. |
| G2 | Coverage Gap | MEDIUM | spec.md:487-488 (SC-010) | SC-010 (soportar hasta 500 empleados activos sin degradar los tiempos de SC-001/003/006/008/009) no tiene ninguna tarea de prueba de carga/escala en `tasks.md` — las pruebas de integración (T018, T028, T038, etc.) validan corrección funcional, no volumen. | Agregar una tarea de prueba de carga ligera (p. ej. sembrar 500 empleados y medir tiempo de respuesta del marcaje) en la Fase 9 (Polish), o documentar explícitamente que la validación de escala se pospone. |
| U2 | Underspecification | LOW | plan.md:198-221 vs tasks.md:58 | El árbol de `Project Structure` de `plan.md` no muestra una carpeta `Views/Shared/` en la raíz del proyecto, pero T007 crea `src/TimeClockSystem.Web/Views/Shared/_Layout.cshtml` ahí. Es una convención estándar de ASP.NET Core MVC, no una contradicción real, pero el árbol quedó incompleto. | Agregar `Views/Shared/` al árbol de `plan.md` §Project Structure para que coincida con T007. |
| G3 | Coverage Gap (informational) | LOW | spec.md (FR-008/009, FR-012, FR-016 a FR-047 salvo lo ya cubierto), plan.md:53-70 | 34 de 47 FR y 7 de 10 SC no tienen ninguna tarea en `tasks.md` — pero esto es **intencional y está documentado explícitamente** en `plan.md` §"Diferido explícitamente a v1.1" (Incidencias, Exportación/Importación, offline, motor de pre-nómina, panel de presencia en vivo, reportes/auditoría, roles adicionales). No requiere acción; se incluye aquí solo para que el cálculo de cobertura no se lea como un descuido. | Ninguna — ya está resuelto por diseño. Retomar en la planeación de v1.1. |

## Coverage Summary Table

| Requirement Key | Has Task? | Task IDs | Notes |
|-----------------|-----------|----------|-------|
| FR-001 | Parcial | T049, T053 | Canales reducidos a portal_web/pin (ver I3) |
| FR-002, FR-003 | Sí | T050, T056 | Atribuidos a Módulo 2 en plan.md, implementados en Módulo 5 (ver I1) |
| FR-004 | Sí | T025 | — |
| FR-005 | Sí | T051 | — |
| FR-006 | Sí | T048, T052 | — |
| FR-007 | Sí (stub) | T047 | Punto de integración, sin implementación real (por diseño) |
| FR-010 | Sí | T052 | — |
| FR-011, FR-015 | Sí | T029, T033, T037 | — |
| FR-013, FR-014 | Parcial | T036 | FR-014 sin mecanismo real de exclusión (ver U1) |
| FR-046 | **No** | — | Gap real, ver G1 |
| FR-008, FR-009, FR-012, FR-016–FR-045 (resto), FR-047 | No (por diseño) | — | Diferido a v1.1, ver G3 |
| SC-001, SC-002 | Sí | T045-T056 (flujo general) | — |
| SC-010 | Parcial | — | Sin prueba de escala, ver G2 |
| SC-003–SC-009 | No (por diseño) | — | Correctamente fuera de alcance v1.0 |

## Constitution Alignment Issues

Ninguno. Los 5 principios siguen pasando según el Constitution Check de `plan.md`; los hallazgos de esta tabla son de consistencia/completitud entre documentos, no violaciones de la constitución.

## Unmapped Tasks

Ninguna — las 67 tareas de `tasks.md` mapean a un módulo/fase con propósito claro.

## Metrics

- Total Requirements: 57 (47 FR + 10 SC)
- Total Tasks: 67
- Coverage %: ~25% de requisitos con ≥1 tarea (14/57) — el ~75% restante corresponde casi todo a alcance intencionalmente diferido a v1.1 (G3), salvo FR-046 (G1, gap real) y SC-010 (G2, gap parcial)
- Ambiguity Count: 0
- Duplication Count: 0
- Critical Issues Count: 0 (el hallazgo de mayor severidad es HIGH, no CRITICAL — nada viola la constitución ni bloquea la funcionalidad base)

## Next Actions

Solo hay hallazgos HIGH/MEDIUM/LOW, ninguno CRITICAL — se puede proceder a `/speckit-implement` si se prefiere, pero se recomienda resolver al menos **G1** (consentimiento de geolocalización) antes, porque toca un requisito de privacidad ya capturado por el propio Módulo 5.

- `G1`: agregar el campo de consentimiento a `data-model.md` y una tarea en el Módulo 5 (o diferirlo explícitamente en `plan.md`)
- `I1`, `I3`: editar `plan.md` (tabla de alcance y lista de diferidos) para que coincida con lo que `tasks.md`/`data-model.md` realmente construyen
- `I2`: renombrar `[US1]`–`[US6]` a `[M1]`–`[M6]` en `tasks.md`
- `U1`, `G2`, `U2`: ajustes menores, se pueden resolver junto con lo anterior o dejarse para después
