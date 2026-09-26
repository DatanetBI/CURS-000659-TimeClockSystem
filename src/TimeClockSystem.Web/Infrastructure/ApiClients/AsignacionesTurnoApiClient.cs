using System.Net.Http.Json;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

public record AsignacionTurnoApiDto(int Id, int EmpleadoId, string? EmpleadoNombre, int TurnoId, string? TurnoNombre, DateOnly Fecha);

/// <summary>Cliente HTTP de Asignaciones de Turno (contracts/turnos-y-asignaciones.md).</summary>
public class AsignacionesTurnoApiClient(HttpClient http)
{
    public async Task<bool> CrearAsync(int empleadoId, int turnoId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/asignaciones-turno", new { empleadoId, turnoId, fecha }, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
