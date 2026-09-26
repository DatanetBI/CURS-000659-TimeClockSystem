# Data Model: Separación de TimeClockSystem en Backend WebAPI y Frontend MVC

Todas las entidades ya existen hoy en `TimeClockSystem.Web/Domain/` y se relocalizan sin cambios de
campos ni de comportamiento a `TimeClockSystem.Domain` (FR-005), salvo `RegistroAuditoria`, que es
nueva (FR-011). Los tipos y validaciones aquí descritos son los que ya están implementados; este
documento no introduce cambios de esquema adicionales.

## Empleado

Persona cuya asistencia se controla.

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| NumeroEmpleado | string(20) | Requerido |
| Nombre | string(150) | Requerido |
| CentroTrabajoId | int | Requerido, FK → CentroTrabajo |
| Estado | enum (`Activo`, `Baja`) | Default `Activo` |
| ConsentimientoGeolocalizacion | bool | Debe ser `true` antes de aceptar una marca con geolocalización |

Relaciones: 1 CentroTrabajo → N Empleado; 1 Empleado → 0..1 CredencialDeMarcaje; 1 Empleado → N Marca; 1 Empleado → N AsignacionTurno.

## CentroTrabajo

Geofence / ubicación autorizada para el marcaje de un grupo de empleados.

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| Nombre | string(120) | Requerido |
| Latitud | double | Requerido, rango [-90, 90] |
| Longitud | double | Requerido, rango [-180, 180] |
| RadioMetros | int | Requerido, rango [1, 100000] |

## Turno

Plantilla de horario asignable a empleados.

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| Nombre | string(80) | Requerido |
| Tipo | enum (`Fijo`, `Rotativo`, `Nocturno`, `Flexible`, `OnCall`) | Default `Fijo` |
| HoraEntrada | TimeSpan | Requerido |
| HoraSalida | TimeSpan | Requerido |
| DuracionRecesoMinutos | int | Rango [0, 480] |
| ToleranciaMinutos | int | Rango [0, 1440] |

Comportamiento: `DuracionTotalMinutos()` calcula la duración total considerando turnos que cruzan
medianoche (`HoraSalida` < `HoraEntrada`).

## AsignacionTurno

Vínculo entre un Empleado y un Turno para una fecha.

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| EmpleadoId | int | Requerido, FK → Empleado |
| TurnoId | int | Requerido, FK → Turno |
| Fecha | DateOnly | Requerido |

## DiaFestivo

Catálogo simple de fechas festivas (informativo, sin factores de pago asociados).

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| Fecha | DateOnly | Requerido |
| Descripcion | string(120) | Requerido |

## Marca

Evento de entrada/salida/receso de un Empleado.

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| EmpleadoId | int | Requerido, FK → Empleado |
| Tipo | enum (`Entrada`, `Salida`, `InicioReceso`, `FinReceso`) | Requerido |
| Canal | enum (`PortalWeb`, `Pin`) | Requerido |
| Timestamp | DateTime | Requerido |
| Latitud / Longitud | double? | Opcionales; requeridos si el canal captura geolocalización |
| Estado | enum (`Valida`, `Rechazada`) | Requerido |
| MotivoRechazo | enum (`Ninguno`, `Geofence`, `EntradaDuplicada`, `CredencialesInvalidas`, `SinConsentimientoGeolocalizacion`) | Default `Ninguno` |

Reglas de negocio asociadas (sin cambios, relocalizadas a `TimeClockSystem.Domain`):
- `GeofenceValidator.EstaDentroDelGeofence` — distancia Haversine contra el `CentroTrabajo` del Empleado.
- `ConsentimientoValidator.PuedeCapturarGeolocalizacion` — bloquea la captura si no hay consentimiento.
- `PuntualidadCalculator.Calcular` — compara la marca de entrada contra el turno asignado + tolerancia.

## CredencialDeMarcaje

Número de empleado + PIN que permite marcar sin biometría (relación 1:1 con Empleado).

| Campo | Tipo | Reglas |
|---|---|---|
| EmpleadoId | int | Clave primaria y FK → Empleado |
| PinHash | string | Requerido; el PIN nunca se guarda en texto plano (Principio V) |
| ActualizadoPorUserId | string? | Opcional |
| FechaActualizacion | DateTime | Requerido |

## RegistroAuditoria *(nueva — FR-011)*

Evento sensible registrado por el Backend para trazabilidad ante disputas laborales.

| Campo | Tipo | Reglas |
|---|---|---|
| Id | int | Clave primaria |
| Evento | enum (`InicioSesionExitoso`, `InicioSesionFallido`, `MarcajeRechazado`, `AccesoDenegadoPorRol`) | Requerido |
| UsuarioOEmpleadoId | string | Requerido; identifica quién intentó la acción |
| Detalle | string(500) | Qué se intentó (ej. recurso, motivo de rechazo) |
| Timestamp | DateTime (UTC) | Requerido |

Relaciones: ninguna FK dura hacia Empleado/Usuario (se conserva el identificador como texto para no
perder el registro si la cuenta se elimina más adelante). Consultable solo por el rol Administrador,
filtrable por rango de fecha y por usuario (FR-011a).

## Usuario/Rol (Identity)

Sin cambios de forma respecto a hoy: `ApplicationUser` (Identity) con roles `Administrador` y
`Empleado` (`Roles.Todos`), y su vínculo existente `ApplicationUser.EmpleadoId` hacia `Empleado`
(usado hoy por `MarcajeController.ObtenerEmpleadoActualAsync`). Se relocaliza a
`TimeClockSystem.Infrastructure.Identity`; el Frontend ya no tiene su propio almacén de Identity
(ver research.md #2).

**Propagación de `EmpleadoId` vía token**: dado que el Backend ya no comparte proceso ni base de
datos de Identity con el Frontend, el token emitido por `POST /api/auth/login` MUST incluir un claim
`empleadoId` (cuando el `ApplicationUser` autenticado está vinculado a un Empleado). Los endpoints
que hoy resuelven "el empleado de la sesión actual" (`POST /api/marcaje` canal `PortalWeb`,
`GET /api/consulta-asistencias` para el rol Empleado) MUST leer ese claim directamente del token en
vez de volver a consultar por nombre de usuario, preservando el mismo comportamiento que
`ObtenerEmpleadoActualAsync` tiene hoy (ver contracts/auth.md).
