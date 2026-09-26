namespace TimeClockSystem.Application.Abstractions;

/// <summary>
/// Identidad resuelta tras un inicio de sesión exitoso (FR-002). <c>EmpleadoId</c> viaja luego
/// como claim en el token (data-model.md - Usuario/Rol, contracts/auth.md) para que Marcaje y
/// ConsultaAsistencias resuelvan "el empleado de la sesión" sin volver a consultar por usuario.
/// </summary>
public record CuentaAutenticada(string UserId, string Nombre, string Rol, int? EmpleadoId);

/// <summary>
/// Resuelve usuario/contraseña contra el almacén de Identity (implementado en Infrastructure).
/// </summary>
public interface IUserAccountService
{
    Task<CuentaAutenticada?> AutenticarAsync(string usuario, string contrasena, CancellationToken cancellationToken = default);
}

/// <summary>
/// Token de expiración fija emitido tras un login exitoso (FR-002: sin renovación silenciosa).
/// </summary>
public record TokenEmitido(string Token, DateTime ExpiraEnUtc);

public interface ITokenService
{
    TokenEmitido Emitir(CuentaAutenticada cuenta);
}
