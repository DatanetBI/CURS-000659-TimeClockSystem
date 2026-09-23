# Contrato de Exportación / Importación (FR-036 a FR-039)

Este es el único contrato "externo" real de v1: el formato de archivo que un sistema de nómina/ERP o un
directorio activo/HRIS tendría que producir o consumir para integrarse con TimeClockSystem en una fase
posterior (ver `spec.md` §Aclaraciones — no hay conexión en vivo a un proveedor concreto en v1).

## Exportación: consolidado de horas e incidencias (FR-036)

**Formato**: CSV, codificación UTF-8, una fila por empleado y periodo.

| Columna | Tipo | Descripción |
|---|---|---|
| numero_empleado | texto | Identificador del empleado (`Empleado.NumeroEmpleado`) |
| periodo_inicio, periodo_fin | fecha (`AAAA-MM-DD`) | Rango del periodo exportado |
| horas_trabajadas | decimal | |
| horas_atraso | decimal | |
| horas_extra_diurnas | decimal | |
| horas_extra_nocturnas | decimal | |
| horas_extra_festivas | decimal | |
| horas_extra_dominicales | decimal | |
| dias_ausencia_justificada | entero | |
| dias_ausencia_injustificada | entero | |
| moneda | texto, siempre `MXN` | Constitución — Principio II |

**Generación**: automatizada y programada (`BackgroundService`); si falla, se reintenta según la
política configurada (por defecto 3 intentos — FR-037) antes de marcar el lote como "Fallido" y notificar
a TI. El estado de cada lote queda consultable en pantalla después de cada ejecución (FR-039).

## Importación: altas y bajas de empleados (FR-038)

**Formato**: CSV, codificación UTF-8, una fila por empleado.

| Columna | Tipo | Descripción |
|---|---|---|
| numero_empleado | texto, único | |
| nombre | texto | |
| fecha_ingreso | fecha (`AAAA-MM-DD`) | Base del cálculo de antigüedad/vacaciones |
| centro_trabajo | texto | Debe existir como Geofence/Centro de Trabajo ya configurado |
| rol | uno de: empleado, supervisor, rrhh, nomina, administrador, auditor, ti, ejecutivo | |
| accion | `alta` \| `baja` | |

**Reglas de procesamiento** (CL6, CL12):
- Una fila con `centro_trabajo` o `rol` inexistente se rechaza individualmente; el resto del archivo se
  procesa normalmente. El resultado de la carga reporta filas aceptadas y rechazadas con su motivo.
- Una `baja` para un empleado con una solicitud pendiente o un turno futuro asignado cancela/marca para
  revisión esas solicitudes y libera esos turnos (no falla silenciosamente).

## Fuera de este contrato

No se define aquí un protocolo de transporte (SFTP, API REST de un ERP específico, LDAP) — v1 solo
produce/consume el archivo. Conectar este contrato a un sistema concreto es trabajo de una fase
posterior (ver `spec.md` §Fuera de Alcance).
