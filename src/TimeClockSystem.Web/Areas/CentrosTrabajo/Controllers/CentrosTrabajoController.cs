using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.CentrosTrabajo.Controllers;

[Area("CentrosTrabajo")]
[Authorize(Roles = Roles.Administrador)]
public class CentrosTrabajoController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var centros = await db.CentrosTrabajo.OrderBy(c => c.Nombre).ToListAsync();
        return View(centros);
    }

    public IActionResult Create() => View(new CentroTrabajo());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CentroTrabajo modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        db.CentrosTrabajo.Add(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"Centro de trabajo \"{modelo.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var centro = await db.CentrosTrabajo.FindAsync(id);
        if (centro is null)
        {
            return NotFound();
        }
        return View(centro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CentroTrabajo modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        db.CentrosTrabajo.Update(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"Centro de trabajo \"{modelo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
