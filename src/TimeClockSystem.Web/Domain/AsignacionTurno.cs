using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeClockSystem.Web.Domain;

/// <summary>
/// Vínculo entre un Empleado y un Turno para una fecha (data-model.md - AsignaciónTurno).
/// </summary>
public class AsignacionTurno
{
    public int Id { get; set; }

    [Required]
    public int EmpleadoId { get; set; }

    [ForeignKey(nameof(EmpleadoId))]
    public Empleado? Empleado { get; set; }

    [Required]
    public int TurnoId { get; set; }

    [ForeignKey(nameof(TurnoId))]
    public Turno? Turno { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateOnly Fecha { get; set; }
}
