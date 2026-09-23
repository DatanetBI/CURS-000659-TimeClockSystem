using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Infrastructure.Data;

namespace TimeClockSystem.Web.Infrastructure.Identity;

public static class IdentityConfiguration
{
    /// <summary>
    /// Registra ASP.NET Core Identity con cookies de sesion y los 2 roles de v1.0
    /// (research.md #3 - Autenticacion y control de acceso).
    /// </summary>
    public static IServiceCollection AddTimeClockIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Politica de contrasena de portal (no confundir con el PIN de marcaje, que es
                // numerico de 4-6 digitos y se valida por separado - ver CredencialDeMarcaje).
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Cuenta/IniciarSesion";
            options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        });

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
