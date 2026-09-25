# Contract: Autenticación

Base path: `/api/auth`

## POST /api/auth/login

Autentica a un usuario existente y emite el token que el Frontend usará en cada llamada posterior.

- **Auth**: ninguna (endpoint público)
- **Request**: `{ "usuario": string, "contrasena": string }`
- **Response 200**: `{ "token": string, "expiraEnUtc": datetime, "rol": "Administrador" | "Empleado", "nombre": string }`
- **Response 401**: credenciales inválidas — el Backend MUST registrar un `RegistroAuditoria`
  (`InicioSesionFallido`) con el usuario intentado (FR-011).
- **Notas**: el token tiene expiración fija (FR-002); no existe endpoint de refresh. Un
  `InicioSesionExitoso` también se registra en auditoría.

## POST /api/auth/logout

Invalida la sesión del lado del Frontend (no requiere revocar el token del lado del Backend, ya que
es de expiración corta y fija).

- **Auth**: Bearer token válido
- **Response 204**: sin contenido
