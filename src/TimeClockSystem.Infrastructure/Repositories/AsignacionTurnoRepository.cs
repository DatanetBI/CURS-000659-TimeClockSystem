using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class AsignacionTurnoRepository(ApplicationDbContext db) : IAsignacionTurnoRepository
{
    public async Task<IReadOnlyList<AsignacionTurno>> ListarAsync(int? empleadoId, DateOnly? fecha, CancellationToken cancellationToken = default)
    {
        var query = db.AsignacionesTurno
            .Include(a => a.Empleado)
            .Include(a => a.Turno)
            .AsNoTracking()
            .AsQueryable();

        if (empleadoId is not null)
        {
            query = query.Where(a => a.EmpleadoId == empleadoId);
        }
        if (fecha is not null)
        {
            query = query.Where(a => a.Fecha == fecha);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<AsignacionTurno?> ObtenerParaEmpleadoEnFechaAsync(int empleadoId, DateOnly fecha, CancellationToken cancellationToken = default) =>
        db.AsignacionesTurno
            .Include(a => a.Turno)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmpleadoId == empleadoId && a.Fecha == fecha, cancellationToken);

    public async Task AgregarAsync(AsignacionTurno asignacion, CancellationToken cancellationToken = default)
    {
        db.AsignacionesTurno.Add(asignacion);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var asignacion = await db.AsignacionesTurno.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (asignacion is null)
        {
            return false;
        }

        db.AsignacionesTurno.Remove(asignacion);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
