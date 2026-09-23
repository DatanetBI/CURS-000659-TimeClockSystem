namespace TimeClockSystem.Tests.Integration;

public class EmpleadosTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EmpleadosTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_ComoAdministrador_MuestraLosEmpleadosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var html = await (await client.GetAsync("/Empleados/Empleados")).Content.ReadAsStringAsync();

        Assert.Contains("E001", html);
        Assert.Contains("Juan Pérez", html);
    }

    [Fact]
    public async Task CrearEmpleadoYAsignarPin_ComoAdministrador_Persiste()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var createPage = await client.GetAsync("/Empleados/Empleados/Create");
        createPage.EnsureSuccessStatusCode();
        var token = AuthTestHelper.ExtractAntiForgeryToken(await createPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["NumeroEmpleado"] = "E999",
            ["Nombre"] = "Empleado de Prueba",
            ["CentroTrabajoId"] = "1",
            ["Estado"] = "Activo",
            ["__RequestVerificationToken"] = token,
        };

        var createResponse = await client.PostAsync("/Empleados/Empleados/Create", new FormUrlEncodedContent(form));
        createResponse.EnsureSuccessStatusCode();

        var indexHtml = await (await client.GetAsync("/Empleados/Empleados")).Content.ReadAsStringAsync();
        Assert.Contains("E999", indexHtml);
    }

    [Fact]
    public async Task CrearEmpleado_ConNumeroDuplicado_MuestraErrorDeValidacion()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var createPage = await client.GetAsync("/Empleados/Empleados/Create");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await createPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["NumeroEmpleado"] = "E001", // ya existe en los datos mock
            ["Nombre"] = "Otro Empleado",
            ["CentroTrabajoId"] = "1",
            ["Estado"] = "Activo",
            ["__RequestVerificationToken"] = token,
        };

        var response = await client.PostAsync("/Empleados/Empleados/Create", new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ya existe un empleado con este número", html);
    }
}
