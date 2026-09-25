using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public interface IEmpleadoRepository
{
    Task<IReadOnlyList<Empleado>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Empleado?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Empleado?> ObtenerPorNumeroEmpleadoAsync(string numeroEmpleado, CancellationToken cancellationToken = default);

    Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default);

    Task ActualizarAsync(Empleado empleado, CancellationToken cancellationToken = default);
}
