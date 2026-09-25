using System.Net.Http.Json;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

/// <summary>Cliente HTTP de Turnos (contracts/turnos-y-asignaciones.md).</summary>
public class TurnosApiClient(HttpClient http)
{
    public async Task<List<Turno>> ListarAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<Turno>>("api/turnos", ApiJsonOptions.Default, cancellationToken) ?? [];

    public async Task<Turno?> ObtenerAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/turnos/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<Turno>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<bool> CrearAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/turnos", turno, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarAsync(int id, Turno turno, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/turnos/{id}", turno, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
