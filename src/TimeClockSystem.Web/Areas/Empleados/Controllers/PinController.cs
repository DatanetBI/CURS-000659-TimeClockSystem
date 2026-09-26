using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Areas.Empleados.Models;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;

namespace TimeClockSystem.Web.Areas.Empleados.Controllers;

/// <summary>
/// Asignar/restablecer el PIN de marcaje de un empleado (FR-004). Solo Administrador/RRHH — en v1.0,
/// solo Administrador (plan.md - Simplificación de roles).
/// </summary>
[Area("Empleados")]
[Authorize(Roles = Roles.Administrador)]
public class PinController(EmpleadosApiClient empleadosApi) : Controller
{
    public async Task<IActionResult> AsignarPin(int empleadoId)
    {
        var empleado = await empleadosApi.ObtenerAsync(empleadoId);
        if (empleado is null)
        {
            return NotFound();
        }

        return View(new AsignarPinViewModel { EmpleadoId = empleado.Id, NombreEmpleado = empleado.Nombre });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarPin(AsignarPinViewModel modelo)
    {
        var empleado = await empleadosApi.ObtenerAsync(modelo.EmpleadoId);
        if (empleado is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            modelo.NombreEmpleado = empleado.Nombre;
            return View(modelo);
        }

        await empleadosApi.ActualizarCredencialAsync(modelo.EmpleadoId, modelo.Pin);
        TempData["Mensaje"] = $"PIN de marcaje de \"{empleado.Nombre}\" actualizado correctamente.";
        return RedirectToAction("Index", "Empleados");
    }
}
