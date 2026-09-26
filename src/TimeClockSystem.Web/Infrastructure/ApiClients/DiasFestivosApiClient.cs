using System.Net.Http.Json;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

/// <summary>Cliente HTTP de Días Festivos (contracts/dias-festivos.md).</summary>
public class DiasFestivosApiClient(HttpClient http)
{
    public async Task<List<DiaFestivo>> ListarAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<DiaFestivo>>("api/dias-festivos", ApiJsonOptions.Default, cancellationToken) ?? [];

    public async Task<bool> CrearAsync(DiaFestivo festivo, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/dias-festivos", festivo, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
