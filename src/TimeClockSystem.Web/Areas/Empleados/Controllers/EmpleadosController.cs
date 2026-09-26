using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.Empleados.Controllers;

[Area("Empleados")]
[Authorize(Roles = Roles.Administrador)]
public class EmpleadosController(EmpleadosApiClient empleadosApi, CentrosTrabajoApiClient centrosTrabajoApi) : Controller
{
    public async Task<IActionResult> Index() =>
        View((await empleadosApi.ListarAsync()).OrderBy(e => e.NumeroEmpleado).ToList());

    public async Task<IActionResult> Create()
    {
        await CargarCentrosDeTrabajoAsync();
        return View(new Empleado());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Empleado modelo)
    {
        if (!ModelState.IsValid)
        {
            await CargarCentrosDeTrabajoAsync();
            return View(modelo);
        }

        if (!await empleadosApi.CrearAsync(modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el empleado (verifica que el número de empleado no esté repetido).");
            await CargarCentrosDeTrabajoAsync();
            return View(modelo);
        }

        TempData["Mensaje"] = $"Empleado \"{modelo.Nombre}\" creado correctamente. Ahora puedes asignarle un PIN de marcaje.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var empleado = await empleadosApi.ObtenerAsync(id);
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

        if (!ModelState.IsValid)
        {
            await CargarCentrosDeTrabajoAsync();
            return View(modelo);
        }

        if (!await empleadosApi.ActualizarAsync(id, modelo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el empleado (verifica que el número de empleado no esté repetido).");
            await CargarCentrosDeTrabajoAsync();
            return View(modelo);
        }

        TempData["Mensaje"] = $"Empleado \"{modelo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCentrosDeTrabajoAsync()
    {
        ViewBag.CentrosTrabajo = new SelectList(await centrosTrabajoApi.ListarAsync(), "Id", "Nombre");
    }
}
