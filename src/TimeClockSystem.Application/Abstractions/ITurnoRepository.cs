using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public interface ITurnoRepository
{
    Task<IReadOnlyList<Turno>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task AgregarAsync(Turno turno, CancellationToken cancellationToken = default);

    Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default);

    Task<bool> TieneAsignacionesVigentesAsync(int turnoId, CancellationToken cancellationToken = default);

    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAsignacionTurnoRepository
{
    Task<IReadOnlyList<AsignacionTurno>> ListarAsync(int? empleadoId, DateOnly? fecha, CancellationToken cancellationToken = default);

    Task<AsignacionTurno?> ObtenerParaEmpleadoEnFechaAsync(int empleadoId, DateOnly fecha, CancellationToken cancellationToken = default);

    Task AgregarAsync(AsignacionTurno asignacion, CancellationToken cancellationToken = default);

    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
