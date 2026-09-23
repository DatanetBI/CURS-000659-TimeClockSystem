using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Areas.Turnos.Models;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.Turnos.Controllers;

/// <summary>
/// Asignación individual y masiva de turnos a empleados (FR-013, FR-014). En v1.0, la exclusión por
/// "restricción horaria individual aprobada" (FR-014) no tiene efecto real — depende de Incidencias,
/// diferido a v1.1 (plan.md) — por lo que el conteo de excluidos siempre es 0 en esta versión.
/// </summary>
[Area("Turnos")]
[Authorize(Roles = Roles.Administrador)]
public class AsignacionesController(ApplicationDbContext db) : Controller
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

        db.AsignacionesTurno.Add(new AsignacionTurno
        {
            EmpleadoId = modelo.EmpleadoId,
            TurnoId = modelo.TurnoId,
            Fecha = modelo.Fecha,
        });
        await db.SaveChangesAsync();

        TempData["Mensaje"] = "Turno asignado correctamente.";
        return RedirectToAction(nameof(Individual));
    }

    public async Task<IActionResult> Masiva()
    {
        await CargarListasAsync();
        ViewBag.Empleados = await db.Empleados.OrderBy(e => e.NumeroEmpleado).ToListAsync();
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
            ViewBag.Empleados = await db.Empleados.OrderBy(e => e.NumeroEmpleado).ToListAsync();
            return View(modelo);
        }

        var asignados = 0;
        var excluidos = 0; // v1.0: siempre 0 — ver nota de la clase.

        foreach (var empleadoId in modelo.EmpleadoIdsSeleccionados)
        {
            db.AsignacionesTurno.Add(new AsignacionTurno
            {
                EmpleadoId = empleadoId,
                TurnoId = modelo.TurnoId,
                Fecha = modelo.Fecha,
            });
            asignados++;
        }

        await db.SaveChangesAsync();

        TempData["Mensaje"] = $"Asignación masiva completada: {asignados} empleado(s) asignados, {excluidos} excluido(s).";
        return RedirectToAction(nameof(Masiva));
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Turnos = new SelectList(await db.Turnos.OrderBy(t => t.Nombre).ToListAsync(), "Id", "Nombre");
        ViewBag.EmpleadosIndividual = new SelectList(
            await db.Empleados.OrderBy(e => e.NumeroEmpleado).ToListAsync(), "Id", "NumeroEmpleado");
    }
}
