using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class MarcaRepository(ApplicationDbContext db) : IMarcaRepository
{
    public async Task<bool> TieneEntradaAbiertaAsync(int empleadoId, CancellationToken cancellationToken = default)
    {
        var ultima = await db.Marcas
            .Where(m => m.EmpleadoId == empleadoId && m.Estado == EstadoMarca.Valida &&
                        (m.Tipo == TipoMarca.Entrada || m.Tipo == TipoMarca.Salida))
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return ultima is { Tipo: TipoMarca.Entrada };
    }

    public async Task RegistrarAsync(Marca marca, CancellationToken cancellationToken = default)
    {
        db.Marcas.Add(marca);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Marca>> ConsultarAsync(int empleadoId, DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        var desdeUtc = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var hastaUtc = hasta.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await db.Marcas
            .Where(m => m.EmpleadoId == empleadoId && m.Timestamp >= desdeUtc && m.Timestamp <= hastaUtc)
            .OrderByDescending(m => m.Timestamp)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Marca>> ConsultarTodasAsync(int? empleadoId, int? centroTrabajoId, DateOnly? fecha, int limite, CancellationToken cancellationToken = default)
    {
        var query = db.Marcas
            .Include(m => m.Empleado)
            .ThenInclude(e => e!.CentroTrabajo)
            .AsNoTracking()
            .AsQueryable();

        if (empleadoId is not null)
        {
            query = query.Where(m => m.EmpleadoId == empleadoId);
        }
        if (centroTrabajoId is not null)
        {
            query = query.Where(m => m.Empleado!.CentroTrabajoId == centroTrabajoId);
        }
        if (fecha is not null)
        {
            var inicio = fecha.Value.ToDateTime(TimeOnly.MinValue);
            var fin = inicio.AddDays(1);
            query = query.Where(m => m.Timestamp >= inicio && m.Timestamp < fin);
        }

        return await query.OrderByDescending(m => m.Timestamp).Take(limite).ToListAsync(cancellationToken);
    }
}
