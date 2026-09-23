using Microsoft.AspNetCore.Identity;

namespace TimeClockSystem.Web.Infrastructure.Identity;

/// <summary>
/// Cuenta de portal (Identity). Vinculada a un Empleado cuando el usuario tiene el rol "Empleado".
/// Ver data-model.md - Usuario (Identity).
/// </summary>
public class ApplicationUser : IdentityUser
{
    public int? EmpleadoId { get; set; }
}
