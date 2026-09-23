using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Areas.Marcaje.Models;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.Marcaje.Controllers;

/// <summary>
/// Registro de asistencia desde una sesión de portal autenticada (FR-001, FR-005, FR-006, FR-046).
/// </summary>
[Area("Marcaje")]
[Authorize(Roles = Roles.Empleado)]
public class MarcajeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, MarcajeService marcajeService)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var empleado = await ObtenerEmpleadoActualAsync();
        if (empleado is null)
        {
            return NotFound("Tu cuenta no está vinculada a ningún empleado.");
        }

        var hoy = DateTime.UtcNow.Date;
        var marcasDeHoy = await db.Marcas
            .Where(m => m.EmpleadoId == empleado.Id && m.Timestamp >= hoy)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        var ultimaValida = marcasDeHoy.FirstOrDefault(m => m.Estado == EstadoMarca.Valida);

        return View(new MarcajeIndexViewModel
        {
            NombreEmpleado = empleado.Nombre,
            TieneEntradaAbierta = ultimaValida is { Tipo: TipoMarca.Entrada },
            MarcasDeHoy = marcasDeHoy,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarMarcaViewModel modelo)
    {
        var empleado = await ObtenerEmpleadoActualAsync();
        if (empleado is null)
        {
            return NotFound();
        }

        var resultado = await marcajeService.RegistrarAsync(
            empleado.Id, modelo.Tipo, CanalMarca.PortalWeb, modelo.Latitud, modelo.Longitud);

        TempData["Mensaje"] = resultado.Aceptada
            ? $"Marca registrada: {modelo.Tipo} a las {resultado.Marca!.Timestamp:HH:mm}."
            : null;

        if (!resultado.Aceptada)
        {
            TempData["Error"] = DescribirMotivo(resultado.Motivo);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Empleado?> ObtenerEmpleadoActualAsync()
    {
        var usuario = await userManager.GetUserAsync(User);
        if (usuario?.EmpleadoId is null)
        {
            return null;
        }
        return await db.Empleados.Include(e => e.CentroTrabajo).FirstOrDefaultAsync(e => e.Id == usuario.EmpleadoId);
    }

    private static string DescribirMotivo(MotivoRechazoMarca motivo) => motivo switch
    {
        MotivoRechazoMarca.EntradaDuplicada => "Ya tienes una entrada abierta sin salida registrada.",
        MotivoRechazoMarca.Geofence => "Tu marca fue rechazada: no estás dentro del perímetro autorizado de tu centro de trabajo.",
        MotivoRechazoMarca.SinConsentimientoGeolocalizacion => "No podemos registrar tu marca: falta tu consentimiento para usar tu ubicación.",
        _ => "No se pudo registrar la marca.",
    };
}
