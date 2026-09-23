using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Areas.Empleados.Models;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.Empleados.Controllers;

/// <summary>
/// Asignar/restablecer el PIN de marcaje de un empleado (FR-004). Solo Administrador/RRHH — en v1.0,
/// solo Administrador (plan.md - Simplificación de roles).
/// </summary>
[Area("Empleados")]
[Authorize(Roles = Roles.Administrador)]
public class PinController(ApplicationDbContext db) : Controller
{
    private static readonly PasswordHasher<Empleado> PinHasher = new();

    public async Task<IActionResult> AsignarPin(int empleadoId)
    {
        var empleado = await db.Empleados.FindAsync(empleadoId);
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
        var empleado = await db.Empleados.FindAsync(modelo.EmpleadoId);
        if (empleado is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            modelo.NombreEmpleado = empleado.Nombre;
            return View(modelo);
        }

        var credencial = await db.CredencialesDeMarcaje.FindAsync(empleado.Id);
        var usuarioAdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (credencial is null)
        {
            credencial = new CredencialDeMarcaje { EmpleadoId = empleado.Id };
            db.CredencialesDeMarcaje.Add(credencial);
        }

        credencial.PinHash = PinHasher.HashPassword(empleado, modelo.Pin);
        credencial.ActualizadoPorUserId = usuarioAdminId;
        credencial.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync();
        TempData["Mensaje"] = $"PIN de marcaje de \"{empleado.Nombre}\" actualizado correctamente.";
        return RedirectToAction("Index", "Empleados");
    }
}
