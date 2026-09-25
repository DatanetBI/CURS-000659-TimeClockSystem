using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Cultura del producto: español de México (Principio II de la constitución).
var culturaMx = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = culturaMx;
CultureInfo.DefaultThreadCurrentUICulture = culturaMx;

// URL base del Backend desde configuración (FR-007/FR-008: llamadas HTTP directas, sin gateway).
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("No se configuró Api:BaseUrl.");

builder.Services.AddHttpContextAccessor();

// Nombre de aplicación explícito para Data Protection: el Frontend y el Backend corren en la
// misma máquina y comparten el almacén de claves por usuario; sin esto, ambos podrían competir
// por el mismo "discriminador" de aplicación y romper la cookie de autenticación/antiforgery.
builder.Services.AddDataProtection().SetApplicationName("TimeClockSystem.Web");

// Autenticación por cookie propia del Frontend (sin Identity ni EF Core — research.md #2). El
// token del Backend viaja como claim dentro de esta cookie cifrada, nunca al navegador (FR-002a).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/IniciarSesion";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    });

builder.Services.AddAuthorization();

builder.Services.AddTransient<TokenForwardingHandler>();

builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

void ConfigurarClienteApi<TClient>(IHttpClientBuilder builder) => builder.AddHttpMessageHandler<TokenForwardingHandler>();

ConfigurarClienteApi<EmpleadosApiClient>(builder.Services.AddHttpClient<EmpleadosApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<CentrosTrabajoApiClient>(builder.Services.AddHttpClient<CentrosTrabajoApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<TurnosApiClient>(builder.Services.AddHttpClient<TurnosApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<AsignacionesTurnoApiClient>(builder.Services.AddHttpClient<AsignacionesTurnoApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<DiasFestivosApiClient>(builder.Services.AddHttpClient<DiasFestivosApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<MarcajeApiClient>(builder.Services.AddHttpClient<MarcajeApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<ConsultaAsistenciasApiClient>(builder.Services.AddHttpClient<ConsultaAsistenciasApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));
ConfigurarClienteApi<AuditoriaApiClient>(builder.Services.AddHttpClient<AuditoriaApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl)));

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add<TimeClockSystem.Web.Infrastructure.BackendUnavailableExceptionFilter>());

// El HtmlEncoder por defecto convierte acentos/eñes a entidades numéricas (&#233;); se permite el
// rango Latin-1 Supplement para que el español de México se renderice como texto literal.
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaMx),
    SupportedCultures = [culturaMx],
    SupportedUICultures = [culturaMx],
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
