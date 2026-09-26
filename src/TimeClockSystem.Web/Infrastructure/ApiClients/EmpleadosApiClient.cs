using System.Net.Http.Json;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

internal record EmpleadoApiDto(int Id, string NumeroEmpleado, string Nombre, int CentroTrabajoId, string? CentroTrabajoNombre, EstadoEmpleado Estado, bool ConsentimientoGeolocalizacion);
internal record CredencialApiDto(bool TieneCredencial, DateTime? FechaActualizacion);

/// <summary>Cliente HTTP de Empleados (contracts/empleados.md).</summary>
public class EmpleadosApiClient(HttpClient http)
{
    public async Task<List<Empleado>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var dtos = await http.GetFromJsonAsync<List<EmpleadoApiDto>>("api/empleados", ApiJsonOptions.Default, cancellationToken) ?? [];
        return dtos.Select(AParaViewModel).ToList();
    }

    public async Task<Empleado?> ObtenerAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/empleados/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var dto = await response.Content.ReadFromJsonAsync<EmpleadoApiDto>(ApiJsonOptions.Default, cancellationToken);
        return dto is null ? null : AParaViewModel(dto);
    }

    public async Task<bool> CrearAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/empleados", new
        {
            numeroEmpleado = empleado.NumeroEmpleado,
            nombre = empleado.Nombre,
            centroTrabajoId = empleado.CentroTrabajoId,
            consentimientoGeolocalizacion = empleado.ConsentimientoGeolocalizacion,
        }, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarAsync(int id, Empleado empleado, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/empleados/{id}", new
        {
            numeroEmpleado = empleado.NumeroEmpleado,
            nombre = empleado.Nombre,
            centroTrabajoId = empleado.CentroTrabajoId,
            estado = empleado.Estado,
            consentimientoGeolocalizacion = empleado.ConsentimientoGeolocalizacion,
        }, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarCredencialAsync(int empleadoId, string pin, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/empleados/{empleadoId}/credencial", new { pin }, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static Empleado AParaViewModel(EmpleadoApiDto dto) => new()
    {
        Id = dto.Id,
        NumeroEmpleado = dto.NumeroEmpleado,
        Nombre = dto.Nombre,
        CentroTrabajoId = dto.CentroTrabajoId,
        CentroTrabajo = dto.CentroTrabajoNombre is null ? null : new CentroTrabajo { Id = dto.CentroTrabajoId, Nombre = dto.CentroTrabajoNombre },
        Estado = dto.Estado,
        ConsentimientoGeolocalizacion = dto.ConsentimientoGeolocalizacion,
    };
}
