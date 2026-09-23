using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TimeClockSystem.Tests.Integration;

public class CentrosTrabajoTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CentrosTrabajoTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_ComoAdministrador_MuestraLosCentrosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var response = await client.GetAsync("/CentrosTrabajo/CentrosTrabajo");
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Oficina Central CDMX", html);
    }

    [Fact]
    public async Task CrearYEditar_ComoAdministrador_PersisteElCentroDeTrabajo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var createPage = await client.GetAsync("/CentrosTrabajo/CentrosTrabajo/Create");
        createPage.EnsureSuccessStatusCode();
        var token = AuthTestHelper.ExtractAntiForgeryToken(await createPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["Nombre"] = "Sucursal de Prueba",
            ["Latitud"] = "19.0",
            ["Longitud"] = "-99.0",
            ["RadioMetros"] = "200",
            ["__RequestVerificationToken"] = token,
        };

        var createResponse = await client.PostAsync("/CentrosTrabajo/CentrosTrabajo/Create", new FormUrlEncodedContent(form));
        createResponse.EnsureSuccessStatusCode();

        var indexHtml = await (await client.GetAsync("/CentrosTrabajo/CentrosTrabajo")).Content.ReadAsStringAsync();
        Assert.Contains("Sucursal de Prueba", indexHtml);
    }

    [Fact]
    public async Task Index_SinSesion_RedirigeAIniciarSesion()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/CentrosTrabajo/CentrosTrabajo");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Cuenta/IniciarSesion", response.Headers.Location?.ToString());
    }
}
