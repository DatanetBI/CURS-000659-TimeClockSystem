using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Turnos;

public record AsignacionTurnoDto(int Id, int EmpleadoId, string? EmpleadoNombre, int TurnoId, string? TurnoNombre, DateOnly Fecha);

public record CrearAsignacionTurnoRequest(int EmpleadoId, int TurnoId, DateOnly Fecha);

/// <summary>Casos de uso de Asignaciones de Turno (contracts/turnos-y-asignaciones.md).</summary>
public class AsignacionesTurnoService(IAsignacionTurnoRepository asignaciones, IEmpleadoRepository empleados, ITurnoRepository turnos)
{
    public async Task<IReadOnlyList<AsignacionTurnoDto>> ListarAsync(int? empleadoId, DateOnly? fecha, CancellationToken cancellationToken = default) =>
        (await asignaciones.ListarAsync(empleadoId, fecha, cancellationToken)).Select(AParaDto).ToList();

    public async Task<AsignacionTurnoDto?> CrearAsync(CrearAsignacionTurnoRequest request, CancellationToken cancellationToken = default)
    {
        if (await empleados.ObtenerPorIdAsync(request.EmpleadoId, cancellationToken) is null ||
            await turnos.ObtenerPorIdAsync(request.TurnoId, cancellationToken) is null)
        {
            return null;
        }

        var asignacion = new AsignacionTurno
        {
            EmpleadoId = request.EmpleadoId,
            TurnoId = request.TurnoId,
            Fecha = request.Fecha,
        };
        await asignaciones.AgregarAsync(asignacion, cancellationToken);
        return AParaDto(asignacion);
    }

    public Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default) =>
        asignaciones.EliminarAsync(id, cancellationToken);

    private static AsignacionTurnoDto AParaDto(AsignacionTurno asignacion) => new(
        asignacion.Id, asignacion.EmpleadoId, asignacion.Empleado?.Nombre,
        asignacion.TurnoId, asignacion.Turno?.Nombre, asignacion.Fecha);
}
