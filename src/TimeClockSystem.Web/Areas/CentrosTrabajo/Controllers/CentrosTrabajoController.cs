using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.CentrosTrabajo.Controllers;

[Area("CentrosTrabajo")]
[Authorize(Roles = Roles.Administrador)]
public class CentrosTrabajoController(CentrosTrabajoApiClient centrosTrabajoApi) : Controller
{
    public async Task<IActionResult> Index() =>
        View((await centrosTrabajoApi.ListarAsync()).OrderBy(c => c.Nombre).ToList());

    public IActionResult Create() => View(new CentroTrabajo());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CentroTrabajo modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        if (!await centrosTrabajoApi.CrearAsync(modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el centro de trabajo.");
            return View(modelo);
        }

        TempData["Mensaje"] = $"Centro de trabajo \"{modelo.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var centro = await centrosTrabajoApi.ObtenerAsync(id);
        return centro is null ? NotFound() : View(centro);
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

        if (!await centrosTrabajoApi.ActualizarAsync(id, modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el centro de trabajo.");
            return View(modelo);
        }

        TempData["Mensaje"] = $"Centro de trabajo \"{modelo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
