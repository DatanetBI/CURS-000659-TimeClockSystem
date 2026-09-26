using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Web.Areas.Turnos.Models;

public class AsignacionIndividualViewModel
{
    [Required(ErrorMessage = "Selecciona un empleado.")]
    [Display(Name = "Empleado")]
    public int EmpleadoId { get; set; }

    [Required(ErrorMessage = "Selecciona un turno.")]
    [Display(Name = "Turno")]
    public int TurnoId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public class AsignacionMasivaViewModel
{
    [Required(ErrorMessage = "Selecciona un turno.")]
    [Display(Name = "Turno")]
    public int TurnoId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Centro de trabajo (opcional, filtra el grupo)")]
    public int? CentroTrabajoId { get; set; }

    public List<int> EmpleadoIdsSeleccionados { get; set; } = [];
}

public class AsignacionMasivaResultadoViewModel
{
    public int Asignados { get; set; }
    public int Excluidos { get; set; }
}
