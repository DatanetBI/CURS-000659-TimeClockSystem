using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeClockSystem.Domain;

public enum TipoMarca
{
    Entrada,
    Salida,
    InicioReceso,
    FinReceso,
}

public enum CanalMarca
{
    PortalWeb,
    Pin,
}

public enum EstadoMarca
{
    Valida,
    Rechazada,
}

public enum MotivoRechazoMarca
{
    Ninguno,
    Geofence,
    EntradaDuplicada,
    CredencialesInvalidas,
    SinConsentimientoGeolocalizacion,
}

/// <summary>
/// Evento de entrada/salida/receso (data-model.md - Marca de Asistencia). Sin estado
/// "PendienteSincronizacion" en v1.0 — no hay modo offline (research.md #5).
/// </summary>
public class Marca
{
    public int Id { get; set; }

    [Required]
    public int EmpleadoId { get; set; }

    [ForeignKey(nameof(EmpleadoId))]
    public Empleado? Empleado { get; set; }

    public TipoMarca Tipo { get; set; }

    public CanalMarca Canal { get; set; }

    public DateTime Timestamp { get; set; }

    public double? Latitud { get; set; }

    public double? Longitud { get; set; }

    public EstadoMarca Estado { get; set; }

    public MotivoRechazoMarca MotivoRechazo { get; set; } = MotivoRechazoMarca.Ninguno;
}
