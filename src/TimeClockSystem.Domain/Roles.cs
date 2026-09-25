namespace TimeClockSystem.Domain;

/// <summary>
/// Nombres canónicos de los roles del sistema (research.md #2, FR-003). Compartidos por
/// Application (autorización de negocio) e Infrastructure (creación de roles de Identity).
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";

    public static readonly string[] Todos = [Administrador, Empleado];
}
