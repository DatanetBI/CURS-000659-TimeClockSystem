# Contract: Días Festivos

Base path: `/api/dias-festivos`

Lectura disponible para **Administrador** y **Empleado** (el Portal de consulta los muestra a
ambos); escritura solo para **Administrador** (FR-003).

## GET /api/dias-festivos

- **Auth**: Administrador o Empleado
- **Response 200**: `[{ id, fecha, descripcion }]`

## POST /api/dias-festivos

- **Auth**: Administrador
- **Request**: `{ fecha, descripcion }`
- **Response 201 / 400**.

## PUT /api/dias-festivos/{id}

- **Auth**: Administrador
- **Response 200 / 404 / 400**.

## DELETE /api/dias-festivos/{id}

- **Auth**: Administrador
- **Response 204 / 404**.
