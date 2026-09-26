# Contract: Empleados

Base path: `/api/empleados`

Todos los endpoints requieren rol **Administrador**, salvo donde se indique lo contrario (FR-003).

## GET /api/empleados

Lista empleados (con datos de su CentroTrabajo).

- **Response 200**: `[{ id, numeroEmpleado, nombre, centroTrabajoId, centroTrabajoNombre, estado, consentimientoGeolocalizacion }]`

## GET /api/empleados/{id}

- **Response 200**: igual forma que un elemento de la lista anterior.
- **Response 404**: no existe.

## POST /api/empleados

Crea un empleado.

- **Request**: `{ numeroEmpleado, nombre, centroTrabajoId, consentimientoGeolocalizacion }`
- **Response 201**: empleado creado.
- **Response 400**: validaciones fallidas (mismas reglas que hoy: número/nombre requeridos, centro de trabajo existente).

## PUT /api/empleados/{id}

Actualiza un empleado (incluye cambio de `Estado` a `Baja`).

- **Request**: mismo shape que POST + `estado`.
- **Response 200 / 404 / 400**.

## GET /api/empleados/{id}/credencial

Consulta si el empleado tiene una `CredencialDeMarcaje` configurada (sin exponer el PIN/hash).

- **Response 200**: `{ tieneCredencial: bool, fechaActualizacion: datetime? }`

## PUT /api/empleados/{id}/credencial

Crea o actualiza el PIN de marcaje del empleado.

- **Request**: `{ pin: string }` (4-6 dígitos)
- **Response 204**: el Backend hashea el PIN antes de persistirlo (Principio V); nunca se devuelve el PIN ni su hash.
