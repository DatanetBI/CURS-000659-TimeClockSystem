using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Areas.Auditoria.Models;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;

namespace TimeClockSystem.Web.Areas.Auditoria.Controllers;

/// <summary>
/// Pantalla de solo lectura para que un Administrador consulte el log de auditoría, filtrando por
/// fecha y por usuario (FR-011a, SC-006), sin necesidad de herramientas técnicas (Principio IV).
/// </summary>
[Area("Auditoria")]
[Authorize(Roles = Roles.Administrador)]
public class AuditoriaController(AuditoriaApiClient auditoriaApi) : Controller
{
    public async Task<IActionResult> Index(DateOnly? desde, DateOnly? hasta, string? usuario, int pagina = 1)
    {
        var resultado = await auditoriaApi.ConsultarAsync(desde, hasta, usuario, pagina);

        return View(new AuditoriaViewModel
        {
            Filtro = new FiltroAuditoriaViewModel { Desde = desde, Hasta = hasta, Usuario = usuario, Pagina = pagina },
            Total = resultado.Total,
            Elementos = resultado.Elementos.Select(e => new FilaAuditoriaViewModel
            {
                Evento = e.Evento,
                UsuarioOEmpleadoId = e.UsuarioOEmpleadoId,
                Detalle = e.Detalle,
                TimestampUtc = e.TimestampUtc,
            }).ToList(),
        });
    }
}
