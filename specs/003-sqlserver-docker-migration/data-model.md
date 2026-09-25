# Data Model: Migración a SQL Server y Contenerización con Docker

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

## Alcance de este documento

Esta funcionalidad **no introduce entidades nuevas ni cambia relaciones de negocio**. El modelo de
dominio (`TimeClockSystem.Domain`) y el `ApplicationDbContext` (`TimeClockSystem.Infrastructure`)
permanecen sin cambios de forma: mismos `DbSet`, mismas propiedades, misma regla de unicidad
(`Empleado.NumeroEmpleado`). Lo único que cambia es el **proveedor de base de datos** que traduce
ese modelo a esquema físico, y por lo tanto el **conjunto de migraciones** que lo materializa.

## Entidades existentes (sin cambios de forma)

| Entidad | DbSet | Notas relevantes para la migración de proveedor |
|---|---|---|
| `ApplicationUser` (Identity) | (heredado de `IdentityDbContext`) | Tablas estándar de ASP.NET Core Identity; SQL Server es el proveedor de referencia para el que estas tablas fueron diseñadas originalmente — sin ajustes esperados. |
| `CentroTrabajo` | `CentrosTrabajo` | `Latitud`/`Longitud` como `double`; SQL Server los mapea a `float`, equivalente a SQLite `REAL`. |
| `Empleado` | `Empleados` | Índice único en `NumeroEmpleado` (`OnModelCreating`); se recrea igual en SQL Server. |
| `CredencialDeMarcaje` | `CredencialesDeMarcaje` | Sin cambios. |
| `Turno` | `Turnos` | Sin cambios. |
| `AsignacionTurno` | `AsignacionesTurno` | Sin cambios. |
| `DiaFestivo` | `DiasFestivos` | Sin cambios. |
| `Marca` | `Marcas` | Columnas de fecha/hora: SQLite las almacena como texto ISO-8601; SQL Server las mapea a `datetime2`, con la misma precisión efectiva para este dominio (sin microsegundos relevantes al negocio). |
| `RegistroAuditoria` | `RegistrosAuditoria` | Sin cambios. |

No se requiere ninguna migración de datos entre proveedores (Clarification de la spec: la base de
datos SQL Server inicia vacía).

## Migraciones EF Core

**Estado actual**: `src/TimeClockSystem.Infrastructure/Data/Migrations/` contiene 6 migraciones
incrementales generadas contra el proveedor SQLite (`InitialIdentity` →
`Modulo6_Auditoria`) más su `ApplicationDbContextModelSnapshot.cs`.

**Estado destino**: Esa carpeta se reemplaza por una única migración inicial
(`InitialSqlServer` o nombre equivalente) generada con `dotnet ef migrations add` contra
`UseSqlServer`, junto con su propio `ApplicationDbContextModelSnapshot.cs` regenerado. El
contenido semántico del esquema (tablas, columnas, índices, claves foráneas) es el mismo que el
resultado acumulado de las 6 migraciones anteriores; solo cambia el dialecto SQL generado por el
proveedor.

**Aplicación en el arranque**: sin cambios de comportamiento — `db.Database.MigrateAsync()` en
`Program.cs` sigue aplicando automáticamente las migraciones pendientes al iniciar el Backend
(FR-010), ahora contra el nuevo proveedor, dentro de la ventana acotada de reintentos descrita en
[research.md #4](./research.md#4-espera-acotada-del-backend-a-que-la-base-de-datos-esté-lista).

## Datos de ejemplo (DbSeeder)

Sin cambios: `DbSeeder.SeedAsync` sigue siendo el único punto de siembra, sigue siendo idempotente
(verifica `await userManager.Users.AnyAsync()` antes de sembrar) y sigue sembrando el mismo
conjunto de centros de trabajo, empleados, turnos, días festivos y marcas mock (FR-011). No se
requiere ningún cambio en `DbSeeder.cs` para funcionar contra SQL Server.
