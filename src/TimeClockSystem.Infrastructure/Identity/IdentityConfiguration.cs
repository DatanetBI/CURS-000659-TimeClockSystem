using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Infrastructure.Identity;

public static class IdentityConfiguration
{
    /// <summary>
    /// Registra ASP.NET Core Identity Core (sin autenticación por cookie: el Backend ahora
    /// autentica por token — FR-002, research.md #2). El Frontend ya no comparte este almacén.
    /// </summary>
    public static IServiceCollection AddTimeClockIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Politica de contrasena de portal (no confundir con el PIN de marcaje, que es
                // numerico de 4-6 digitos y se valida por separado - ver CredencialDeMarcaje).
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Crea los roles Administrador/Empleado si no existen. Se invoca al iniciar la aplicacion,
    /// antes del DbSeeder de datos mock (T010).
    /// </summary>
    public static async Task EnsureRolesCreatedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
