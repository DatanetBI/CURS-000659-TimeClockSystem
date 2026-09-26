using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TimeClockSystem.Web.Tests.Integration;

/// <summary>
/// Pruebas de humo del Frontend que no requieren un Backend en ejecución: verifican la página de
/// inicio pública y que la cookie de autenticación protege las Areas administrativas (FR-004).
/// Las reglas de negocio (marcaje, CRUD) ahora se prueban en TimeClockSystem.Api.Tests, contra el
/// Backend real (research.md #8).
/// </summary>
public class NavegacionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public NavegacionTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Home_SinSesion_RespondeCorrectamente()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("/Empleados/Empleados")]
    [InlineData("/CentrosTrabajo/CentrosTrabajo")]
    [InlineData("/Turnos/Turnos")]
    [InlineData("/DiasFestivos/DiasFestivos")]
    [InlineData("/ConsultaAsistencias/ConsultaAsistencias")]
    [InlineData("/Auditoria/Auditoria")]
    [InlineData("/Marcaje/Marcaje")]
    public async Task AreaProtegida_SinSesion_RedirigeAIniciarSesion(string ruta)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Cuenta/IniciarSesion", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task MarcajePorPin_SinSesion_EsAccesiblePublicamente()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Marcaje/PinMarcaje");

        response.EnsureSuccessStatusCode();
    }
}
