using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeClockSystem.Web.ViewModels;

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

    [Display(Name = "Consiente el uso de su geolocalización para marcar asistencia")]
    public bool ConsentimientoGeolocalizacion { get; set; }
}
