# Contract: Marcaje

Base path: `/api/marcaje`

## POST /api/marcaje

Registra una marca (Entrada/Salida/InicioReceso/FinReceso) para el empleado autenticado (canal
`PortalWeb`) o para el empleado identificado por PIN (canal `Pin`).

- **Auth**: Empleado (canal `PortalWeb`, usa el empleado de la sesión) o público con credencial PIN
  válida (canal `Pin`, ver `POST /api/marcaje/pin`).
- **Request**: `{ tipo: "Entrada"|"Salida"|"InicioReceso"|"FinReceso", latitud?: double, longitud?: double }`
- **Response 200**: `{ aceptada: true, marcaId, timestamp }`
- **Response 200 (rechazada)**: `{ aceptada: false, motivo: "Geofence"|"EntradaDuplicada"|"CredencialesInvalidas"|"SinConsentimientoGeolocalizacion" }`
  — el Backend MUST registrar un `RegistroAuditoria` (`MarcajeRechazado`) con el motivo (FR-011).
- **Notas de red**: el Frontend hace un único intento sin reintento automático (FR-012); el Backend
  no necesita una clave de idempotencia en esta iteración (research.md #6).

## POST /api/marcaje/pin

Variante para kiosco: identifica al empleado por número de empleado + PIN en vez de sesión.

- **Auth**: ninguna (canal kiosco compartido)
- **Request**: `{ numeroEmpleado: string, pin: string, tipo, latitud?, longitud? }`
- **Response**: igual forma que `POST /api/marcaje`.
- **Response 200 (rechazada, `CredencialesInvalidas`)**: PIN o número de empleado incorrectos — se
  registra en auditoría igual que cualquier otro rechazo.
