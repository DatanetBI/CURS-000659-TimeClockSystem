using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeClockSystem.Web.Areas.Turnos.Models;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;

namespace TimeClockSystem.Web.Areas.Turnos.Controllers;

/// <summary>
/// Asignación individual y masiva de turnos a empleados (FR-013, FR-014). En v1.0, la exclusión por
/// "restricción horaria individual aprobada" (FR-014) no tiene efecto real — depende de Incidencias,
/// diferido a v1.1 (plan.md) — por lo que el conteo de excluidos siempre es 0 en esta versión.
/// </summary>
[Area("Turnos")]
[Authorize(Roles = Roles.Administrador)]
public class AsignacionesController(
    AsignacionesTurnoApiClient asignacionesApi, TurnosApiClient turnosApi, EmpleadosApiClient empleadosApi) : Controller
{
    public async Task<IActionResult> Individual()
    {
        await CargarListasAsync();
        return View(new AsignacionIndividualViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Individual(AsignacionIndividualViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(modelo);
        }

        if (!await asignacionesApi.CrearAsync(modelo.EmpleadoId, modelo.TurnoId, modelo.Fecha))
        {
            ModelState.AddModelError(string.Empty, "No se pudo asignar el turno.");
            await CargarListasAsync();
            return View(modelo);
        }

        TempData["Mensaje"] = "Turno asignado correctamente.";
        return RedirectToAction(nameof(Individual));
    }

    public async Task<IActionResult> Masiva()
    {
        await CargarListasAsync();
        ViewBag.Empleados = (await empleadosApi.ListarAsync()).OrderBy(e => e.NumeroEmpleado).ToList();
        return View(new AsignacionMasivaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Masiva(AsignacionMasivaViewModel modelo)
    {
        if (!ModelState.IsValid || modelo.EmpleadoIdsSeleccionados.Count == 0)
        {
            if (modelo.EmpleadoIdsSeleccionados.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Selecciona al menos un empleado.");
            }
            await CargarListasAsync();
            ViewBag.Empleados = (await empleadosApi.ListarAsync()).OrderBy(e => e.NumeroEmpleado).ToList();
            return View(modelo);
        }

        var asignados = 0;
        var excluidos = 0; // v1.0: siempre 0 — ver nota de la clase.

        foreach (var empleadoId in modelo.EmpleadoIdsSeleccionados)
        {
            if (await asignacionesApi.CrearAsync(empleadoId, modelo.TurnoId, modelo.Fecha))
            {
                asignados++;
            }
            else
            {
                excluidos++;
            }
        }

        TempData["Mensaje"] = $"Asignación masiva completada: {asignados} empleado(s) asignados, {excluidos} excluido(s).";
        return RedirectToAction(nameof(Masiva));
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Turnos = new SelectList((await turnosApi.ListarAsync()).OrderBy(t => t.Nombre), "Id", "Nombre");
        ViewBag.EmpleadosIndividual = new SelectList((await empleadosApi.ListarAsync()).OrderBy(e => e.NumeroEmpleado), "Id", "NumeroEmpleado");
    }
}
