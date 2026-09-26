using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.Auth;

namespace TimeClockSystem.Api.Controllers;

public record LoginRequest(string Usuario, string Contrasena);
public record LoginResponse(string Token, DateTime ExpiraEnUtc, string Rol, string Nombre, int? EmpleadoId);

/// <summary>Autenticación (contracts/auth.md).</summary>
[ApiController]
[Route("api/auth")]
public class AuthController(AutenticarUseCase autenticar) : ControllerBase
{
    /// <summary>Autentica usuario/contraseña y emite el token de expiración fija que el Frontend adjunta a cada llamada.</summary>
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await autenticar.LoginAsync(request.Usuario, request.Contrasena, cancellationToken);
        if (!resultado.Exitoso)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(resultado.Token!, resultado.ExpiraEnUtc!.Value, resultado.Rol!, resultado.Nombre!, resultado.EmpleadoId));
    }

    /// <summary>No-op del lado del Backend (el token es de expiración fija sin estado que revocar); existe por completitud del contrato.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout() => NoContent();
}
