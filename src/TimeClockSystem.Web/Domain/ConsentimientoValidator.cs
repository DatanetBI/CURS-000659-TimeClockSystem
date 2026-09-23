namespace TimeClockSystem.Web.Domain;

/// <summary>
/// Bloquea una marca con geolocalización si el empleado no ha otorgado su consentimiento
/// (FR-046, CL9).
/// </summary>
public static class ConsentimientoValidator
{
    public static bool PuedeCapturarGeolocalizacion(Empleado empleado) =>
        empleado.ConsentimientoGeolocalizacion;
}
