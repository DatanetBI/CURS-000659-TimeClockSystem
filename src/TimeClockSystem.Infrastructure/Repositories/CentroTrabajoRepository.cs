using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class CentroTrabajoRepository(ApplicationDbContext db) : ICentroTrabajoRepository
{
    public async Task<IReadOnlyList<CentroTrabajo>> ListarAsync(CancellationToken cancellationToken = default) =>
        await db.CentrosTrabajo.AsNoTracking().ToListAsync(cancellationToken);

    public Task<CentroTrabajo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.CentrosTrabajo.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AgregarAsync(CentroTrabajo centro, CancellationToken cancellationToken = default)
    {
        db.CentrosTrabajo.Add(centro);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(CentroTrabajo centro, CancellationToken cancellationToken = default) =>
        await db.SaveChangesAsync(cancellationToken);

    public Task<bool> TieneEmpleadosAsignadosAsync(int centroTrabajoId, CancellationToken cancellationToken = default) =>
        db.Empleados.AnyAsync(e => e.CentroTrabajoId == centroTrabajoId, cancellationToken);

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var centro = await db.CentrosTrabajo.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (centro is not null)
        {
            db.CentrosTrabajo.Remove(centro);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
