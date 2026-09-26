using Microsoft.AspNetCore.Identity;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Infrastructure.Identity;

namespace TimeClockSystem.Infrastructure.Auth;

/// <summary>Resuelve usuario/contraseña contra ASP.NET Core Identity Core (research.md #2).</summary>
public class UserAccountService(UserManager<ApplicationUser> userManager) : IUserAccountService
{
    public async Task<CuentaAutenticada?> AutenticarAsync(string usuario, string contrasena, CancellationToken cancellationToken = default)
    {
        var cuenta = await userManager.FindByNameAsync(usuario);
        if (cuenta is null || !await userManager.CheckPasswordAsync(cuenta, contrasena))
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(cuenta);
        var rol = roles.FirstOrDefault();
        if (rol is null)
        {
            return null;
        }

        return new CuentaAutenticada(cuenta.Id, cuenta.UserName ?? usuario, rol, cuenta.EmpleadoId);
    }
}
