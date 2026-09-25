namespace TimeClockSystem.Web.Infrastructure.Auth;

/// <summary>
/// Copia propia del Frontend de los nombres de rol (idénticos a `TimeClockSystem.Domain.Roles`).
/// El Frontend no referencia el proyecto Domain del Backend (research.md #4), por lo que estas
/// constantes se duplican intencionalmente aquí.
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";
}
