namespace TimeClockSystem.Tests.Integration;

public class TurnosTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TurnosTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_ComoAdministrador_MuestraLosTurnosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var html = await (await client.GetAsync("/Turnos/Turnos")).Content.ReadAsStringAsync();

        Assert.Contains("Fijo diurno", html);
        Assert.Contains("Nocturno", html);
    }

    [Fact]
    public async Task CrearTurno_ConToleranciaInvalida_MuestraErrorDeValidacion()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var createPage = await client.GetAsync("/Turnos/Turnos/Create");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await createPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["Nombre"] = "Turno inválido",
            ["Tipo"] = "Fijo",
            ["HoraEntrada"] = "08:00",
            ["HoraSalida"] = "17:00", // 9 horas = 540 minutos
            ["DuracionRecesoMinutos"] = "60",
            ["ToleranciaMinutos"] = "600", // mayor que la duración total
            ["__RequestVerificationToken"] = token,
        };

        var response = await client.PostAsync("/Turnos/Turnos/Create", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("La tolerancia no puede ser negativa", html);
    }

    [Fact]
    public async Task AsignacionMasiva_ComoAdministrador_AsignaAlGrupoCompleto()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var page = await client.GetAsync("/Turnos/Asignaciones/Masiva");
        page.EnsureSuccessStatusCode();
        var token = AuthTestHelper.ExtractAntiForgeryToken(await page.Content.ReadAsStringAsync());

        var form = new List<KeyValuePair<string, string>>
        {
            new("TurnoId", "1"),
            new("Fecha", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd")),
            new("EmpleadoIdsSeleccionados", "1"),
            new("EmpleadoIdsSeleccionados", "2"),
            new("__RequestVerificationToken", token),
        };

        var response = await client.PostAsync("/Turnos/Asignaciones/Masiva", new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();

        var resultHtml = await response.Content.ReadAsStringAsync();
        Assert.Contains("2 empleado(s) asignados", resultHtml);
    }
}
