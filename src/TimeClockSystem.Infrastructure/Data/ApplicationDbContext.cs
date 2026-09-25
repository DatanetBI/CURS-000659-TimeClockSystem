using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Identity;

namespace TimeClockSystem.Infrastructure.Data;

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
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Empleado>()
            .HasIndex(e => e.NumeroEmpleado)
            .IsUnique();
    }
}
