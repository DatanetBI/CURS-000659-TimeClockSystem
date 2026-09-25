using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Turnos;

public record TurnoDto(int Id, string Nombre, TipoTurno Tipo, TimeSpan HoraEntrada, TimeSpan HoraSalida, int DuracionRecesoMinutos, int ToleranciaMinutos, int DuracionTotalMinutos);

public record GuardarTurnoRequest(string Nombre, TipoTurno Tipo, TimeSpan HoraEntrada, TimeSpan HoraSalida, int DuracionRecesoMinutos, int ToleranciaMinutos);

public enum EliminarTurnoResultado { Eliminado, NoEncontrado, TieneAsignacionesVigentes }

/// <summary>Casos de uso de Turnos (contracts/turnos-y-asignaciones.md).</summary>
public class TurnosService(ITurnoRepository turnos)
{
    public async Task<IReadOnlyList<TurnoDto>> ListarAsync(CancellationToken cancellationToken = default) =>
        (await turnos.ListarAsync(cancellationToken)).Select(AParaDto).ToList();

    public async Task<TurnoDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default)
    {
        var turno = await turnos.ObtenerPorIdAsync(id, cancellationToken);
        return turno is null ? null : AParaDto(turno);
    }

    public async Task<TurnoDto> CrearAsync(GuardarTurnoRequest request, CancellationToken cancellationToken = default)
    {
        var turno = new Turno
        {
            Nombre = request.Nombre,
            Tipo = request.Tipo,
            HoraEntrada = request.HoraEntrada,
            HoraSalida = request.HoraSalida,
            DuracionRecesoMinutos = request.DuracionRecesoMinutos,
            ToleranciaMinutos = request.ToleranciaMinutos,
        };
        await turnos.AgregarAsync(turno, cancellationToken);
        return AParaDto(turno);
    }

    public async Task<TurnoDto?> ActualizarAsync(int id, GuardarTurnoRequest request, CancellationToken cancellationToken = default)
    {
        var turno = await turnos.ObtenerPorIdAsync(id, cancellationToken);
        if (turno is null)
        {
            return null;
        }

        turno.Nombre = request.Nombre;
        turno.Tipo = request.Tipo;
        turno.HoraEntrada = request.HoraEntrada;
        turno.HoraSalida = request.HoraSalida;
        turno.DuracionRecesoMinutos = request.DuracionRecesoMinutos;
        turno.ToleranciaMinutos = request.ToleranciaMinutos;
        await turnos.ActualizarAsync(turno, cancellationToken);
        return AParaDto(turno);
    }

    public async Task<EliminarTurnoResultado> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await turnos.ObtenerPorIdAsync(id, cancellationToken) is null)
        {
            return EliminarTurnoResultado.NoEncontrado;
        }

        if (await turnos.TieneAsignacionesVigentesAsync(id, cancellationToken))
        {
            return EliminarTurnoResultado.TieneAsignacionesVigentes;
        }

        await turnos.EliminarAsync(id, cancellationToken);
        return EliminarTurnoResultado.Eliminado;
    }

    private static TurnoDto AParaDto(Turno turno) => new(
        turno.Id, turno.Nombre, turno.Tipo, turno.HoraEntrada, turno.HoraSalida,
        turno.DuracionRecesoMinutos, turno.ToleranciaMinutos, turno.DuracionTotalMinutos());
}
