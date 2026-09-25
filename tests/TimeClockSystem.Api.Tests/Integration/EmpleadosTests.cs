using System.Net;
using System.Net.Http.Json;

namespace TimeClockSystem.Api.Tests.Integration;

public class EmpleadosTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public EmpleadosTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    private record EmpleadoDto(int Id, string NumeroEmpleado, string Nombre);

    [Fact]
    public async Task Listar_ComoAdministrador_MuestraLosEmpleadosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var empleados = await client.GetFromJsonAsync<List<EmpleadoDto>>("api/empleados");

        Assert.Contains(empleados!, e => e.NumeroEmpleado == "E001" && e.Nombre == "Juan Pérez");
    }

    [Fact]
    public async Task Crear_ComoAdministrador_Persiste()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var response = await client.PostAsJsonAsync("api/empleados", new
        {
            numeroEmpleado = "E999",
            nombre = "Empleado de Prueba",
            centroTrabajoId = 1,
            consentimientoGeolocalizacion = true,
        });
        response.EnsureSuccessStatusCode();

        var empleados = await client.GetFromJsonAsync<List<EmpleadoDto>>("api/empleados");
        Assert.Contains(empleados!, e => e.NumeroEmpleado == "E999");
    }

    [Fact]
    public async Task Crear_ConNumeroDuplicado_DevuelveBadRequest()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var response = await client.PostAsJsonAsync("api/empleados", new
        {
            numeroEmpleado = "E001", // ya existe en los datos mock
            nombre = "Otro Empleado",
            centroTrabajoId = 1,
            consentimientoGeolocalizacion = true,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
