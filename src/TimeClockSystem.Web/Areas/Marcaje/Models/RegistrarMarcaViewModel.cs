using System.ComponentModel.DataAnnotations;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.Marcaje.Models;

public class RegistrarMarcaViewModel
{
    [Required]
    public TipoMarca Tipo { get; set; }

    /// <summary>Poblados por JavaScript (geolocalización del navegador); pueden venir vacíos.</summary>
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
}

public class MarcajeIndexViewModel
{
    public string NombreEmpleado { get; set; } = string.Empty;
    public bool TieneEntradaAbierta { get; set; }
    public IReadOnlyList<Marca> MarcasDeHoy { get; set; } = [];
}
