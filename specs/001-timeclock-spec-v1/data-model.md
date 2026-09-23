# Data Model — TimeClockSystem v1.0

Derivado de la sección "Key Entities" de `spec.md`. Los campos y reglas citan el requisito funcional
(`FR-XXX`) o el criterio de aceptación (`CL-XX`) que los origina. Este documento describe el modelo
lógico; el esquema físico (tablas EF Core) es una tarea de implementación (`/speckit-tasks`).

## Empleado

Persona cuya asistencia se controla.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| NumeroEmpleado | texto, único | Identificador usado en el marcaje por PIN (FR-002) |
| Nombre | texto | |
| FechaIngreso | fecha | Base para el cálculo de antigüedad y saldo de vacaciones (FR-021) |
| CentroTrabajoId | referencia a Geofence/Centro de Trabajo | |
| RolId(es) | referencia a Rol | Un empleado puede tener uno o más roles (FR-043) |
| Estado | activo / baja | Actualizado por importación de altas/bajas (FR-038, CL6) |
| ConsentimientoGeolocalizacion | booleano + fecha | Debe ser verdadero antes de aceptar una marca con geolocalización (FR-046, CL9) |
| ConsentimientoBiometrico | booleano + fecha | Reservado para cuando se habilite un proveedor biométrico real (FR-007/FR-046) |

**Reglas de validación**: `NumeroEmpleado` es único e inmutable. Un empleado en estado "baja" no puede
registrar nuevas marcas ni enviar nuevas solicitudes (CL6).

## Credencial de Marcaje

PIN de acceso rápido para marcar sin biometría.

| Campo | Tipo | Notas |
|---|---|---|
| EmpleadoId | referencia a Empleado | Relación 1:1 |
| PinHash | hash | Nunca se guarda en texto plano (Principio V de la constitución) |
| ActualizadoPor | referencia a Usuario (RRHH/Admin) | Quién asignó/restableció el PIN (FR-004) |
| FechaActualizacion | fecha | |

**Reglas de validación**: El PIN es numérico de 4 a 6 dígitos (aclaración de `spec.md`). Solo RRHH o un
Administrador puede crear o restablecer este valor (FR-004); no existe autoservicio de restablecimiento
en v1.

## Turno

Plantilla de horario.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| Tipo | fijo / rotativo / nocturno / flexible / on-call | FR-011 |
| HoraEntrada, HoraSalida | hora | Puede cruzar la medianoche (FR-012) |
| DuracionReceso | minutos | |
| ToleranciaMinutos | entero ≥ 0 | Debe ser menor a la duración total del turno (FR-015) |

**Reglas de validación**: `ToleranciaMinutos` no puede ser negativo ni mayor que la duración del turno
(FR-015). Un turno nocturno que cruza la medianoche distribuye sus horas entre las dos fechas calendario
al calcular el consolidado (FR-012).

## AsignaciónTurno

Vínculo entre un Empleado (o grupo) y un Turno para una fecha o rango de fechas.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EmpleadoId | referencia a Empleado | |
| TurnoId | referencia a Turno | |
| Fecha / RangoFechas | fecha(s) | |
| OrigenAsignacionMasiva | booleano + referencia al lote | Para el resumen de FR-013 (asignados/excluidos) |

**Reglas de validación**: Una asignación masiva respeta las restricciones horarias individuales
aprobadas de un empleado (p. ej. médicas) y lo excluye del lote si hay conflicto (FR-014); el resumen de
la operación reporta cuántos empleados fueron asignados y cuántos excluidos.

## Marca de Asistencia

Evento de entrada, salida, inicio o fin de receso.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EmpleadoId | referencia a Empleado | |
| Tipo | entrada / salida / inicio_receso / fin_receso | FR-001 |
| Canal | RFID / portal_web / app_movil / terminal_fisico / pin | FR-001, FR-002 |
| Timestamp | fecha-hora | |
| Geolocalizacion | coordenadas (opcional) | Requerida para validar geofence (FR-006) |
| Estado | `PendienteSincronizacion` → `Valida` \| `Rechazada` | Ver máquina de estados abajo |
| MotivoRechazo | texto (si Rechazada) | geofence / entrada_duplicada / credenciales_invalidas |

**Máquina de estados**:

```text
PendienteSincronizacion --(conectividad restablecida, credenciales y geofence OK)--> Valida
PendienteSincronizacion --(conectividad restablecida, falla validación)--> Rechazada
(captura online) --(geofence OK, sin entrada duplicada)--> Valida
(captura online) --(geofence falla o entrada duplicada)--> Rechazada
```

**Reglas de validación**:
- No se acepta una entrada si ya existe una entrada abierta sin su salida correspondiente (FR-005).
- Una marca con geolocalización fuera del geofence configurado se rechaza y genera un evento de
  seguridad (FR-006, FR-010).
- Una marca duplicada (mismo empleado, tipo y ventana de tiempo) se descarta en la sincronización sin
  duplicar el conteo de horas (FR-009, CL2).
- El estado `PendienteSincronizacion` cubre tanto la falta de conectividad del dispositivo como la
  indisponibilidad temporal del sistema central (FR-008, CL14).

## Geofence / Centro de Trabajo

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| Nombre | texto | |
| CoordenadasCentro | coordenadas | |
| RadioMetros | entero > 0 | |

## Calendario de Festivos

| Campo | Tipo | Notas |
|---|---|---|
| Fecha | fecha, única | |
| Descripcion | texto | |

Usado por el motor de pre-nómina para clasificar un día completo trabajado como "hora festiva"
(FR-017, RN6 de `spec.md`).

## Consolidado de Periodo (Pre-nómina)

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EmpleadoId | referencia a Empleado | |
| Periodo | rango de fechas | |
| HorasTrabajadas, Atrasos, Ausencias | numérico | FR-016 |
| HorasExtraDiurnas/Nocturnas/Festivas/Dominicales | numérico | FR-017 |
| Estado | calculado / recalculado / exportado | |

**Reglas de validación**: Se recalcula automáticamente si se aprueba, después del cierre, una solicitud
que afecta una incidencia de ese periodo (FR-019, CL5).

## Incidencia

Registro por día que alimenta el Consolidado: atraso, ausencia justificada/injustificada, hora extra.

| Campo | Tipo | Notas |
|---|---|---|
| EmpleadoId, Fecha | | |
| Tipo | atraso / ausencia_justificada / ausencia_injustificada / hora_extra | FR-026 |
| SolicitudRelacionadaId | referencia a Solicitud (opcional) | Justifica una ausencia (FR-026) |

## Solicitud

Permiso, vacaciones, incapacidad o día compensatorio.

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EmpleadoId | referencia a Empleado | |
| Tipo | vacaciones / permiso_con_goce / permiso_sin_goce / incapacidad / compensatorio | FR-020 |
| FechaInicio, FechaFin | fecha | Puede ser retroactiva (CL8) |
| Comprobantes | archivo(s) adjuntos | Obligatorio para incapacidad médica (FR-022) |
| Estado | `Pendiente` → `Aprobada` \| `Rechazada` \| `Escalada` | Ver máquina de estados |
| MotivoRechazo | texto (si Rechazada) | FR-025 |

**Máquina de estados**:

```text
Pendiente --(aprobador decide antes del plazo)--> Aprobada
Pendiente --(aprobador decide antes del plazo)--> Rechazada
Pendiente --(vence el plazo configurado, por defecto 3 días hábiles)--> Escalada
Escalada --(siguiente nivel decide)--> Aprobada
Escalada --(siguiente nivel decide)--> Rechazada
```

**Reglas de validación**:
- Una solicitud de vacaciones que excede el saldo disponible se rechaza antes de llegar a aprobación
  (FR-021, RN7).
- Una solicitud de incapacidad médica exige al menos un comprobante adjunto antes de poder enviarse
  (FR-022).
- El estado `Escalada` conserva un vínculo al nivel de aprobación siguiente (FR-024).

## Aprobación

| Campo | Tipo | Notas |
|---|---|---|
| SolicitudId | referencia a Solicitud | |
| Nivel | supervisor / RRHH | FR-023 |
| Decision | aprobada / rechazada | |
| Motivo | texto (si rechazada) | FR-025 |
| AprobadorId, Fecha | | |

## Saldo de Vacaciones

| Campo | Tipo | Notas |
|---|---|---|
| EmpleadoId | referencia a Empleado | |
| DiasDisponibles, DiasReservados, DiasUtilizados | numérico | FR-021, FR-028 |

**Reglas de validación**: `DiasDisponibles` se recalcula a partir de la antigüedad del empleado según la
tabla de la Ley Federal del Trabajo (incluida la reforma de "vacaciones dignas"); RRHH puede corregirlo
manualmente en casos excepcionales (ver Assumptions de `spec.md`).

## Exportación / Importación

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| Tipo | exportacion_nomina / importacion_altas_bajas | FR-036, FR-038 |
| Periodo o LoteOrigen | | |
| Estado | `Pendiente` → `Exitosa` \| `Fallida` | Ver máquina de estados |
| Intentos | entero | Máximo configurable, por defecto 3 (FR-037) |
| ArchivoGenerado / ArchivoRecibido | ruta o referencia | |

**Máquina de estados**:

```text
Pendiente --(se genera/procesa el archivo correctamente)--> Exitosa
Pendiente --(falla, intentos restantes > 0)--> Pendiente (reintento)
Pendiente --(falla, intentos agotados)--> Fallida
```

## Rol

| Campo | Tipo | Notas |
|---|---|---|
| Nombre | empleado / supervisor / rrhh / nomina / administrador / auditor / ti / ejecutivo | FR-043 |
| Permisos | conjunto de permisos por área funcional | |

## Registro de Auditoría

| Campo | Tipo | Notas |
|---|---|---|
| Id | identificador único | |
| EntidadAfectada, EntidadId | | Marca, Turno o Solicitud modificados manualmente (FR-044) |
| UsuarioId, Fecha | | |
| ValorAnterior, ValorNuevo | texto/JSON | |

**Reglas de validación**: Ninguna operación de la aplicación expone `UPDATE` ni `DELETE` sobre esta
tabla, para ningún rol, incluido Administrador (FR-045). Se filtra por empleado o por periodo (FR-047).

## Relaciones principales

```text
Empleado 1---1 CredencialDeMarcaje
Empleado 1---N Marca
Empleado 1---N AsignaciónTurno N---1 Turno
Empleado 1---1 SaldoDeVacaciones
Empleado 1---N Solicitud 1---N Aprobación
Solicitud 0..1---N Incidencia
Empleado 1---N ConsolidadoDePeriodo 1---N Incidencia
CentroTrabajo(Geofence) 1---N Empleado
```
