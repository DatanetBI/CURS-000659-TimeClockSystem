using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Cultura del producto: español de México (Principio II de la constitución).
var culturaMx = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = culturaMx;
CultureInfo.DefaultThreadCurrentUICulture = culturaMx;

// Cadena de conexión desde configuración (appsettings.json en desarrollo, variable de entorno
// ConnectionStrings__DefaultConnection en producción — Principio V, sin secretos en el código).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se configuró ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddTimeClockIdentity();

builder.Services.AddScoped<TimeClockSystem.Web.Domain.IBiometricVerificationProvider, TimeClockSystem.Web.Domain.NullBiometricVerificationProvider>();
builder.Services.AddScoped<TimeClockSystem.Web.Areas.Marcaje.MarcajeService>();

builder.Services.AddControllersWithViews();

// El HtmlEncoder por defecto convierte acentos/eñes a entidades numéricas (&#233;); se permite el
// rango Latin-1 Supplement para que el español de México se renderice como texto literal.
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement));

var app = builder.Build();

// Sembrar roles y datos mock al iniciar (research.md #10 - Datos de ejemplo para la prueba de concepto).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await IdentityConfiguration.EnsureRolesCreatedAsync(scope.ServiceProvider);
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

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
