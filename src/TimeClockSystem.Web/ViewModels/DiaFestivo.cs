using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Web.ViewModels;

public class DiaFestivo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(120)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;
}
