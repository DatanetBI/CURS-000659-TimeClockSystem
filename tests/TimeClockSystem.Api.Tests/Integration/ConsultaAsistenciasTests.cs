using System.Net;
using System.Net.Http.Json;

namespace TimeClockSystem.Api.Tests.Integration;

public class ConsultaAsistenciasTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public ConsultaAsistenciasTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    private record FilaAdminDto(string NumeroEmpleado, string CentroTrabajo);

    [Fact]
    public async Task ConsultarComoAdministrador_SinFiltros_MuestraLasMarcasDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var filas = await client.GetFromJsonAsync<List<FilaAdminDto>>("api/consulta-asistencias/admin");

        Assert.Contains(filas!, f => f.NumeroEmpleado == "E001" && f.CentroTrabajo == "Oficina Central CDMX");
    }

    [Fact]
    public async Task ConsultarComoAdministrador_FiltradoPorEmpleado_SoloDevuelveSusMarcas()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        // E001 tiene Id=1 en una base recién sembrada.
        var filas = await client.GetFromJsonAsync<List<FilaAdminDto>>("api/consulta-asistencias/admin?empleadoId=1");

        Assert.All(filas!, f => Assert.Equal("E001", f.NumeroEmpleado));
    }

    [Fact]
    public async Task ConsultarComoAdministrador_SinToken_DevuelveNoAutorizado()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/consulta-asistencias/admin");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
