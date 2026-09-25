# Contract: Auditoría *(nuevo — FR-011, FR-011a)*

Base path: `/api/auditoria`

## GET /api/auditoria?desde=&hasta=&usuario=&pagina=&tamanoPagina=

- **Auth**: Administrador únicamente (FR-011a).
- **Response 200**: `{ total: int, elementos: [{ id, evento, usuarioOEmpleadoId, detalle, timestampUtc }] }`
- **Response 403**: si lo intenta un Empleado — este intento en sí también se registra como
  `AccesoDenegadoPorRol` (FR-011).

## Consumo en el Frontend

La nueva Area `Auditoria` (solo visible para Administrador) llama a este endpoint para mostrar una
tabla filtrable por fecha y por usuario, cumpliendo SC-006 (verificable sin herramientas técnicas).
