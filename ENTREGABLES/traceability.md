# Traceability Matrix — Time Clock System

> **Entregable**: `traceability.md` — Matriz RF → HU → CU → pruebas o endpoints.
> **Fuentes originales**: esta matriz **no existe en ningún documento del repositorio** — se construyó cruzando manualmente:
> - RF y su agrupación por módulo: [`docs/specs/functional/03-especificacion-consolidada.md`](../docs/specs/functional/03-especificacion-consolidada.md) §4.
> - HU (US-00X): [`docs/specs/functional/02-user-stories.md`](../docs/specs/functional/02-user-stories.md) y su tabla de trazabilidad Módulo↔HU (§3).
> - CU (CU-00X): [`ENTREGABLES/use-cases.md`](./use-cases.md) (1:1 con cada HU).
> - Bounded Context: [`docs/architecture/domain-model.md`](../docs/architecture/domain-model.md) §1.2 (tabla "Trazabilidad Historia de Usuario → Bounded Context → Agregado").
> - Endpoints: [`specs/002-split-backend-frontend/contracts/*.md`](../specs/002-split-backend-frontend/contracts/).

## ⚠️ Advertencia importante sobre la columna "Endpoint"

Los RF/HU/CU de este documento describen la **visión objetivo de 9 microservicios** (`docs/specs/functional/` + `docs/architecture/`). Los endpoints de la columna "Endpoint" pertenecen, en cambio, al **sistema realmente implementado** en este repositorio (`src/TimeClockSystem.Api`, un monolito Clean Architecture documentado en `specs/002-split-backend-frontend/contracts/`), que cubre un subconjunto mucho más pequeño de funcionalidad (sin biometría, sin offline, sin motor de pre-nómina, sin permisos/vacaciones, sin portal de supervisor en tiempo real, sin integración ERP). Esta columna se incluyó **a petición explícita** para dejar visible qué parte de la visión ya tiene una implementación real y qué parte no, **no** porque ambos conjuntos de documentos describan el mismo sistema. Donde no existe ningún endpoint razonablemente equivalente, se marca **"No implementado"**.

**Columna "Prueba"**: no existe ningún archivo de prueba automatizada en el repositorio vinculado a los RF/HU de `docs/specs/functional/` (los tests en `tests/TimeClockSystem.Api.Tests/` validan el comportamiento del sistema de `specs/002`, no estas historias). Por eso esta matriz usa **"Endpoint"** como columna de verificación, tal como permite el propio nombre del entregable ("pruebas **o** endpoints").

**Leyenda de estado**:
- ✅ **Implementado** — el endpoint cubre el RF de forma equivalente.
- 🟡 **Parcial** — existe un endpoint relacionado, pero cubre solo una parte del RF (canal, campo o variante faltante, indicado entre paréntesis).
- ❌ **No implementado** — no existe ningún endpoint del sistema real que cubra este RF.

---

## Matriz RF → HU → CU → Bounded Context → Endpoint

### Módulo 1 — Marcaje y captura de asistencia

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF1 | US-001, US-002 | CU-001, CU-002 | Clocking & Attendance Capture | `POST /api/marcaje` (canal PortalWeb), `POST /api/marcaje/pin` (canal Pin/kiosco) | 🟡 Parcial (sin canales biométrico físico ni RFID) |
| RF2 | US-001 | CU-001 | Clocking & Attendance Capture | `POST /api/marcaje` (rechazo `EntradaDuplicada`) | ✅ Implementado |
| RF3 | US-002 | CU-002 | Clocking & Attendance Capture | `POST /api/marcaje` (rechazo `Geofence`); `GET/POST/PUT/DELETE /api/centros-trabajo` (configuración) | ✅ Implementado |
| RF4 | US-002 | CU-002 | Clocking & Attendance Capture | — | ❌ No implementado (sin liveness/reconocimiento facial) |
| RF5 | US-003 | CU-003 | Clocking & Attendance Capture | — | ❌ No implementado (sin captura/almacenamiento offline) |
| RF6 | US-003 | CU-003 | Clocking & Attendance Capture | — | ❌ No implementado (depende de RF5) |
| RF7 | US-002 | CU-002 | Clocking & Attendance Capture | `GET /api/auditoria` (evento `MarcajeRechazado`) | 🟡 Parcial (cubre geofence; no hay evento de liveness porque no existe) |

### Módulo 2 — Horarios y turnos

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF8 | US-004 | CU-004 | Scheduling & Shift Management | `GET/POST/PUT/DELETE /api/turnos` | 🟡 Parcial (tipos Fijo/Nocturno/Flexible con tolerancia; sin patrón de rotación explícito ni on-call) |
| RF9 | US-004 | CU-004 | Scheduling & Shift Management | `GET/POST/PUT/DELETE /api/turnos` (turno con horario que cruza medianoche) | 🟡 Parcial (el turno se configura, pero no hay motor de pre-nómina que prorratee horas entre los dos días calendario) |
| RF10 | US-005 | CU-005 | Scheduling & Shift Management | `POST /api/asignaciones-turno` | 🟡 Parcial (solo asignación individual; sin asignación masiva por grupo/departamento) |
| RF11 | US-005 | CU-005 | Scheduling & Shift Management | — | ❌ No implementado (sin `RestriccionHoraria` ni carga masiva) |

### Módulo 3 — Pre-nómina y motor de reglas

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF12 | US-006 | CU-006 | Pre-Payroll Engine | `GET /api/consulta-asistencias` (campo `indicadorPuntualidad`) | 🟡 Parcial (solo puntualidad Puntual/Tardío/SinTurnoAsignado; sin cálculo de horas trabajadas/extra) |
| RF13 | US-006 | CU-006 | Pre-Payroll Engine | — | ❌ No implementado (sin clasificación diurna/nocturna/festiva/dominical) |
| RF14 | US-006 | CU-006 | Pre-Payroll Engine | — | ❌ No implementado (sin motor de reglas configurable por entidad legal) |
| RF15 | US-007 | CU-007 | Pre-Payroll Engine | — | ❌ No implementado (sin recálculo de periodos) |

### Módulo 4 — Incidencias, justificaciones y permisos

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF16 | US-008 | CU-008 | Absence Management | — | ❌ No implementado |
| RF17 | US-008 | CU-008 | Absence Management | — | ❌ No implementado |
| RF18 | US-009 | CU-009 | Absence Management | — | ❌ No implementado |
| RF19 | US-009 | CU-009 | Absence Management | — | ❌ No implementado |
| RF20 | US-009 | CU-009 | Absence Management | — | ❌ No implementado |

> El módulo Absence Management no tiene ningún endpoint equivalente en el sistema implementado: no existen solicitudes, vacaciones, permisos ni workflow de aprobación en `specs/002-split-backend-frontend/contracts/`.

### Módulo 5 — Portal del Empleado (ESS)

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF21 | US-010 | CU-010 | Employee Self-Service | `GET /api/consulta-asistencias` | 🟡 Parcial (historial de marcas y puntualidad; sin acumulado de horas trabajadas ni resumen de incidencias) |
| RF22 | US-010 | CU-010 | Employee Self-Service | — | ❌ No implementado (sin `SaldoVacaciones`) |
| RF23 | US-010 | CU-010 | Employee Self-Service | — | ❌ No implementado (sin detección de marca omitida ni flujo de corrección) |
| RF24 | US-010 | CU-010 | Employee Self-Service | — | ❌ No implementado (sin notificaciones) |

### Módulo 6 — Portal del Supervisor (MSS)

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF25 | US-011 | CU-011 | Manager Self-Service | — | ❌ No implementado (sin panel de presencia en tiempo real) |
| RF26 | US-009 | CU-009 | Manager Self-Service / Absence Management | — | ❌ No implementado |
| RF27 | US-011b | CU-011b | Manager Self-Service | — | ❌ No implementado (sin `ReasignacionCobertura`) |
| RF28 | US-011b | CU-011b | Manager Self-Service | — | ❌ No implementado |

### Módulo 7 — Integración y exportación

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF29 | US-012 | CU-012 | Integration & Payroll Export | — | ❌ No implementado (sin exportación a ERP) |
| RF30 | US-012 | CU-012 | Integration & Payroll Export | — | ❌ No implementado |
| RF31 | US-012 | CU-012 | Integration & Payroll Export | — | ❌ No implementado (sin sincronización con directorio activo/HRIS) |

### Módulo 8 — Reportes y Business Intelligence

*Historias derivadas: no tienen HU/CU propios — se apoyan en US-006, US-007 y US-013 (ver `user-stories.md` §3).*

| RF | HU (derivada) | CU (derivado) | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF32 | US-006, US-007 | CU-006, CU-007 | Pre-Payroll Engine | `GET /api/consulta-asistencias/admin` (vista filtrable por empleado/centro/fecha) | 🟡 Parcial (tabla operativa, no un reporte formal por periodo/departamento) |
| RF33 | US-006, US-007 | CU-006, CU-007 | Pre-Payroll Engine | — | ❌ No implementado (sin KPIs de ausentismo/costo de horas extra) |
| RF34 | US-013 | CU-013 | Audit & Compliance | — | ❌ No implementado (sin exportación PDF/Excel/CSV de reportes) |

### Módulo 9 — Administración, seguridad y auditoría

| RF | HU | CU | Bounded Context | Endpoint (sistema implementado) | Estado |
|---|---|---|---|---|---|
| RF35 | US-002 (seguridad) | CU-002 | Identity (transversal) | `POST /api/auth/login` (roles `Administrador`/`Empleado` vía JWT) | 🟡 Parcial (2 roles vs. los 8 del modelo objetivo — `EMPLEADO`, `SUPERVISOR`, `ANALISTA_RRHH`, `ENCARGADO_NOMINA`, `ADMINISTRADOR`, `AUDITOR_CUMPLIMIENTO`, `TI_INTEGRACIONES`, `EJECUTIVO`) |
| RF36 | US-013 | CU-013 | Audit & Compliance | `GET /api/auditoria` (eventos `InicioSesionFallido`, `MarcajeRechazado`, `AccesoDenegadoPorRol`, con usuario/fecha/detalle) | ✅ Implementado |
| RF37 | US-013 | CU-013 | Audit & Compliance | `GET /api/auditoria` (solo lectura; no existen `PUT`/`DELETE` sobre este recurso) | ✅ Implementado (por diseño: el contrato no expone edición/borrado) |
| RF38 | US-002 | CU-002 | Clocking & Attendance Capture / Identity | `POST/PUT /api/empleados` (campo `consentimientoGeolocalizacion`) | 🟡 Parcial (solo consentimiento de geolocalización; no hay consentimiento biométrico distinto, ya que no hay biometría implementada) |

---

## Resumen de cobertura

| Estado | RF | Cantidad | % del total (38) |
|---|---|---|---|
| ✅ Implementado | RF2, RF3, RF36, RF37 | 4 | ~11% |
| 🟡 Parcial | RF1, RF7, RF8, RF9, RF10, RF12, RF21, RF32, RF35, RF38 | 10 | ~26% |
| ❌ No implementado | RF4, RF5, RF6, RF11, RF13, RF14, RF15, RF16–RF20, RF22, RF23, RF24, RF25–RF28, RF29–RF31, RF33, RF34 | 24 | ~63% |

Esta cobertura confirma cuantitativamente la advertencia del inicio del documento: el sistema implementado en este repositorio (`specs/002-split-backend-frontend`) es un **subconjunto reducido** (aproximadamente el módulo 1 "Marcaje" y una porción de los módulos 2, 5, 8 y 9) de la visión de 9 microservicios descrita en `docs/specs/functional/` y `docs/architecture/`.

## Documentos relacionados

- [`ENTREGABLES/requirements.md`](./requirements.md) — Catálogo completo de RF/RNF (fuente de la columna RF).
- [`ENTREGABLES/user-stories.md`](./user-stories.md) — Historias completas (fuente de la columna HU).
- [`ENTREGABLES/use-cases.md`](./use-cases.md) — Casos de uso (fuente de la columna CU).
- [`ENTREGABLES/technical-spec.md`](./technical-spec.md) — Arquitectura de bounded contexts (fuente de la columna Bounded Context).
- [`specs/002-split-backend-frontend/contracts/`](../specs/002-split-backend-frontend/contracts/) — Contratos de los endpoints citados en la columna Endpoint (sistema realmente implementado).
