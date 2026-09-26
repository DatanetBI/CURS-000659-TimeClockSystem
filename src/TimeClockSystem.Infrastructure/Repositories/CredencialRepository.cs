using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class CredencialRepository(ApplicationDbContext db) : ICredencialRepository
{
    public Task<CredencialDeMarcaje?> ObtenerPorEmpleadoIdAsync(int empleadoId, CancellationToken cancellationToken = default) =>
        db.CredencialesDeMarcaje.FirstOrDefaultAsync(c => c.EmpleadoId == empleadoId, cancellationToken);

    public async Task GuardarAsync(int empleadoId, string pinHash, string? actualizadoPorUserId, CancellationToken cancellationToken = default)
    {
        var credencial = await db.CredencialesDeMarcaje.FirstOrDefaultAsync(c => c.EmpleadoId == empleadoId, cancellationToken);
        if (credencial is null)
        {
            db.CredencialesDeMarcaje.Add(new CredencialDeMarcaje
            {
                EmpleadoId = empleadoId,
                PinHash = pinHash,
                ActualizadoPorUserId = actualizadoPorUserId,
                FechaActualizacion = DateTime.UtcNow,
            });
        }
        else
        {
            credencial.PinHash = pinHash;
            credencial.ActualizadoPorUserId = actualizadoPorUserId;
            credencial.FechaActualizacion = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
