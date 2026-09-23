using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CentroTrabajo> CentrosTrabajo => Set<CentroTrabajo>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<CredencialDeMarcaje> CredencialesDeMarcaje => Set<CredencialDeMarcaje>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<AsignacionTurno> AsignacionesTurno => Set<AsignacionTurno>();
    public DbSet<DiaFestivo> DiasFestivos => Set<DiaFestivo>();
    public DbSet<Marca> Marcas => Set<Marca>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Empleado>()
            .HasIndex(e => e.NumeroEmpleado)
            .IsUnique();
    }
}
