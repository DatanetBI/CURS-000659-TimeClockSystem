using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Abstractions;

public interface IMarcaRepository
{
    Task<bool> TieneEntradaAbiertaAsync(int empleadoId, CancellationToken cancellationToken = default);

    Task RegistrarAsync(Marca marca, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Marca>> ConsultarAsync(int empleadoId, DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vista de Administrador: navega las marcas de todos los empleados con filtros opcionales
    /// (contracts/consulta-asistencias.md - GET /api/consulta-asistencias/admin).
    /// </summary>
    Task<IReadOnlyList<Marca>> ConsultarTodasAsync(int? empleadoId, int? centroTrabajoId, DateOnly? fecha, int limite, CancellationToken cancellationToken = default);
}
