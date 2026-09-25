using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TimeClockSystem.Application.Abstractions;

namespace TimeClockSystem.Infrastructure.Auth;

/// <summary>
/// Emite el token de expiración fija que el Frontend adjunta a cada llamada al Backend
/// (FR-002, sin renovación silenciosa). Incluye el claim <c>empleadoId</c> cuando la cuenta está
/// vinculada a un Empleado, para que Marcaje/ConsultaAsistencias resuelvan "el empleado de la
/// sesión" sin volver a consultar por usuario (data-model.md - Usuario/Rol, contracts/auth.md).
/// </summary>
public class JwtTokenService(JwtOptions options) : ITokenService
{
    public TokenEmitido Emitir(CuentaAutenticada cuenta)
    {
        var expiraEnUtc = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, cuenta.UserId),
            new(ClaimTypes.Name, cuenta.Nombre),
            new(ClaimTypes.Role, cuenta.Rol),
        ];
        if (cuenta.EmpleadoId is { } empleadoId)
        {
            claims.Add(new Claim("empleadoId", empleadoId.ToString()));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiraEnUtc,
            signingCredentials: signingCredentials);

        return new TokenEmitido(new JwtSecurityTokenHandler().WriteToken(token), expiraEnUtc);
    }
}
