# Arquitectura e Implementación — TimeClockSystem (sistema real)

> Resumen **simplificado** de lo que el código realmente contiene hoy, organizado por capas de
> Clean Architecture. A diferencia de los documentos en `ENTREGABLES/` (que describen la visión
> funcional objetivo de 9 microservicios), este archivo describe **el sistema que existe en
> `src/`**: un backend .NET (`TimeClockSystem.Api`) más un frontend MVC (`TimeClockSystem.Web`),
> organizados en 4 proyectos: `Domain → Application → Infrastructure → Api`.

---

## 1. Domain (`src/TimeClockSystem.Domain/`)

Contiene las entidades y las reglas de negocio puras: no depende de ningún otro proyecto, de EF
Core ni de ASP.NET Core.

### Entidades (con sus reglas ya incorporadas como validaciones de datos)

| Entidad | Qué representa | Reglas clave incluidas |
|---|---|---|
| `Empleado` | Persona cuya asistencia se controla | Número de empleado y nombre obligatorios; pertenece a un `CentroTrabajo`; guarda si dio su **consentimiento de geolocalización** |
| `CentroTrabajo` | Un perímetro geográfico (geofence) autorizado | Latitud [-90,90], longitud [-180,180], radio en metros (mínimo 1) |
| `Turno` | Plantilla de horario | Tipos: `Fijo`, `Rotativo`, `Nocturno`, `Flexible`, `OnCall`; hora entrada/salida, receso (0–480 min) y tolerancia (0–1440 min); sabe calcular su duración total aunque cruce la medianoche |
| `AsignacionTurno` | Vincula un `Empleado` con un `Turno` en una fecha | — |
| `Marca` | Un evento de entrada/salida/receso | Canal (`PortalWeb` o `Pin`), estado (`Valida`/`Rechazada`) y motivo de rechazo (`Geofence`, `EntradaDuplicada`, `CredencialesInvalidas`, `SinConsentimientoGeolocalizacion`) |
| `CredencialDeMarcaje` | El PIN de marcaje de un empleado (1 a 1) | El PIN nunca se guarda en texto plano, solo su hash |
| `DiaFestivo` | Catálogo simple de fechas festivas | Solo informativo (sin factor de pago asociado) |
| `RegistroAuditoria` | Un evento sensible registrado para trazabilidad | Eventos: `InicioSesionExitoso`, `InicioSesionFallido`, `MarcajeRechazado`, `AccesoDenegadoPorRol` |
| `Roles` | Constantes de los roles del sistema | Solo dos: `Administrador` y `Empleado` |

### Contratos y reglas de negocio "puras" (clases estáticas, sin estado)

- **`GeofenceValidator`** — calcula la distancia real entre dos coordenadas (fórmula de Haversine) y decide si una marca cayó dentro del radio permitido del centro de trabajo.
- **`ConsentimientoValidator`** — bloquea una marca con geolocalización si el empleado no dio su consentimiento.
- **`PuntualidadCalculator`** — compara la hora de una marca de entrada contra el turno asignado + su tolerancia, y devuelve `Puntual`, `Tardío` o `SinTurnoAsignado`.
- **`IBiometricVerificationProvider`** — contrato para un futuro proveedor de reconocimiento facial/liveness; **hoy no tiene ninguna implementación real** (el marcaje biométrico no está construido en esta versión).

**En simple**: el Domain es "el reglamento de la empresa" escrito en código — qué es un empleado, qué es una marca válida, cómo se calcula un atraso — sin saber nada de bases de datos ni de HTTP.

---

## 2. Application (`src/TimeClockSystem.Application/`)

Contiene los **casos de uso** (qué puede hacer un usuario), los **DTOs** (los datos que entran y
salen de cada operación) y los **puertos** (interfaces que Infrastructure debe implementar).

### Casos de uso / Servicios

| Caso de uso | Qué hace |
|---|---|
| `AutenticarUseCase` | Valida usuario/contraseña, emite el token y registra en auditoría tanto el éxito como el fallo del login |
| `RegistrarMarcaUseCase` | El corazón del sistema: valida entrada duplicada, consentimiento y geofence (en ese orden) antes de aceptar una marca; funciona igual si el empleado entra por sesión web o por PIN de kiosco |
| `EmpleadosService` | CRUD de empleados + gestión de su PIN de marcaje (hasheado) |
| `CentrosTrabajoService` | CRUD de centros de trabajo (geofences) |
| `TurnosService` / `AsignacionesTurnoService` | CRUD de turnos y de sus asignaciones a empleados |
| `DiasFestivosService` | CRUD del catálogo de días festivos |
| `ConsultarAsistenciasUseCase` | Devuelve el historial de marcas de un empleado (o de todos, para el Administrador), con su indicador de puntualidad |
| `ConsultarAuditoriaUseCase` | Devuelve la bitácora de auditoría filtrable por fecha/usuario (solo Administrador) |

### DTOs y validaciones de entrada

Cada caso de uso define sus propios registros (`record`) de entrada/salida — por ejemplo,
`CrearEmpleadoRequest`, `EmpleadoDto`, `ResultadoMarcaje`, `LoginResultado` — de modo que el
Domain nunca se expone directamente por la API. Las validaciones básicas de formato (campos
obligatorios, rangos numéricos) ya viven como atributos en las entidades del Domain
(`[Required]`, `[StringLength]`, `[Range]`) y las valida automáticamente ASP.NET Core al recibir
la petición; las reglas de negocio más finas (geofence, duplicados, consentimiento) las aplica
explícitamente cada caso de uso.

### Puertos (interfaces en `Abstractions/`)

`IEmpleadoRepository`, `ICentroTrabajoRepository`, `ITurnoRepository`, `IMarcaRepository`,
`ICredencialRepository`, `IDiaFestivoRepository`, `IAuditLogService`, `IPinHasher`,
`IUserAccountService`, `ITokenService` — Application solo conoce estas interfaces; nunca conoce
EF Core, SQL Server ni ningún detalle técnico. Quien las implementa es Infrastructure.

**En simple**: Application es "el mostrador de atención" — recibe una petición (crear empleado,
marcar entrada, iniciar sesión), aplica las reglas del Domain, y le pide a alguien más (un
puerto) que guarde o traiga los datos, sin saber cómo lo hace ese alguien.

---

## 3. Infrastructure (`src/TimeClockSystem.Infrastructure/`)

Implementa los puertos de Application con tecnología real: EF Core, ASP.NET Core Identity, JWT y
el hashing del PIN.

| Pieza | Qué hace |
|---|---|
| `ApplicationDbContext` (EF Core) | El `DbContext` con los 8 `DbSet` (Empleados, CentrosTrabajo, Turnos, AsignacionesTurno, Marcas, CredencialesDeMarcaje, DiasFestivos, RegistrosAuditoria) + las tablas de Identity |
| `Data/Migrations/` | **Una sola migración** (`InitialSqlServer`) que crea las 16 tablas contra SQL Server |
| `DbSeeder` | Siembra datos de ejemplo (empleados, centros, turnos, marcas, 2 administradores) la primera vez que arranca contra una base vacía — es **idempotente** (no vuelve a sembrar si ya hay usuarios) |
| Repositorios (`Repositories/*.cs`) | Un repositorio EF Core por cada interfaz del Domain/Application (`EmpleadoRepository`, `MarcaRepository`, etc.) — el único lugar del sistema que sabe escribir consultas EF Core |
| `JwtTokenService` / `JwtOptions` | Emite el token JWT firmado con la clave configurada, con expiración fija (sin refresh token) |
| `PinHasher` | Hashea y verifica el PIN de marcaje (nunca se compara ni se guarda en texto plano) |
| `UserAccountService` | Autentica contra ASP.NET Core Identity y resuelve el rol + el `EmpleadoId` vinculado |
| `IdentityConfiguration` | Crea los roles `Administrador`/`Empleado` en Identity al arrancar |
| `AuditLogService` | Implementa `IAuditLogService`: inserta un `RegistroAuditoria` por cada evento sensible |
| `NullBiometricVerificationProvider` | Implementación "vacía" de `IBiometricVerificationProvider` — deja el punto de extensión listo, pero no verifica nada realmente (no hay biometría en esta versión) |

**Servicios externos**: no hay ninguna integración con un sistema externo real (ERP, directorio
activo, notificaciones push) en esta versión — todo lo que Infrastructure implementa es
persistencia local (SQL Server) y seguridad (Identity + JWT).

**En simple**: Infrastructure es "la bodega y el candado" — donde de verdad se guardan los datos
(SQL Server) y donde se verifica la identidad de quien entra (Identity + JWT).

---

## 4. API (`src/TimeClockSystem.Api/`)

Expone los casos de uso de Application como endpoints HTTP.

### Controllers y endpoints (30 endpoints en 9 controllers)

| Controller | Endpoints | Acceso |
|---|---|---|
| `AuthController` | `POST /api/auth/login`, `POST /api/auth/logout` | Público |
| `EmpleadosController` | `GET/POST/PUT /api/empleados[..]`, `GET/PUT /api/empleados/{id}/credencial` | Administrador |
| `CentrosTrabajoController` | `GET/POST/PUT/DELETE /api/centros-trabajo[..]` | Administrador |
| `TurnosController` | `GET/POST/PUT/DELETE /api/turnos[..]` | Administrador |
| `AsignacionesTurnoController` | `GET/POST/DELETE /api/asignaciones-turno[..]` | Administrador |
| `DiasFestivosController` | `GET` (Admin y Empleado), `POST/PUT/DELETE` (solo Admin) `/api/dias-festivos[..]` | Mixto |
| `MarcajeController` | `POST /api/marcaje` (sesión), `POST /api/marcaje/pin` (kiosco) | Empleado / Público (PIN) |
| `ConsultaAsistenciasController` | `GET /api/consulta-asistencias`, `GET /api/consulta-asistencias/admin` | Empleado (solo su propio historial) / Administrador (todos) |
| `AuditoriaController` | `GET /api/auditoria` | Administrador |

### OpenAPI / Swagger

Configurado en `Program.cs` con `Swashbuckle.AspNetCore`: expone `GET /swagger` y
`/swagger/v1/swagger.json` en ambiente de desarrollo, con el esquema de seguridad `Bearer`
documentado (para pegar el token JWT y probar los endpoints protegidos directamente desde el
navegador) y los comentarios XML de cada controller incluidos como descripción.

### Seguridad

- **Autenticación**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), token de
  expiración fija (60 min por defecto), sin renovación silenciosa.
- **Autorización**: por rol (`[Authorize(Roles = ...)]`), solo dos roles (`Administrador`,
  `Empleado`).
- **Auditoría transversal**: `AuditingAuthorizationMiddlewareResultHandler` registra
  automáticamente en la bitácora **cualquier** solicitud rechazada por rol (403), sin importar el
  controller — no hace falta código adicional en cada endpoint para que quede auditado.
- **Secretos**: la clave de firma del JWT y la cadena de conexión **nunca están en el código ni
  en `appsettings.json`** — se inyectan por variable de entorno o `appsettings.Development.json`
  (ver `ENTREGABLES/technical-spec.md` §4 para el detalle general de seguridad de la visión
  objetivo; en el sistema real esto se resolvió en la migración a Docker/SQL Server, ver
  `specs/003-sqlserver-docker-migration/`).

**En simple**: la API es "la puerta de entrada" — recibe la petición HTTP, valida el token/rol, y
llama al caso de uso de Application que corresponde.

---

## 5. Datos Técnicos

| Dato | Valor |
|---|---|
| **Lenguaje** | C# 13 |
| **Framework** | .NET 10 / ASP.NET Core 10 |
| **Proyectos** | `TimeClockSystem.Domain`, `TimeClockSystem.Application`, `TimeClockSystem.Infrastructure`, `TimeClockSystem.Api` (+ `TimeClockSystem.Web`, el frontend MVC que consume la API por HTTP) |
| **Persistencia** | Microsoft SQL Server, vía `Microsoft.EntityFrameworkCore.SqlServer` — antes era SQLite; migrado en `specs/003-sqlserver-docker-migration/` |
| **Migraciones** | Una migración única (`InitialSqlServer`) → 16 tablas (8 del dominio + Identity) |
| **Datos de ejemplo** | Sembrados automáticamente al primer arranque por `DbSeeder` (idempotente) |
| **Autenticación** | ASP.NET Core Identity + JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| **Documentación de API** | Swagger / OpenAPI vía `Swashbuckle.AspNetCore`, con comentarios XML de cada controller y esquema `Bearer` configurado |
| **Endpoints documentados** | 30, repartidos en 9 controllers (ver tabla de la sección 4) — el detalle línea a línea (request/response) está en [`specs/002-split-backend-frontend/contracts/`](../../specs/002-split-backend-frontend/contracts/) |
| **Validaciones ↔ requisitos** | Los `DataAnnotations` de cada entidad del Domain (`[Required]`, `[StringLength]`, `[Range]`) implementan directamente las reglas de formato de los contratos; las reglas de negocio (geofence, entrada duplicada, consentimiento, puntualidad, PIN hasheado, auditoría de rechazos) están en clases del Domain/Application citadas en las secciones 1–2, cada una trazable a un requisito funcional (`FR-00X`) documentado en `specs/002-split-backend-frontend/spec.md` |
| **Contenerización** | `docker-compose.yml` en la raíz levanta los 3 componentes (SQL Server + Api + Web) con un solo comando — ver `specs/003-sqlserver-docker-migration/quickstart.md` |
| **Pruebas automatizadas** | `tests/TimeClockSystem.Api.Tests` (23 pruebas) y `tests/TimeClockSystem.Web.Tests` (9 pruebas), ambas en verde a la fecha de este documento |

## Documentos relacionados

- [`specs/002-split-backend-frontend/spec.md`](../../specs/002-split-backend-frontend/spec.md) — Especificación funcional del sistema real (los `FR-00X` citados arriba).
- [`specs/002-split-backend-frontend/contracts/`](../../specs/002-split-backend-frontend/contracts/) — Contrato detallado de cada endpoint (request/response/errores).
- [`specs/002-split-backend-frontend/data-model.md`](../../specs/002-split-backend-frontend/data-model.md) — Modelo de datos original de estas mismas entidades.
- [`specs/003-sqlserver-docker-migration/`](../../specs/003-sqlserver-docker-migration/) — Migración a SQL Server y contenerización con Docker.
- [`ENTREGABLES/technical-spec.md`](../technical-spec.md) — Arquitectura de la **visión objetivo** (9 microservicios), para contrastar con este resumen del sistema real.
- [`ENTREGABLES/traceability.md`](../traceability.md) — Qué parte de esa visión objetivo ya está cubierta por los endpoints listados aquí.
