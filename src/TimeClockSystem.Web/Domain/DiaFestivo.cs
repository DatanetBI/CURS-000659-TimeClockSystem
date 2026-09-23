using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Web.Domain;

/// <summary>
/// Catálogo simple de fechas festivas (data-model.md - DiaFestivo). Sin factores de pago
/// asociados en v1.0 — solo un dato informativo para el Portal de consulta (Módulo 6).
/// </summary>
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
