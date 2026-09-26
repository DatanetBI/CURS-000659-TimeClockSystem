using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeClockSystem.Domain;

public enum EstadoEmpleado
{
    Activo,
    Baja,
}

/// <summary>
/// Persona cuya asistencia se controla (data-model.md - Empleado).
/// </summary>
public class Empleado
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El número de empleado es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Número de empleado")]
    public string NumeroEmpleado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El centro de trabajo es obligatorio.")]
    [Display(Name = "Centro de trabajo")]
    public int CentroTrabajoId { get; set; }

    [ForeignKey(nameof(CentroTrabajoId))]
    public CentroTrabajo? CentroTrabajo { get; set; }

    [Display(Name = "Estado")]
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    /// <summary>
    /// Debe ser true antes de aceptar una marca con geolocalización (FR-046, CL9).
    /// </summary>
    [Display(Name = "Consiente el uso de su geolocalización para marcar asistencia")]
    public bool ConsentimientoGeolocalizacion { get; set; }

    public CredencialDeMarcaje? CredencialDeMarcaje { get; set; }
}
