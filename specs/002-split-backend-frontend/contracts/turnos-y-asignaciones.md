# Contract: Turnos y Asignaciones de Turno

Todos los endpoints requieren rol **Administrador** (FR-003).

## Turnos — base path: `/api/turnos`

### GET /api/turnos
- **Response 200**: `[{ id, nombre, tipo, horaEntrada, horaSalida, duracionRecesoMinutos, toleranciaMinutos, duracionTotalMinutos }]`

### GET /api/turnos/{id}
- **Response 200 / 404**.

### POST /api/turnos
- **Request**: `{ nombre, tipo, horaEntrada, horaSalida, duracionRecesoMinutos, toleranciaMinutos }`
- **Response 201 / 400**.

### PUT /api/turnos/{id}
- **Response 200 / 404 / 400**.

### DELETE /api/turnos/{id}
- **Response 204 / 404 / 409** (si tiene asignaciones vigentes).

## Asignaciones de Turno — base path: `/api/asignaciones-turno`

### GET /api/asignaciones-turno?empleadoId=&fecha=
- **Response 200**: `[{ id, empleadoId, empleadoNombre, turnoId, turnoNombre, fecha }]`

### POST /api/asignaciones-turno
- **Request**: `{ empleadoId, turnoId, fecha }`
- **Response 201 / 400** (empleado/turno deben existir).

### DELETE /api/asignaciones-turno/{id}
- **Response 204 / 404**.
