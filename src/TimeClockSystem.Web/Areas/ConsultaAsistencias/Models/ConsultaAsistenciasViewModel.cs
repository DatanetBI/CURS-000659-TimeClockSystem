using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Areas.ConsultaAsistencias.Models;

public class FiltroConsultaViewModel
{
    public int? EmpleadoId { get; set; }
    public int? CentroTrabajoId { get; set; }
    public DateOnly? Fecha { get; set; }
}

public class FilaConsultaViewModel
{
    public string NumeroEmpleado { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string CentroTrabajo { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public TipoMarca Tipo { get; set; }
    public EstadoMarca Estado { get; set; }
    public MotivoRechazoMarca MotivoRechazo { get; set; }
    public IndicadorPuntualidad? Puntualidad { get; set; }
    public bool EsDiaFestivo { get; set; }
}

public class ConsultaAsistenciasViewModel
{
    public FiltroConsultaViewModel Filtro { get; set; } = new();
    public IReadOnlyList<FilaConsultaViewModel> Resultados { get; set; } = [];
}
