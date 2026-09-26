using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Web.Areas.Empleados.Models;

public class AsignarPinViewModel
{
    public int EmpleadoId { get; set; }

    public string NombreEmpleado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El PIN es obligatorio.")]
    [RegularExpression(@"^\d{4,6}$", ErrorMessage = "El PIN debe ser numérico, de 4 a 6 dígitos.")]
    [Display(Name = "Nuevo PIN")]
    public string Pin { get; set; } = string.Empty;
}
