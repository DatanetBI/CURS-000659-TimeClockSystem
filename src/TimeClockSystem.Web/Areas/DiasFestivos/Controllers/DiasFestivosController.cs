using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.DiasFestivos.Controllers;

[Area("DiasFestivos")]
[Authorize(Roles = Roles.Administrador)]
public class DiasFestivosController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var festivos = await db.DiasFestivos.OrderBy(f => f.Fecha).ToListAsync();
        return View(festivos);
    }

    public IActionResult Create() => View(new DiaFestivo());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiaFestivo modelo)
    {
        if (await db.DiasFestivos.AnyAsync(f => f.Fecha == modelo.Fecha))
        {
            ModelState.AddModelError(nameof(DiaFestivo.Fecha), "Ya existe un día festivo registrado en esa fecha.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        db.DiasFestivos.Add(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = "Día festivo agregado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
