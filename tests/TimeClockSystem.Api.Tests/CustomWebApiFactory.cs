using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Api.Tests;

/// <summary>
/// Fixture compartida por las pruebas de integración de la API: cada instancia usa su propio
/// archivo SQLite temporal (creado y sembrado por el DbSeeder real al arrancar), borrado al
/// terminar. Reemplaza el registro de <see cref="ApplicationDbContext"/> después de que Program.cs
/// construye el host (patrón recomendado por Microsoft para pruebas de integración con EF Core).
/// Usa el entorno "Testing" (en vez de "Development") para que Program.cs cree el esquema con
/// <c>EnsureCreated</c> en lugar de aplicar las migraciones reales: estas se generan en SQL
/// específico de SQL Server (proveedor de producción) y no son sintaxis válida contra SQLite.
/// </summary>
public class CustomWebApiFactory : WebApplicationFactory<Program>
{
    private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"timeclock-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Program.cs registra ApplicationDbContext con el proveedor SqlServer (producción).
            // No basta con quitar DbContextOptions<ApplicationDbContext>: EF Core combina todas
            // las configuraciones registradas vía IDbContextOptionsConfiguration<> sobre el mismo
            // options builder, así que también hay que quitar esa entrada — de lo contrario,
            // SqlServer y Sqlite quedan configurados a la vez y EF Core lo rechaza en tiempo de
            // ejecución ("Only a single database provider can be registered").
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            try
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
            catch (IOException)
            {
                // El archivo puede seguir en uso brevemente tras el shutdown; no es crítico.
            }
        }
    }
}
