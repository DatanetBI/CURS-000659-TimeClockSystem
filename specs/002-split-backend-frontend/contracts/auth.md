# Contract: Autenticación

Base path: `/api/auth`

## POST /api/auth/login

Autentica a un usuario existente y emite el token que el Frontend usará en cada llamada posterior.

- **Auth**: ninguna (endpoint público)
- **Request**: `{ "usuario": string, "contrasena": string }`
- **Response 200**: `{ "token": string, "expiraEnUtc": datetime, "rol": "Administrador" | "Empleado", "nombre": string, "empleadoId": int? }`
- **Response 401**: credenciales inválidas — el Backend MUST registrar un `RegistroAuditoria`
  (`InicioSesionFallido`) con el usuario intentado (FR-011).
- **Notas**: el token tiene expiración fija (FR-002); no existe endpoint de refresh. Un
  `InicioSesionExitoso` también se registra en auditoría.
- **Claims del token**: además del rol, el token MUST incluir un claim `empleadoId` cuando el
  `ApplicationUser` autenticado está vinculado a un Empleado (equivalente al
  `ApplicationUser.EmpleadoId` actual). Los endpoints que resuelven "el empleado de la sesión"
  (`POST /api/marcaje` canal `PortalWeb`, `GET /api/consulta-asistencias` para rol Empleado) MUST
  leer este claim en vez de volver a consultar por nombre de usuario (ver data-model.md —
  Usuario/Rol).

## POST /api/auth/logout

Invalida la sesión del lado del Frontend (no requiere revocar el token del lado del Backend, ya que
es de expiración corta y fija).

- **Auth**: Bearer token válido
- **Response 204**: sin contenido
