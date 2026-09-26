using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Repositories;

public class EmpleadoRepository(ApplicationDbContext db) : IEmpleadoRepository
{
    public async Task<IReadOnlyList<Empleado>> ListarAsync(CancellationToken cancellationToken = default) =>
        await db.Empleados.Include(e => e.CentroTrabajo).AsNoTracking().ToListAsync(cancellationToken);

    public Task<Empleado?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Empleados.Include(e => e.CentroTrabajo).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Empleado?> ObtenerPorNumeroEmpleadoAsync(string numeroEmpleado, CancellationToken cancellationToken = default) =>
        db.Empleados.Include(e => e.CentroTrabajo).FirstOrDefaultAsync(e => e.NumeroEmpleado == numeroEmpleado, cancellationToken);

    public async Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Empleado empleado, CancellationToken cancellationToken = default) =>
        await db.SaveChangesAsync(cancellationToken);
}
