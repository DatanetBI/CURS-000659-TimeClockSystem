# Contract: Consulta de Asistencias

Base path: `/api/consulta-asistencias`

## GET /api/consulta-asistencias?empleadoId=&desde=&hasta=

Consulta las marcas de un empleado en un rango de fechas, con su indicador de puntualidad.

- **Auth**: Empleado (solo puede consultar su propio `empleadoId`, el Backend MUST validarlo contra
  el usuario autenticado) o Administrador (puede consultar cualquier empleado).
- **Response 200**: `[{ marcaId, tipo, canal, timestamp, estado, motivoRechazo, indicadorPuntualidad: "Puntual"|"Tardio"|"SinTurnoAsignado" }]`
- **Response 403**: un Empleado intentando consultar a otro empleado — el Backend MUST registrar un
  `RegistroAuditoria` (`AccesoDenegadoPorRol`) (FR-011).

## GET /api/consulta-asistencias/admin?empleadoId=&centroTrabajoId=&fecha=

Vista de Administrador que navega las marcas de todos los empleados (todos los filtros son
opcionales) — descubierto durante la implementación como el uso real de la pantalla existente
"Consulta de Asistencias" (a diferencia del endpoint anterior, pensado para el historial de un solo
empleado).

- **Auth**: Administrador únicamente.
- **Response 200**: `[{ numeroEmpleado, nombreEmpleado, centroTrabajo, timestamp, tipo, estado, motivoRechazo, puntualidad, esDiaFestivo }]` (máx. 200 filas, más recientes primero).
