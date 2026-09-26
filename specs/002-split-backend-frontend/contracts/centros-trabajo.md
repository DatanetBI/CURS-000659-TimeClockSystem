# Contract: Centros de Trabajo

Base path: `/api/centros-trabajo`

Todos los endpoints requieren rol **Administrador** (FR-003).

## GET /api/centros-trabajo

- **Response 200**: `[{ id, nombre, latitud, longitud, radioMetros }]`

## GET /api/centros-trabajo/{id}

- **Response 200 / 404**.

## POST /api/centros-trabajo

- **Request**: `{ nombre, latitud, longitud, radioMetros }`
- **Response 201 / 400** (rangos: latitud [-90,90], longitud [-180,180], radioMetros [1,100000]).

## PUT /api/centros-trabajo/{id}

- **Request**: mismo shape que POST.
- **Response 200 / 404 / 400**.

## DELETE /api/centros-trabajo/{id}

- **Response 204 / 404**.
- **Response 409**: si tiene empleados asignados (misma regla que hoy en el Frontend).
