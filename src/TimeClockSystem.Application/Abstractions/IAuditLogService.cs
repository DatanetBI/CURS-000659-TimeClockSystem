using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public record RegistroAuditoriaConsulta(int Id, EventoAuditoria Evento, string UsuarioOEmpleadoId, string Detalle, DateTime TimestampUtc);

public record ConsultaAuditoriaPagina(int Total, IReadOnlyList<RegistroAuditoriaConsulta> Elementos);

/// <summary>
/// Registra y consulta eventos sensibles (FR-011, FR-011a): inicios de sesión, marcajes
/// rechazados y accesos denegados por rol.
/// </summary>
public interface IAuditLogService
{
    Task RegistrarAsync(EventoAuditoria evento, string usuarioOEmpleadoId, string detalle, CancellationToken cancellationToken = default);

    Task<ConsultaAuditoriaPagina> ConsultarAsync(
        DateOnly? desde, DateOnly? hasta, string? usuario, int pagina, int tamanoPagina,
        CancellationToken cancellationToken = default);
}
