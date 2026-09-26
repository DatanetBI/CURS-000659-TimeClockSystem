using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TimeClockSystem.Web.Infrastructure;

/// <summary>
/// Captura cualquier falla de red al llamar al Backend (indisponible, timeout) en cualquier
/// Controller y muestra un mensaje claro y no técnico en vez de la excepción cruda (FR-009, SC-004).
/// </summary>
public class BackendUnavailableExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not (HttpRequestException or TaskCanceledException))
        {
            return;
        }

        context.ExceptionHandled = true;
        context.Result = new ViewResult { ViewName = "~/Views/Shared/BackendNoDisponible.cshtml" };
        context.HttpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
    }
}
