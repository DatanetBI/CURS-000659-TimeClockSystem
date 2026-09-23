using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Areas.Marcaje.Models;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;

namespace TimeClockSystem.Web.Areas.Marcaje.Controllers;

/// <summary>
/// Marcaje por número de empleado + PIN, modo kiosco: sin sesión de portal completa (FR-002,
/// FR-003). Disponible en un terminal compartido.
/// </summary>
[Area("Marcaje")]
[AllowAnonymous]
public class PinMarcajeController(ApplicationDbContext db, MarcajeService marcajeService) : Controller
{
    private static readonly PasswordHasher<Empleado> PinHasher = new();

    public IActionResult Index() => View(new MarcajePinViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MarcajePinViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var empleado = await db.Empleados
            .Include(e => e.CredencialDeMarcaje)
            .FirstOrDefaultAsync(e => e.NumeroEmpleado == modelo.NumeroEmpleado);

        if (empleado?.CredencialDeMarcaje is null ||
            PinHasher.VerifyHashedPassword(empleado, empleado.CredencialDeMarcaje.PinHash, modelo.Pin)
                == PasswordVerificationResult.Failed)
        {
            // FR-003: mensaje genérico, sin indicar cuál de los dos datos falló.
            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            return View(modelo);
        }

        var resultado = await marcajeService.RegistrarAsync(
            empleado.Id, modelo.Tipo, CanalMarca.Pin, modelo.Latitud, modelo.Longitud);

        ViewBag.ResultadoAceptado = resultado.Aceptada;
        ViewBag.NombreEmpleado = empleado.Nombre;
        ViewBag.HoraMarca = resultado.Marca?.Timestamp;
        ViewBag.MotivoRechazo = resultado.Motivo;

        return View("Resultado");
    }
}
