using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class DiaFestivoRepository(ApplicationDbContext db) : IDiaFestivoRepository
{
    public async Task<IReadOnlyList<DiaFestivo>> ListarAsync(CancellationToken cancellationToken = default) =>
        await db.DiasFestivos.AsNoTracking().OrderBy(d => d.Fecha).ToListAsync(cancellationToken);

    public Task<DiaFestivo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.DiasFestivos.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task AgregarAsync(DiaFestivo diaFestivo, CancellationToken cancellationToken = default)
    {
        db.DiasFestivos.Add(diaFestivo);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(DiaFestivo diaFestivo, CancellationToken cancellationToken = default) =>
        await db.SaveChangesAsync(cancellationToken);

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var diaFestivo = await db.DiasFestivos.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (diaFestivo is null)
        {
            return false;
        }

        db.DiasFestivos.Remove(diaFestivo);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
