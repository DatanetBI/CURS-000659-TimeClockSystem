using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Areas.Marcaje.Models;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.Marcaje.Controllers;

/// <summary>
/// Marcaje por número de empleado + PIN, modo kiosco: sin sesión de portal completa (FR-002,
/// FR-003). Disponible en un terminal compartido.
/// </summary>
[Area("Marcaje")]
[AllowAnonymous]
public class PinMarcajeController(MarcajeApiClient marcajeApi) : Controller
{
    public IActionResult Index() => View(new MarcajePinViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MarcajePinViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await marcajeApi.RegistrarPorPinAsync(modelo.NumeroEmpleado, modelo.Pin, modelo.Tipo, modelo.Latitud, modelo.Longitud);

        if (resultado is null || (!resultado.Aceptada && resultado.Motivo == MotivoRechazoMarca.CredencialesInvalidas))
        {
            // FR-003: mensaje genérico, sin indicar cuál de los dos datos falló.
            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            return View(modelo);
        }

        ViewBag.ResultadoAceptado = resultado.Aceptada;
        ViewBag.NombreEmpleado = resultado.EmpleadoNombre ?? modelo.NumeroEmpleado;
        ViewBag.HoraMarca = resultado.Timestamp;
        ViewBag.MotivoRechazo = resultado.Motivo;

        return View("Resultado");
    }
}
