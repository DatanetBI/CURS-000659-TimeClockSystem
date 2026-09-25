using System.Net.Http.Json;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

/// <summary>Cliente HTTP de Centros de Trabajo (contracts/centros-trabajo.md).</summary>
public class CentrosTrabajoApiClient(HttpClient http)
{
    public async Task<List<CentroTrabajo>> ListarAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<CentroTrabajo>>("api/centros-trabajo", ApiJsonOptions.Default, cancellationToken) ?? [];

    public async Task<CentroTrabajo?> ObtenerAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/centros-trabajo/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CentroTrabajo>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<bool> CrearAsync(CentroTrabajo centro, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/centros-trabajo", centro, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarAsync(int id, CentroTrabajo centro, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/centros-trabajo/{id}", centro, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
