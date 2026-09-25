# Research: Separación de TimeClockSystem en Backend WebAPI y Frontend MVC

## 1. Capas del Backend (Clean Architecture)

**Decision**: 4 proyectos con dependencias solo hacia adentro: `TimeClockSystem.Domain` (sin
dependencias) ← `TimeClockSystem.Application` (depende solo de Domain) ← `TimeClockSystem.Infrastructure`
(depende de Application + Domain, implementa sus interfaces) ← `TimeClockSystem.Api` (host, depende
de Application + Infrastructure para composición/DI).

**Rationale**: Petición explícita del usuario. Además, replica el patrón Clean/Onion Architecture
más común en plantillas .NET (ej. Jason Taylor Clean Architecture Template), lo que facilita que las
reglas de negocio existentes (`PuntualidadCalculator`, `GeofenceValidator`,
`ConsentimientoValidator`) se relocalicen sin cambios de comportamiento (FR-005) y sean probables de
forma aislada sin EF Core ni ASP.NET (Historia de Usuario 3).

**Alternatives considered**:
- Un único proyecto `TimeClockSystem.Api` con carpetas lógicas (`Domain/`, `Application/`,
  `Infrastructure/`) en vez de proyectos separados: más simple de compilar, pero no fuerza en tiempo
  de compilación la regla "Domain no depende de nada"; se descarta porque el usuario pidió
  explícitamente capas de Clean Architecture, que en .NET normalmente se expresan como proyectos.
- Arquitectura hexagonal con múltiples puertos por adaptador: se descarta por exceder lo que pide la
  spec (Principio I, Simplicidad Ante Todo).

## 2. Autenticación Frontend↔Backend (token + cookie existente)

**Decision**: El Backend expone `POST /api/auth/login` (usuario/contraseña) y devuelve un JWT de
expiración fija con los claims de rol (Administrador/Empleado) ya existentes. El Frontend conserva
su cookie de autenticación de hoy (`CookieAuthenticationDefaults`), pero ya no usa
`AddEntityFrameworkStores` ni valida contraseñas localmente: al iniciar sesión, llama al Backend,
recibe el JWT y lo guarda como un claim dentro de la cookie de autenticación (cifrada
automáticamente por el Data Protection de ASP.NET Core). Un `DelegatingHandler` registrado en el
`IHttpClientFactory` del Frontend toma ese claim y lo agrega como `Authorization: Bearer <token>` en
cada llamada saliente al Backend. El navegador nunca ve el JWT.

**Rationale**: Resuelve exactamente la aclaración registrada en `spec.md` (sesión 2026-09-24): "el
Frontend conserva su propia cookie de sesión... el navegador nunca ve el JWT directamente" (FR-002a).
Es el patrón estándar de ASP.NET Core para un MVC que actúa como su propio cliente de una API sin
introducir un gateway/BFF adicional (ya descartado en la spec, FR-008).

**Alternatives considered**:
- Refresh tokens con renovación silenciosa: descartado explícitamente en `spec.md` (FR-002 pide
  expiración fija sin renovación silenciosa).
- JWT almacenado en `localStorage`/`sessionStorage` del navegador: descartado porque expone el
  token a JavaScript (mayor superficie de ataque XSS) y contradice la aclaración registrada.

## 3. Documentación de la API (Swagger/OpenAPI)

**Decision**: `Swashbuckle.AspNetCore` en `TimeClockSystem.Api`, expuesto en `/swagger` solo en
entornos de desarrollo/pruebas (no en producción, salvo que se decida lo contrario más adelante).

**Rationale**: Es el paquete más usado y documentado para anotar `[ApiController]` con OpenAPI en
ASP.NET Core, y coincide con la terminología que ya usa la spec original ("documentación
Swagger/OpenAPI"). Cumple FR-006 y permite validar SC-003 (flujo de negocio completo usando solo la
documentación del Backend) sin abrir el Frontend.

**Alternatives considered**:
- Generador `Microsoft.AspNetCore.OpenApi` nativo de .NET sin interfaz interactiva: genera el
  documento pero no una UI navegable; se descarta porque dificultaría la prueba manual pedida en
  SC-003.

## 4. Comunicación Frontend→Backend (sin contratos compartidos)

**Decision**: Clientes HTTP tipados (`IHttpClientFactory` + una clase por Area, ej.
`EmpleadosApiClient`) en el Frontend, cada uno con sus propios modelos de solicitud/respuesta
(records simples) que reflejan el contrato JSON del Backend. No se crea un proyecto de contratos
compartido entre Backend y Frontend.

**Rationale**: Mantiene a ambos proyectos genuinamente desacoplados en tiempo de compilación,
reforzando que solo se comunican por HTTP (FR-007) y que pueden desplegarse por separado sin
recompilar el otro. Es la opción más simple (Principio I) dado el tamaño del proyecto.

**Alternatives considered**:
- Proyecto `TimeClockSystem.Contracts` compartido con los DTOs: reduce duplicación de modelos, pero
  reintroduce una dependencia de compilación compartida entre Backend y Frontend, lo cual se
  descarta para mantener el desacoplamiento que pide la spec.

## 5. Registro y consulta de auditoría (FR-011, FR-011a, SC-006)

**Decision**: Nueva entidad `RegistroAuditoria` (Evento, EmpleadoOUsuarioId, Detalle, Timestamp)
persistida por EF Core en la misma base SQLite del Backend (tabla nueva vía migración). El Backend
escribe un registro en cada login (éxito/fallo), marcaje rechazado y acceso denegado por rol. El
Frontend agrega una nueva Area `Auditoria` de solo lectura, accesible únicamente para el rol
Administrador, con filtro por fecha y por usuario, que consume `GET /api/auditoria` (paginado).

**Rationale**: Reutiliza la infraestructura de datos ya elegida (SQLite vía EF Core, fuera de
alcance cambiar de motor) en vez de introducir un sistema de logging externo. La pantalla nueva
resuelve la aclaración obtenida durante esta planificación y cumple el Principio IV de la
constitución (verificable sin herramientas técnicas).

**Alternatives considered**:
- Solo archivo de log estructurado (`ILogger` + Serilog a archivo): más simple de implementar, pero
  el usuario decidió explícitamente que se requiere una pantalla en la app; además un archivo de log
  no es "consultable" por un Administrador sin acceso al servidor.

## 6. Reintento de marcaje ante fallas de red

**Decision**: El cliente HTTP de Marcaje en el Frontend hace un único intento (timeout corto, sin
política de reintento de Polly ni similar). Ante timeout o error de red, el Controller de Marcaje
muestra de inmediato un mensaje de error y no reintenta por su cuenta.

**Rationale**: Ya decidido en `spec.md` (FR-012): un reintento automático podría duplicar una marca
si la primera solicitud sí llegó a completarse en el Backend pero la respuesta se perdió, dado que
"registrar marca" no es una operación garantizada como idempotente en el diseño actual.

**Alternatives considered**: Reintento automático con clave de idempotencia (`Idempotency-Key`) que
el Backend deduplique: técnicamente más robusto, pero excede el alcance actual de la spec
(Principio III, Cero Alcance Fantasma); queda como mejora futura documentable por separado.

## 7. Migración de datos existentes

**Decision**: Las migraciones EF Core existentes (`Infrastructure/Data/Migrations/*`) se trasladan
tal cual a `TimeClockSystem.Infrastructure`; el archivo SQLite existente se reutiliza sin cambios de
esquema salvo la migración aditiva que crea la tabla `RegistrosAuditoria`. No se reescribe ni se
elimina ninguna migración existente.

**Rationale**: Cumple FR-010 (cero pérdida de datos históricos) de la forma más simple: mover el
mismo `ApplicationDbContext` y sus migraciones ya probadas, en vez de regenerarlas.

**Alternatives considered**: Recrear el esquema desde cero en el nuevo proyecto: se descarta por
riesgo de pérdida/discrepancia de datos frente a la base SQLite ya en uso.

## 8. Proyectos de pruebas

**Decision**: Dos proyectos de prueba: `TimeClockSystem.Api.Tests` (pruebas unitarias de
Domain/Application + pruebas de integración de la API vía `WebApplicationFactory`) y
`TimeClockSystem.Web.Tests` (pruebas de integración del Frontend, renombrado desde el actual
`TimeClockSystem.Tests`).

**Rationale**: Mantiene la relación 1:1 "un proyecto de pruebas por aplicación desplegable" ya usada
hoy, sin fragmentar en más proyectos de los necesarios (Principio I).

**Alternatives considered**: Un tercer proyecto `TimeClockSystem.Domain.Tests` separado de
`Api.Tests`: se descarta por simplicidad, dado el tamaño actual del dominio; puede dividirse más
adelante si el proyecto crece.
