# Contract: Consulta de Asistencias

Base path: `/api/consulta-asistencias`

## GET /api/consulta-asistencias?empleadoId=&desde=&hasta=

Consulta las marcas de un empleado en un rango de fechas, con su indicador de puntualidad.

- **Auth**: Empleado (solo puede consultar su propio `empleadoId`, el Backend MUST validarlo contra
  el usuario autenticado) o Administrador (puede consultar cualquier empleado).
- **Response 200**: `[{ marcaId, tipo, canal, timestamp, estado, motivoRechazo, indicadorPuntualidad: "Puntual"|"Tardio"|"SinTurnoAsignado" }]`
- **Response 403**: un Empleado intentando consultar a otro empleado — el Backend MUST registrar un
  `RegistroAuditoria` (`AccesoDenegadoPorRol`) (FR-011).
