using System.ComponentModel.DataAnnotations;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.Marcaje.Models;

public class MarcajePinViewModel
{
    [Required(ErrorMessage = "El número de empleado es obligatorio.")]
    [Display(Name = "Número de empleado")]
    public string NumeroEmpleado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El PIN es obligatorio.")]
    [DataType(DataType.Password)]
    [Display(Name = "PIN")]
    public string Pin { get; set; } = string.Empty;

    [Required]
    public TipoMarca Tipo { get; set; }

    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
}
