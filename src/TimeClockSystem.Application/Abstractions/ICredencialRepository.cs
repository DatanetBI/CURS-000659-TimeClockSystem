using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public interface ICredencialRepository
{
    Task<CredencialDeMarcaje?> ObtenerPorEmpleadoIdAsync(int empleadoId, CancellationToken cancellationToken = default);

    Task GuardarAsync(int empleadoId, string pinHash, string? actualizadoPorUserId, CancellationToken cancellationToken = default);
}
