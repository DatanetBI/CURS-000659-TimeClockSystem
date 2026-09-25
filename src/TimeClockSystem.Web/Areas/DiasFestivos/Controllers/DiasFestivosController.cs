using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.DiasFestivos.Controllers;

[Area("DiasFestivos")]
[Authorize(Roles = Roles.Administrador)]
public class DiasFestivosController(DiasFestivosApiClient diasFestivosApi) : Controller
{
    public async Task<IActionResult> Index() =>
        View((await diasFestivosApi.ListarAsync()).OrderBy(f => f.Fecha).ToList());

    public IActionResult Create() => View(new DiaFestivo());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiaFestivo modelo)
    {
        if ((await diasFestivosApi.ListarAsync()).Any(f => f.Fecha == modelo.Fecha))
        {
            ModelState.AddModelError(nameof(DiaFestivo.Fecha), "Ya existe un día festivo registrado en esa fecha.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        if (!await diasFestivosApi.CrearAsync(modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo agregar el día festivo.");
            return View(modelo);
        }

        TempData["Mensaje"] = "Día festivo agregado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
