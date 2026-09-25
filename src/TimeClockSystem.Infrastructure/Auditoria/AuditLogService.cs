using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Auditoria;

/// <summary>Persiste y consulta el log de auditoría vía EF Core (FR-011, FR-011a).</summary>
public class AuditLogService(ApplicationDbContext db) : IAuditLogService
{
    public async Task RegistrarAsync(EventoAuditoria evento, string usuarioOEmpleadoId, string detalle, CancellationToken cancellationToken = default)
    {
        db.RegistrosAuditoria.Add(new RegistroAuditoria
        {
            Evento = evento,
            UsuarioOEmpleadoId = usuarioOEmpleadoId,
            Detalle = detalle,
            TimestampUtc = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConsultaAuditoriaPagina> ConsultarAsync(
        DateOnly? desde, DateOnly? hasta, string? usuario, int pagina, int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        var query = db.RegistrosAuditoria.AsNoTracking().AsQueryable();

        if (desde is not null)
        {
            var desdeUtc = desde.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(r => r.TimestampUtc >= desdeUtc);
        }
        if (hasta is not null)
        {
            var hastaUtc = hasta.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(r => r.TimestampUtc <= hastaUtc);
        }
        if (!string.IsNullOrWhiteSpace(usuario))
        {
            query = query.Where(r => r.UsuarioOEmpleadoId == usuario);
        }

        var total = await query.CountAsync(cancellationToken);
        var elementos = await query
            .OrderByDescending(r => r.TimestampUtc)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(r => new RegistroAuditoriaConsulta(r.Id, r.Evento, r.UsuarioOEmpleadoId, r.Detalle, r.TimestampUtc))
            .ToListAsync(cancellationToken);

        return new ConsultaAuditoriaPagina(total, elementos);
    }
}
