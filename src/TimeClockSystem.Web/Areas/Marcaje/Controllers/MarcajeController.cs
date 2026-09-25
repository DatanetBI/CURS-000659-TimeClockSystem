using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Areas.Marcaje.Models;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.Marcaje.Controllers;

/// <summary>
/// Registro de asistencia desde una sesión de portal autenticada (FR-001, FR-005, FR-006, FR-046).
/// </summary>
[Area("Marcaje")]
[Authorize(Roles = Roles.Empleado)]
public class MarcajeController(MarcajeApiClient marcajeApi, ConsultaAsistenciasApiClient consultaApi) : Controller
{
    public async Task<IActionResult> Index()
    {
        var empleadoId = ObtenerEmpleadoIdActual();
        if (empleadoId is null)
        {
            return NotFound("Tu cuenta no está vinculada a ningún empleado.");
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var marcasDeHoy = (await consultaApi.ConsultarAsync(empleadoId.Value, hoy, hoy))
            .OrderByDescending(m => m.Timestamp)
            .ToList();

        var ultimaValida = marcasDeHoy.FirstOrDefault(m => m.Estado == EstadoMarca.Valida);

        return View(new MarcajeIndexViewModel
        {
            NombreEmpleado = User.Identity?.Name ?? string.Empty,
            TieneEntradaAbierta = ultimaValida is { Tipo: TipoMarca.Entrada },
            MarcasDeHoy = marcasDeHoy.Select(m => new Marca
            {
                Id = m.MarcaId,
                Tipo = m.Tipo,
                Canal = m.Canal,
                Timestamp = m.Timestamp,
                Estado = m.Estado,
                MotivoRechazo = m.MotivoRechazo,
            }).ToList(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarMarcaViewModel modelo)
    {
        if (ObtenerEmpleadoIdActual() is null)
        {
            return NotFound();
        }

        MarcajeApiResponse? resultado;
        try
        {
            resultado = await marcajeApi.RegistrarAsync(modelo.Tipo, modelo.Latitud, modelo.Longitud);
        }
        catch (HttpRequestException)
        {
            // FR-012: un único intento, sin reintento automático — se informa de inmediato.
            TempData["Error"] = "No se pudo registrar tu marca: no hay conexión con el servidor. Inténtalo de nuevo.";
            return RedirectToAction(nameof(Index));
        }

        if (resultado is null)
        {
            TempData["Error"] = "No se pudo registrar tu marca. Inténtalo de nuevo.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = resultado.Aceptada
            ? $"Marca registrada: {modelo.Tipo} a las {resultado.Timestamp:HH:mm}."
            : null;

        if (!resultado.Aceptada)
        {
            TempData["Error"] = DescribirMotivo(resultado.Motivo);
        }

        return RedirectToAction(nameof(Index));
    }

    private int? ObtenerEmpleadoIdActual() =>
        int.TryParse(User.FindFirst("empleadoId")?.Value, out var id) ? id : null;

    private static string DescribirMotivo(MotivoRechazoMarca? motivo) => motivo switch
    {
        MotivoRechazoMarca.EntradaDuplicada => "Ya tienes una entrada abierta sin salida registrada.",
        MotivoRechazoMarca.Geofence => "Tu marca fue rechazada: no estás dentro del perímetro autorizado de tu centro de trabajo.",
        MotivoRechazoMarca.SinConsentimientoGeolocalizacion => "No podemos registrar tu marca: falta tu consentimiento para usar tu ubicación.",
        _ => "No se pudo registrar la marca.",
    };
}
