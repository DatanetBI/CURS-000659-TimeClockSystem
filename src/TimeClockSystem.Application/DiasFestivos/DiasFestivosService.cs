using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.DiasFestivos;

public record DiaFestivoDto(int Id, DateOnly Fecha, string Descripcion);

public record GuardarDiaFestivoRequest(DateOnly Fecha, string Descripcion);

/// <summary>Casos de uso de Días Festivos (contracts/dias-festivos.md).</summary>
public class DiasFestivosService(IDiaFestivoRepository diasFestivos)
{
    public async Task<IReadOnlyList<DiaFestivoDto>> ListarAsync(CancellationToken cancellationToken = default) =>
        (await diasFestivos.ListarAsync(cancellationToken)).Select(AParaDto).ToList();

    public async Task<DiaFestivoDto> CrearAsync(GuardarDiaFestivoRequest request, CancellationToken cancellationToken = default)
    {
        var diaFestivo = new DiaFestivo { Fecha = request.Fecha, Descripcion = request.Descripcion };
        await diasFestivos.AgregarAsync(diaFestivo, cancellationToken);
        return AParaDto(diaFestivo);
    }

    public async Task<DiaFestivoDto?> ActualizarAsync(int id, GuardarDiaFestivoRequest request, CancellationToken cancellationToken = default)
    {
        var diaFestivo = await diasFestivos.ObtenerPorIdAsync(id, cancellationToken);
        if (diaFestivo is null)
        {
            return null;
        }

        diaFestivo.Fecha = request.Fecha;
        diaFestivo.Descripcion = request.Descripcion;
        await diasFestivos.ActualizarAsync(diaFestivo, cancellationToken);
        return AParaDto(diaFestivo);
    }

    public Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default) =>
        diasFestivos.EliminarAsync(id, cancellationToken);

    private static DiaFestivoDto AParaDto(DiaFestivo diaFestivo) => new(diaFestivo.Id, diaFestivo.Fecha, diaFestivo.Descripcion);
}
