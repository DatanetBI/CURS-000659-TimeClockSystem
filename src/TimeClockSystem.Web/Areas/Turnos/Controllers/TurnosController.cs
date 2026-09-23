using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.Turnos.Controllers;

[Area("Turnos")]
[Authorize(Roles = Roles.Administrador)]
public class TurnosController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var turnos = await db.Turnos.OrderBy(t => t.Nombre).ToListAsync();
        return View(turnos);
    }

    public IActionResult Create() => View(new Turno());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Turno modelo)
    {
        ValidarTolerancia(modelo);
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        db.Turnos.Add(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"Turno \"{modelo.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var turno = await db.Turnos.FindAsync(id);
        if (turno is null)
        {
            return NotFound();
        }
        return View(turno);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Turno modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        ValidarTolerancia(modelo);
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        db.Turnos.Update(modelo);
        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"Turno \"{modelo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidarTolerancia(Turno modelo)
    {
        if (modelo.ToleranciaMinutos < 0 || modelo.ToleranciaMinutos >= modelo.DuracionTotalMinutos())
        {
            ModelState.AddModelError(
                nameof(Turno.ToleranciaMinutos),
                "La tolerancia no puede ser negativa ni mayor o igual que la duración del turno.");
        }
    }
}
