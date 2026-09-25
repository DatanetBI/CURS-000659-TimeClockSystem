using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Auth;

/// <summary>
/// Registra en auditoría cualquier solicitud rechazada por autorización de rol (403), sin
/// importar el controller (FR-011, FR-003). Delega el resto del comportamiento al manejador
/// por defecto de ASP.NET Core.
/// </summary>
public class AuditingAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && context.User.Identity?.IsAuthenticated == true)
        {
            var auditoria = context.RequestServices.GetRequiredService<IAuditLogService>();
            var usuarioId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
            await auditoria.RegistrarAsync(
                EventoAuditoria.AccesoDenegadoPorRol, usuarioId,
                $"Acceso denegado a {context.Request.Method} {context.Request.Path}.", context.RequestAborted);
        }

        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
