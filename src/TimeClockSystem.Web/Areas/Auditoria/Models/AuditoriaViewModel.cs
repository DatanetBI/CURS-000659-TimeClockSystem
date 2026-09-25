namespace TimeClockSystem.Web.Areas.Auditoria.Models;

public class FiltroAuditoriaViewModel
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public string? Usuario { get; set; }
    public int Pagina { get; set; } = 1;
}

public class FilaAuditoriaViewModel
{
    public string Evento { get; set; } = string.Empty;
    public string UsuarioOEmpleadoId { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
}

public class AuditoriaViewModel
{
    public FiltroAuditoriaViewModel Filtro { get; set; } = new();
    public IReadOnlyList<FilaAuditoriaViewModel> Elementos { get; set; } = [];
    public int Total { get; set; }
    private const int TamanoPagina = 25;
    public int TotalPaginas => Total == 0 ? 1 : (int)Math.Ceiling(Total / (double)TamanoPagina);
}
