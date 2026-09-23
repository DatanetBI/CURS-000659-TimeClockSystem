using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.Empleados.Controllers;

[Area("Empleados")]
[Authorize(Roles = Roles.Administrador)]
public class EmpleadosController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var empleados = await db.Empleados
            .Include(e => e.CentroTrabajo)
            .OrderBy(e => e.NumeroEmpleado)
            .ToListAsync();
        return View(empleados);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCentrosDeTrabajoAsync();
        return View(new Empleado());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Empleado modelo)
    {
        if (await NumeroEmpleadoYaExisteAsync(modelo.NumeroEmpleado, modelo.Id))
        {
            ModelState.AddModelError(nameof(Empleado.NumeroEmpleado), "Ya existe un empleado con este número.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCentrosDeTrabajoAsync();
            return View(modelo);
        }

        db.Empleados.Add(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"Empleado \"{modelo.Nombre}\" creado correctamente. Ahora puedes asignarle un PIN de marcaje.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var empleado = await db.Empleados.FindAsync(id);
        if (empleado is null)
        {
            return NotFound();
        }
        await CargarCentrosDeTrabajoAsync();
        return View(empleado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Empleado modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        if (await NumeroEmpleadoYaExisteAsync(modelo.NumeroEmpleado, modelo.Id))
        {
            ModelState.AddModelError(nameof(Empleado.NumeroEmpleado), "Ya existe un empleado con este número.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCentrosDeTrabajoAsync();
            return View(modelo);
        }

        db.Empleados.Update(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"Empleado \"{modelo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> NumeroEmpleadoYaExisteAsync(string numeroEmpleado, int idActual) =>
        await db.Empleados.AnyAsync(e => e.NumeroEmpleado == numeroEmpleado && e.Id != idActual);

    private async Task CargarCentrosDeTrabajoAsync()
    {
        ViewBag.CentrosTrabajo = new SelectList(
            await db.CentrosTrabajo.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre");
    }
}
