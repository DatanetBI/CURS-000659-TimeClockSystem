using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public interface ICentroTrabajoRepository
{
    Task<IReadOnlyList<CentroTrabajo>> ListarAsync(CancellationToken cancellationToken = default);

    Task<CentroTrabajo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task AgregarAsync(CentroTrabajo centro, CancellationToken cancellationToken = default);

    Task ActualizarAsync(CentroTrabajo centro, CancellationToken cancellationToken = default);

    Task<bool> TieneEmpleadosAsignadosAsync(int centroTrabajoId, CancellationToken cancellationToken = default);

    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
