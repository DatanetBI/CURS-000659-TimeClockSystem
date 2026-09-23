using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;

namespace TimeClockSystem.Tests.Integration;

public class MarcajeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MarcajeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private const string NumeroEmpleadoE001 = "E001";
    private const string PasswordPortalEmpleado = "Empleado123!";
    private const string PinE001 = "1234";

    [Fact]
    public async Task MarcarEntradaYSalida_DentroDelGeofence_SeRegistranComoValidas()
    {
        var client = await AuthTestHelper.LoginAsync(_factory, NumeroEmpleadoE001, PasswordPortalEmpleado);

        var indexPage = await client.GetAsync("/Marcaje/Marcaje");
        indexPage.EnsureSuccessStatusCode();
        var token = AuthTestHelper.ExtractAntiForgeryToken(await indexPage.Content.ReadAsStringAsync());

        // Oficina Central CDMX: 19.4326, -99.1332 — coordenada dentro del geofence.
        var entrada = new Dictionary<string, string>
        {
            ["Tipo"] = "Entrada",
            ["Latitud"] = "19.4326",
            ["Longitud"] = "-99.1332",
            ["__RequestVerificationToken"] = token,
        };
        var entradaResponse = await client.PostAsync("/Marcaje/Marcaje/Registrar", new FormUrlEncodedContent(entrada));
        var htmlTrasEntrada = await entradaResponse.Content.ReadAsStringAsync();
        Assert.Contains("Marca registrada", htmlTrasEntrada);

        var token2 = AuthTestHelper.ExtractAntiForgeryToken(htmlTrasEntrada);
        var salida = new Dictionary<string, string>
        {
            ["Tipo"] = "Salida",
            ["Latitud"] = "19.4326",
            ["Longitud"] = "-99.1332",
            ["__RequestVerificationToken"] = token2,
        };
        var salidaResponse = await client.PostAsync("/Marcaje/Marcaje/Registrar", new FormUrlEncodedContent(salida));
        Assert.Contains("Marca registrada", await salidaResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task MarcarEntradaDosVeces_SinSalida_LaSegundaSeRechaza()
    {
        var client = await AuthTestHelper.LoginAsync(_factory, "E002", PasswordPortalEmpleado);

        var indexPage = await client.GetAsync("/Marcaje/Marcaje");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await indexPage.Content.ReadAsStringAsync());
        var entrada = new Dictionary<string, string>
        {
            ["Tipo"] = "Entrada",
            ["__RequestVerificationToken"] = token,
        };
        var primeraRespuesta = await client.PostAsync("/Marcaje/Marcaje/Registrar", new FormUrlEncodedContent(entrada));
        var html1 = await primeraRespuesta.Content.ReadAsStringAsync();

        var token2 = AuthTestHelper.ExtractAntiForgeryToken(html1);
        var segundaEntrada = new Dictionary<string, string>
        {
            ["Tipo"] = "Entrada",
            ["__RequestVerificationToken"] = token2,
        };
        var segundaRespuesta = await client.PostAsync("/Marcaje/Marcaje/Registrar", new FormUrlEncodedContent(segundaEntrada));
        var html2 = await segundaRespuesta.Content.ReadAsStringAsync();

        Assert.Contains("Ya tienes una entrada abierta", html2);
    }

    [Fact]
    public async Task MarcarFueraDelGeofence_SeRechaza()
    {
        var client = await AuthTestHelper.LoginAsync(_factory, "E003", PasswordPortalEmpleado);

        var indexPage = await client.GetAsync("/Marcaje/Marcaje");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await indexPage.Content.ReadAsStringAsync());

        // Coordenada a varios kilómetros de Oficina Central CDMX.
        var form = new Dictionary<string, string>
        {
            ["Tipo"] = "Entrada",
            ["Latitud"] = "19.5000",
            ["Longitud"] = "-99.3000",
            ["__RequestVerificationToken"] = token,
        };
        var response = await client.PostAsync("/Marcaje/Marcaje/Registrar", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("no estás dentro del perímetro autorizado", html);
    }

    [Fact]
    public async Task MarcarPorPin_ConCredencialesIncorrectas_MuestraMensajeGenerico()
    {
        var client = _factory.CreateClient();

        var page = await client.GetAsync("/Marcaje/PinMarcaje");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await page.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["NumeroEmpleado"] = NumeroEmpleadoE001,
            ["Pin"] = "0000", // PIN incorrecto (el real es 1234)
            ["Tipo"] = "Entrada",
            ["__RequestVerificationToken"] = token,
        };
        var response = await client.PostAsync("/Marcaje/PinMarcaje", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Credenciales inválidas", html);
    }

    [Fact]
    public async Task MarcarPorPin_ConCredencialesCorrectas_RegistraLaMarca()
    {
        var client = _factory.CreateClient();

        var page = await client.GetAsync("/Marcaje/PinMarcaje");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await page.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["NumeroEmpleado"] = "E004",
            ["Pin"] = PinE001, // todos los empleados de ejemplo comparten el mismo PIN mock
            ["Tipo"] = "Entrada",
            ["__RequestVerificationToken"] = token,
        };
        var response = await client.PostAsync("/Marcaje/PinMarcaje", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Marca registrada", html);
    }

    [Fact]
    public async Task MarcarConGeolocalizacion_SinConsentimiento_SeRechaza()
    {
        // E005 se usa exclusivamente en esta prueba para revocarle el consentimiento sin afectar
        // a las demás pruebas que comparten la misma fixture/DB.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var empleado = await db.Empleados.FirstAsync(e => e.NumeroEmpleado == "E005");
            empleado.ConsentimientoGeolocalizacion = false;
            await db.SaveChangesAsync();
        }

        var client = await AuthTestHelper.LoginAsync(_factory, "E005", PasswordPortalEmpleado);
        var indexPage = await client.GetAsync("/Marcaje/Marcaje");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await indexPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["Tipo"] = "Entrada",
            ["Latitud"] = "20.5888",
            ["Longitud"] = "-100.3899",
            ["__RequestVerificationToken"] = token,
        };
        var response = await client.PostAsync("/Marcaje/Marcaje/Registrar", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("falta tu consentimiento", html);
    }
}
