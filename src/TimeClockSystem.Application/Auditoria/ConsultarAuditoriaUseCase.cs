using TimeClockSystem.Application.Abstractions;

namespace TimeClockSystem.Application.Auditoria;

/// <summary>Consulta paginada del log de auditoría, solo para Administrador (contracts/auditoria.md, FR-011a).</summary>
public class ConsultarAuditoriaUseCase(IAuditLogService auditoria)
{
    private const int TamanoPaginaPorDefecto = 25;
    private const int TamanoPaginaMaximo = 100;

    public Task<ConsultaAuditoriaPagina> ConsultarAsync(
        DateOnly? desde, DateOnly? hasta, string? usuario, int pagina, int? tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        var tamano = Math.Clamp(tamanoPagina ?? TamanoPaginaPorDefecto, 1, TamanoPaginaMaximo);
        var paginaValida = Math.Max(pagina, 1);
        return auditoria.ConsultarAsync(desde, hasta, usuario, paginaValida, tamano, cancellationToken);
    }
}
