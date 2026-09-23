# Data Model — TimeClockSystem v1.0

Derivado de la sección "Key Entities" de `spec.md`, acotado al alcance de v1.0 (ver `plan.md`
§"Alcance de v1.0 vs. v1.1"). Los campos y reglas citan el requisito funcional (`FR-XXX`) que los origina.
Este documento describe el modelo lógico; el esquema físico (tablas EF Core) es una tarea de
implementación (`/speckit-tasks`).

## Entidades de v1.0

### CentroTrabajo (Geofence)

Módulo 1.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| Nombre | texto | |
| CoordenadasCentro | coordenadas | |
| RadioMetros | entero > 0 | |

### Empleado

Módulo 2.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| NumeroEmpleado | texto, único | Usado en el marcaje por PIN (FR-002) |
| Nombre | texto | |
| CentroTrabajoId | referencia a CentroTrabajo | |
| Estado | activo / baja | |

**Reglas de validación**: `NumeroEmpleado` es único e inmutable. Un empleado en estado "baja" no puede
marcar asistencia.

### CredencialDeMarcaje

Módulo 2 (parte de Empleado).

| Campo | Tipo | Notas |
|---|---|---|
| EmpleadoId | referencia a Empleado | Relación 1:1 |
| PinHash | hash | Nunca en texto plano (Principio V de la constitución) |
| ActualizadoPor | referencia a Usuario Administrador | FR-004 |
| FechaActualizacion | fecha | |

**Reglas de validación**: PIN numérico de 4 a 6 dígitos. Solo un Administrador puede crear o restablecer
este valor (FR-004); sin autoservicio de restablecimiento en v1.0.

### Turno

Módulo 3.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| Tipo | fijo / rotativo / nocturno / flexible / on-call | FR-011 |
| HoraEntrada, HoraSalida | hora | Se guarda tal cual (el reparto de horas entre dos fechas para un turno nocturno queda diferido a v1.1) |
| DuracionReceso | minutos | |
| ToleranciaMinutos | entero ≥ 0 | Debe ser menor a la duración total del turno (FR-015) |

**Reglas de validación**: `ToleranciaMinutos` no puede ser negativo ni mayor que la duración del turno.

### AsignaciónTurno

Módulo 3.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EmpleadoId | referencia a Empleado | |
| TurnoId | referencia a Turno | |
| Fecha / RangoFechas | fecha(s) | |

**Reglas de validación**: La asignación masiva a un grupo de empleados reporta cuántos fueron asignados
(FR-013/014); la exclusión por restricciones horarias individuales aprobadas queda diferida a v1.1
(depende de Incidencias, que no existe todavía en v1.0).

### DiaFestivo

Módulo 4.

| Campo | Tipo | Notas |
|---|---|---|
| Fecha | fecha, única | |
| Descripcion | texto | |

Catálogo simple; usado solo como dato informativo en el Portal de consulta (módulo 6), sin factores de
pago asociados (esos se agregan con el motor de pre-nómina en v1.1).

### Marca de Asistencia

Módulo 5.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EmpleadoId | referencia a Empleado | |
| Tipo | entrada / salida / inicio_receso / fin_receso | FR-001 |
| Canal | portal_web / pin | FR-001, FR-002 (app móvil nativa y terminal RFID físico quedan fuera de v1.0 — no hay app nativa en esta versión) |
| Timestamp | fecha-hora | |
| Geolocalizacion | coordenadas (opcional) | Requerida para validar geofence (FR-006) |
| Estado | `Valida` \| `Rechazada` | Sin estado `PendienteSincronizacion` — no hay modo offline en v1.0 |
| MotivoRechazo | texto (si Rechazada) | geofence / entrada_duplicada / credenciales_invalidas |

**Reglas de validación**:
- No se acepta una entrada si ya existe una entrada abierta sin su salida correspondiente (FR-005).
- Una marca con geolocalización fuera del geofence configurado se rechaza y genera un evento de
  seguridad (FR-006, FR-010).
- Si la petición de marcaje falla por falta de conectividad o indisponibilidad del servidor, no se
  guarda nada localmente (a diferencia de la v1.1): el empleado ve un error y reintenta.

**Cálculo simple para el Portal de consulta (módulo 6)**: por cada marca de entrada, se compara su hora
contra la hora de entrada del turno asignado ese día más su tolerancia, para mostrar un indicador
"puntual" / "tardío" — sin clasificar tipos de hora extra ni calcular montos (eso es el motor de
pre-nómina, diferido).

### Usuario (Identity)

Módulo 2 (Administrador) / transversal.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único (ASP.NET Core Identity) | |
| Rol | `Empleado` \| `Administrador` | Ver `plan.md` §"Simplificación de roles para v1.0" |
| EmpleadoId | referencia a Empleado (si Rol = Empleado) | |

## Relaciones (v1.0)

```text
CentroTrabajo 1---N Empleado
Empleado 1---1 CredencialDeMarcaje
Empleado 1---N AsignaciónTurno N---1 Turno
Empleado 1---N Marca
Usuario 0..1---1 Empleado
```

## Entidades diferidas a v1.1 (no se modelan en esta versión)

Se listan solo para trazabilidad con `spec.md` — su diseño detallado se retoma cuando se planifique la
v1.1: **Solicitud**, **Aprobación**, **Incidencia**, **SaldoDeVacaciones**, **ConsolidadoDePeriodo
(Pre-nómina)**, **Exportación/Importación**, **RegistroDeAuditoría**. Ninguna tabla ni pantalla para
estas entidades se crea en v1.0.
