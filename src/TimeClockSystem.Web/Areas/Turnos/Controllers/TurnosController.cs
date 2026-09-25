using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.Turnos.Controllers;

[Area("Turnos")]
[Authorize(Roles = Roles.Administrador)]
public class TurnosController(TurnosApiClient turnosApi) : Controller
{
    public async Task<IActionResult> Index() =>
        View((await turnosApi.ListarAsync()).OrderBy(t => t.Nombre).ToList());

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

        if (!await turnosApi.CrearAsync(modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el turno.");
            return View(modelo);
        }

        TempData["Mensaje"] = $"Turno \"{modelo.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var turno = await turnosApi.ObtenerAsync(id);
        return turno is null ? NotFound() : View(turno);
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

        if (!await turnosApi.ActualizarAsync(id, modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el turno.");
            return View(modelo);
        }

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
