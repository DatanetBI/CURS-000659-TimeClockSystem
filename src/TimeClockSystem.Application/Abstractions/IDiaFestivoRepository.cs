using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public interface IDiaFestivoRepository
{
    Task<IReadOnlyList<DiaFestivo>> ListarAsync(CancellationToken cancellationToken = default);

    Task<DiaFestivo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task AgregarAsync(DiaFestivo diaFestivo, CancellationToken cancellationToken = default);

    Task ActualizarAsync(DiaFestivo diaFestivo, CancellationToken cancellationToken = default);

    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
