# Tablas de la base de datos: TimeClockSystem

**Fuente:** modelo de EF Core, `src/TimeClockSystem.Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`
**Fecha:** 2026-09-26

La base de datos tiene **8 tablas de negocio** y **7 tablas de ASP.NET Identity**.

Los campos marcados como *(enum)* se guardan como `int` en la base. Sus valores empiezan en 0 y siguen el orden de la lista.

## Tablas de negocio

### CentrosTrabajo
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| Nombre | nvarchar(120) | requerido |
| Latitud | float | |
| Longitud | float | |
| RadioMetros | int | radio de la geocerca |

### Empleados
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| NumeroEmpleado | nvarchar(20) | requerido, **único** |
| Nombre | nvarchar(150) | requerido |
| CentroTrabajoId | int | FK → CentrosTrabajo (se borra en cascada) |
| Estado | int *(enum)* | Activo, Baja |
| ConsentimientoGeolocalizacion | bit | |

### CredencialesDeMarcaje (cada empleado tiene como máximo una)
| Campo | Tipo | Notas |
|---|---|---|
| EmpleadoId | int | PK y FK → Empleados (se borra en cascada) |
| PinHash | nvarchar(max) | requerido |
| FechaActualizacion | datetime2 | |
| ActualizadoPorUserId | nvarchar(max) | puede ser nulo |

### Turnos
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| Nombre | nvarchar(80) | requerido |
| Tipo | int *(enum)* | Fijo, Rotativo, Nocturno, Flexible, OnCall |
| HoraEntrada | time | |
| HoraSalida | time | |
| DuracionRecesoMinutos | int | |
| ToleranciaMinutos | int | |

### AsignacionesTurno
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| EmpleadoId | int | FK → Empleados (se borra en cascada) |
| TurnoId | int | FK → Turnos (se borra en cascada) |
| Fecha | date | |

### DiasFestivos
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| Fecha | date | |
| Descripcion | nvarchar(120) | requerido |

### Marcas
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| EmpleadoId | int | FK → Empleados (se borra en cascada) |
| Timestamp | datetime2 | |
| Tipo | int *(enum)* | Entrada, Salida, InicioReceso, FinReceso |
| Canal | int *(enum)* | PortalWeb, Pin |
| Estado | int *(enum)* | Valida, Rechazada |
| MotivoRechazo | int *(enum)* | Ninguno, Geofence, EntradaDuplicada, CredencialesInvalidas, SinConsentimientoGeolocalizacion |
| Latitud | float | puede ser nulo |
| Longitud | float | puede ser nulo |

### RegistrosAuditoria
| Campo | Tipo | Notas |
|---|---|---|
| Id | int | PK, identity |
| Evento | int *(enum)* | InicioSesionExitoso, InicioSesionFallido, MarcajeRechazado, AccesoDenegadoPorRol |
| UsuarioOEmpleadoId | nvarchar(256) | requerido |
| Detalle | nvarchar(500) | requerido |
| TimestampUtc | datetime2 | |

## Tablas de Identity (usuarios y roles)

- **AspNetUsers**: Id (PK), UserName, NormalizedUserName (único), Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount y **EmpleadoId** (int, puede ser nulo). EmpleadoId es un campo propio de `ApplicationUser`, pero no tiene FK hacia Empleados.
- **AspNetRoles**: Id (PK), Name, NormalizedName (único), ConcurrencyStamp
- **AspNetUserRoles**: UserId + RoleId (PK compuesta, ambas son FK)
- **AspNetUserClaims**: Id, UserId (FK), ClaimType, ClaimValue
- **AspNetRoleClaims**: Id, RoleId (FK), ClaimType, ClaimValue
- **AspNetUserLogins**: LoginProvider + ProviderKey (PK), ProviderDisplayName, UserId (FK)
- **AspNetUserTokens**: UserId + LoginProvider + Name (PK), Value

## Relaciones

- Un **CentroTrabajo** tiene muchos **Empleados**.
- Un **Empleado** tiene una sola **CredencialDeMarcaje**.
- Un **Empleado** tiene muchas **Marcas** y muchas **AsignacionesTurno**.
- Un **Turno** tiene muchas **AsignacionesTurno**.
