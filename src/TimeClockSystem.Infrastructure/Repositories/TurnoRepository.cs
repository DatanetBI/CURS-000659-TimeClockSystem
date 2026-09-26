using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class TurnoRepository(ApplicationDbContext db) : ITurnoRepository
{
    public async Task<IReadOnlyList<Turno>> ListarAsync(CancellationToken cancellationToken = default) =>
        await db.Turnos.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Turnos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AgregarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        db.Turnos.Add(turno);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default) =>
        await db.SaveChangesAsync(cancellationToken);

    public Task<bool> TieneAsignacionesVigentesAsync(int turnoId, CancellationToken cancellationToken = default) =>
        db.AsignacionesTurno.AnyAsync(a => a.TurnoId == turnoId && a.Fecha >= DateOnly.FromDateTime(DateTime.UtcNow.Date), cancellationToken);

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var turno = await db.Turnos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (turno is not null)
        {
            db.Turnos.Remove(turno);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
