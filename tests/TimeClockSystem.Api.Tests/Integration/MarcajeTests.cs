using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Api.Tests.Integration;

public class MarcajeTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public MarcajeTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    private const string PasswordPortalEmpleado = "Empleado123!";
    private const string PinDeEjemplo = "1234";

    private record MarcajeResponse(bool Aceptada, string? Motivo, int? MarcaId, DateTime? Timestamp, string? EmpleadoNombre);

    [Fact]
    public async Task MarcarEntradaYSalida_DentroDelGeofence_SeRegistranComoValidas()
    {
        var client = await AuthTestHelper.LoginAsync(factory, "E001", PasswordPortalEmpleado);

        // Oficina Central CDMX: 19.4326, -99.1332 — coordenada dentro del geofence.
        var entrada = await client.PostAsJsonAsync("api/marcaje", new { tipo = "Entrada", latitud = 19.4326, longitud = -99.1332 });
        entrada.EnsureSuccessStatusCode();
        var resultadoEntrada = await entrada.Content.ReadFromJsonAsync<MarcajeResponse>();
        Assert.True(resultadoEntrada!.Aceptada);

        var salida = await client.PostAsJsonAsync("api/marcaje", new { tipo = "Salida", latitud = 19.4326, longitud = -99.1332 });
        var resultadoSalida = await salida.Content.ReadFromJsonAsync<MarcajeResponse>();
        Assert.True(resultadoSalida!.Aceptada);
    }

    [Fact]
    public async Task MarcarEntradaDosVeces_SinSalida_LaSegundaSeRechaza()
    {
        var client = await AuthTestHelper.LoginAsync(factory, "E002", PasswordPortalEmpleado);

        var primera = await client.PostAsJsonAsync("api/marcaje", new { tipo = "Entrada" });
        Assert.True((await primera.Content.ReadFromJsonAsync<MarcajeResponse>())!.Aceptada);

        var segunda = await client.PostAsJsonAsync("api/marcaje", new { tipo = "Entrada" });
        var resultado = await segunda.Content.ReadFromJsonAsync<MarcajeResponse>();

        Assert.False(resultado!.Aceptada);
        Assert.Equal("EntradaDuplicada", resultado.Motivo);
    }

    [Fact]
    public async Task MarcarFueraDelGeofence_SeRechaza()
    {
        var client = await AuthTestHelper.LoginAsync(factory, "E003", PasswordPortalEmpleado);

        // Coordenada a varios kilómetros de Oficina Central CDMX.
        var response = await client.PostAsJsonAsync("api/marcaje", new { tipo = "Entrada", latitud = 19.5000, longitud = -99.3000 });
        var resultado = await response.Content.ReadFromJsonAsync<MarcajeResponse>();

        Assert.False(resultado!.Aceptada);
        Assert.Equal("Geofence", resultado.Motivo);
    }

    [Fact]
    public async Task MarcarPorPin_ConCredencialesIncorrectas_SeRechazaConMotivoGenerico()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/marcaje/pin", new { numeroEmpleado = "E001", pin = "0000", tipo = "Entrada" });
        var resultado = await response.Content.ReadFromJsonAsync<MarcajeResponse>();

        Assert.False(resultado!.Aceptada);
        Assert.Equal("CredencialesInvalidas", resultado.Motivo);
    }

    [Fact]
    public async Task MarcarPorPin_ConCredencialesCorrectas_RegistraLaMarca()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/marcaje/pin", new { numeroEmpleado = "E004", pin = PinDeEjemplo, tipo = "Entrada" });
        var resultado = await response.Content.ReadFromJsonAsync<MarcajeResponse>();

        Assert.True(resultado!.Aceptada);
    }

    [Fact]
    public async Task MarcarConGeolocalizacion_SinConsentimiento_SeRechaza()
    {
        // E005 se usa exclusivamente en esta prueba para revocarle el consentimiento sin afectar
        // a las demás pruebas que comparten la misma fixture/DB.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var empleado = await db.Empleados.FirstAsync(e => e.NumeroEmpleado == "E005");
            empleado.ConsentimientoGeolocalizacion = false;
            await db.SaveChangesAsync();
        }

        var client = await AuthTestHelper.LoginAsync(factory, "E005", PasswordPortalEmpleado);

        var response = await client.PostAsJsonAsync("api/marcaje", new { tipo = "Entrada", latitud = 20.5888, longitud = -100.3899 });
        var resultado = await response.Content.ReadFromJsonAsync<MarcajeResponse>();

        Assert.False(resultado!.Aceptada);
        Assert.Equal("SinConsentimientoGeolocalizacion", resultado.Motivo);
    }
}
