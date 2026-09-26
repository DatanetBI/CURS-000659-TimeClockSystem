using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.CentrosTrabajo;

public record CentroTrabajoDto(int Id, string Nombre, double Latitud, double Longitud, int RadioMetros);

public record GuardarCentroTrabajoRequest(string Nombre, double Latitud, double Longitud, int RadioMetros);

public enum EliminarCentroTrabajoResultado { Eliminado, NoEncontrado, TieneEmpleadosAsignados }

/// <summary>Casos de uso de Centros de Trabajo (contracts/centros-trabajo.md).</summary>
public class CentrosTrabajoService(ICentroTrabajoRepository centros)
{
    public async Task<IReadOnlyList<CentroTrabajoDto>> ListarAsync(CancellationToken cancellationToken = default) =>
        (await centros.ListarAsync(cancellationToken)).Select(AParaDto).ToList();

    public async Task<CentroTrabajoDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default)
    {
        var centro = await centros.ObtenerPorIdAsync(id, cancellationToken);
        return centro is null ? null : AParaDto(centro);
    }

    public async Task<CentroTrabajoDto> CrearAsync(GuardarCentroTrabajoRequest request, CancellationToken cancellationToken = default)
    {
        var centro = new CentroTrabajo
        {
            Nombre = request.Nombre,
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            RadioMetros = request.RadioMetros,
        };
        await centros.AgregarAsync(centro, cancellationToken);
        return AParaDto(centro);
    }

    public async Task<CentroTrabajoDto?> ActualizarAsync(int id, GuardarCentroTrabajoRequest request, CancellationToken cancellationToken = default)
    {
        var centro = await centros.ObtenerPorIdAsync(id, cancellationToken);
        if (centro is null)
        {
            return null;
        }

        centro.Nombre = request.Nombre;
        centro.Latitud = request.Latitud;
        centro.Longitud = request.Longitud;
        centro.RadioMetros = request.RadioMetros;
        await centros.ActualizarAsync(centro, cancellationToken);
        return AParaDto(centro);
    }

    public async Task<EliminarCentroTrabajoResultado> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await centros.ObtenerPorIdAsync(id, cancellationToken) is null)
        {
            return EliminarCentroTrabajoResultado.NoEncontrado;
        }

        if (await centros.TieneEmpleadosAsignadosAsync(id, cancellationToken))
        {
            return EliminarCentroTrabajoResultado.TieneEmpleadosAsignados;
        }

        await centros.EliminarAsync(id, cancellationToken);
        return EliminarCentroTrabajoResultado.Eliminado;
    }

    private static CentroTrabajoDto AParaDto(CentroTrabajo centro) =>
        new(centro.Id, centro.Nombre, centro.Latitud, centro.Longitud, centro.RadioMetros);
}
