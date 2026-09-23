namespace TimeClockSystem.Web.Infrastructure.Identity;

/// <summary>
/// v1.0 solo define dos roles (ver plan.md - Simplificacion de roles para v1.0).
/// Los demas roles de FR-043 se agregan junto con la funcionalidad que los requiere, en v1.1.
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";

    public static readonly string[] Todos = [Administrador, Empleado];
}
