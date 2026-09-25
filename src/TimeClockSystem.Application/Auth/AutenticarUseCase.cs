using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Auth;

public record LoginResultado(bool Exitoso, string? Token, DateTime? ExpiraEnUtc, string? Rol, string? Nombre, int? EmpleadoId);

/// <summary>
/// Autentica usuario/contraseña y emite el token de expiración fija (FR-002, contracts/auth.md),
/// registrando en auditoría tanto los inicios de sesión exitosos como los fallidos (FR-011).
/// </summary>
public class AutenticarUseCase(IUserAccountService cuentas, ITokenService tokens, IAuditLogService auditoria)
{
    public async Task<LoginResultado> LoginAsync(string usuario, string contrasena, CancellationToken cancellationToken = default)
    {
        var cuenta = await cuentas.AutenticarAsync(usuario, contrasena, cancellationToken);
        if (cuenta is null)
        {
            await auditoria.RegistrarAsync(EventoAuditoria.InicioSesionFallido, usuario, "Credenciales inválidas.", cancellationToken);
            return new LoginResultado(false, null, null, null, null, null);
        }

        var emitido = tokens.Emitir(cuenta);
        await auditoria.RegistrarAsync(EventoAuditoria.InicioSesionExitoso, cuenta.UserId, $"Inicio de sesión exitoso (rol {cuenta.Rol}).", cancellationToken);

        return new LoginResultado(true, emitido.Token, emitido.ExpiraEnUtc, cuenta.Rol, cuenta.Nombre, cuenta.EmpleadoId);
    }
}
