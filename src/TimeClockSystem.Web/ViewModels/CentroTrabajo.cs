using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Web.ViewModels;

public class CentroTrabajo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La latitud es obligatoria.")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    [Display(Name = "Latitud")]
    public double Latitud { get; set; }

    [Required(ErrorMessage = "La longitud es obligatoria.")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    [Display(Name = "Longitud")]
    public double Longitud { get; set; }

    [Required(ErrorMessage = "El radio del geofence es obligatorio.")]
    [Range(1, 100_000, ErrorMessage = "El radio debe ser mayor que 0 metros.")]
    [Display(Name = "Radio del geofence (metros)")]
    public int RadioMetros { get; set; }
}
