using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeClockSystem.Web.Domain;

/// <summary>
/// Numero de empleado + PIN que permiten marcar sin biometria (data-model.md - CredencialDeMarcaje).
/// Relacion 1:1 con Empleado. El PIN nunca se guarda en texto plano (Principio V).
/// </summary>
public class CredencialDeMarcaje
{
    [Key]
    public int EmpleadoId { get; set; }

    [ForeignKey(nameof(EmpleadoId))]
    public Empleado? Empleado { get; set; }

    [Required]
    public string PinHash { get; set; } = string.Empty;

    public string? ActualizadoPorUserId { get; set; }

    public DateTime FechaActualizacion { get; set; }
}
