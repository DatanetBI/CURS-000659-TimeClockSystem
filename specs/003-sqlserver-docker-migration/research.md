# Research: Migración a SQL Server y Contenerización con Docker

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

Este documento resuelve las decisiones técnicas necesarias para pasar de Technical Context a
diseño (Phase 1). No hay marcadores `NEEDS CLARIFICATION` pendientes en la spec ni en el Technical
Context del plan; las decisiones aquí documentadas son de implementación (cómo), no de alcance.

## 1. Proveedor EF Core y estrategia de migraciones

**Decision**: Reemplazar `Microsoft.EntityFrameworkCore.Sqlite` por
`Microsoft.EntityFrameworkCore.SqlServer` (misma versión `10.0.12` que el resto de paquetes EF
Core del proyecto) en `TimeClockSystem.Infrastructure.csproj`, cambiar `options.UseSqlite(...)` por
`options.UseSqlServer(...)` en `Program.cs`, **eliminar** la carpeta
`Data/Migrations/` existente (incluyendo `ApplicationDbContextModelSnapshot.cs`) y generar una
migración inicial única (`dotnet ef migrations add InitialSqlServer`) contra el proveedor SQL
Server.

**Rationale**: Las migraciones actuales fueron generadas contra SQLite y codifican decisiones
específicas de ese proveedor (tipos de columna, autoincremento, ausencia de varios tipos nativos
de SQL Server); no aplican directamente contra SQL Server (confirmado en el enunciado de la
funcionalidad). Dado que la Clarification ya resolvió que **no se migran datos existentes** (la
base de datos SQL Server inicia vacía y se resiembra con `DbSeeder`), no hay necesidad de preservar
el historial incremental de migraciones: una sola migración inicial que recree el esquema completo
es la opción más simple (Principio I) y evita arrastrar particularidades de SQLite.

**Alternatives considered**:
- *Reescribir cada migración existente manualmente para SQL Server* — descartado: mucho más
  trabajo y riesgo de errores que regenerar una migración inicial limpia, sin beneficio adicional
  dado que no hay datos que preservar.
- *Mantener ambos proveedores configurables (Sqlite y SqlServer) vía flag* — descartado: añade
  complejidad y superficie de prueba no solicitada por la spec (Principio III, Cero Alcance
  Fantasma); la spec pide reemplazar el motor, no soportar ambos.

## 2. Imagen y edición de SQL Server para el contenedor

**Decision**: `mcr.microsoft.com/mssql/server:2022-latest` (imagen oficial de Microsoft), con
`MSSQL_PID=Developer` (edición Developer: gratuita, funcionalmente equivalente a Enterprise, solo
restringida para uso no productivo) y `ACCEPT_EULA=Y`.

**Rationale**: La Clarification de la spec fija el entorno objetivo como exclusivamente
desarrollo/demo local (FR-012); la edición Developer es la recomendada por Microsoft para ese
escenario, no tiene costo de licencia, y es la misma imagen usada típicamente en ejemplos oficiales
de contenerización de SQL Server.

**Alternatives considered**:
- *SQL Server Express* — descartado: límites de tamaño de base de datos (10 GB) y de recursos
  (memoria/CPU) innecesarios para este caso de uso, sin ninguna ventaja sobre Developer en un
  entorno local.
- *Edición Standard/Enterprise* — descartado: requiere licencia, orientada a producción; fuera de
  alcance según la Clarification del entorno objetivo.

## 3. Orquestación de los tres contenedores con un solo comando

**Decision**: Un archivo `docker-compose.yml` en la raíz del repositorio con tres servicios:
`sqlserver`, `api`, `web`, una red por defecto de Compose compartida entre ellos, y el comando
único `docker compose up --build` (documentado en `quickstart.md`).

**Rationale**: Docker Compose es la herramienta estándar para "un solo comando levanta varios
contenedores relacionados" en un entorno de desarrollo local, ya viene incluida con Docker
Desktop, y no introduce infraestructura adicional (Principio I). No se requiere Kubernetes ni
ninguna plataforma de nube (explícitamente fuera de alcance).

**Alternatives considered**:
- *Script propio (bash/PowerShell) que ejecute `docker run` tres veces* — descartado: reimplementa
  lo que Compose ya resuelve de forma declarativa y estándar (redes, dependencias, volúmenes),
  añadiendo mantenimiento sin beneficio.
- *Kubernetes (minikube/kind) local* — descartado: fuera de alcance explícito de la spec y
  claramente sobre-ingeniería para un entorno de un solo desarrollador (Principio I).

## 4. Espera acotada del Backend a que la base de datos esté lista

**Decision**: Dos mecanismos complementarios:
1. **Healthcheck de Compose** en el servicio `sqlserver` (usando `sqlcmd` o el healthcheck nativo
   de la imagen para verificar que acepta conexiones) y `depends_on: sqlserver: condition:
   service_healthy` en el servicio `api`, para que Compose no arranque el contenedor de `api`
   hasta que SQL Server esté realmente listo.
2. **Reintentos acotados dentro del propio Backend**, alrededor de la llamada a
   `db.Database.MigrateAsync()` en `Program.cs`: un bucle con un número máximo de intentos y una
   espera entre cada uno (p. ej. hasta ~2 minutos en total), que registra cada intento fallido y,
   si se agota la ventana, deja que la excepción original se propague (el proceso termina con un
   mensaje de error claro en los logs del contenedor, satisfaciendo FR-007 y el edge case
   correspondiente).

**Rationale**: El healthcheck de Compose cubre el caso común (arranque en frío de todo el
sistema) sin lógica adicional; el reintento acotado dentro del Backend cubre el caso en que el
Backend se reinicia solo (p. ej. `docker compose restart api`) mientras `sqlserver` aún está
recuperándose, sin depender de que Compose vuelva a evaluar `depends_on`. Ambos mecanismos son
simples (un healthcheck declarativo + un bucle de reintento estándar), no requieren librerías
externas de resiliencia.

**Alternatives considered**:
- *Solo `depends_on` de Compose, sin reintento en el Backend* — descartado: no cubre el caso de
  reinicio aislado del contenedor `api`, dejando una ventana en la que el Backend fallaría de
  inmediato en vez de esperar brevemente.
- *Librería de resiliencia de terceros (p. ej. Polly)* — descartado por ahora: un bucle de
  reintento simple con espera fija es suficiente para este caso acotado (Principio I); se puede
  reconsiderar si surge una necesidad más amplia de resiliencia fuera del alcance de esta
  funcionalidad.

## 5. Manejo de fallo de migración al iniciar

**Decision**: No agregar manejo especial de excepciones alrededor de la aplicación de la
migración en sí (más allá del reintento de conexión del punto 4): si `MigrateAsync()` falla por un
motivo distinto a "base de datos no lista" (p. ej. migración incompatible), la excepción se deja
propagar sin capturarla, de modo que el proceso ASP.NET Core termina antes de llamar a `app.Run()`
y el contenedor se detiene, quedando el stack trace completo en los logs (`docker compose logs
api`).

**Rationale**: Esto ya es el comportamiento por defecto de .NET ante una excepción no controlada
durante el arranque, y cumple exactamente lo pedido en la Clarification (detener el contenedor,
no quedar escuchando peticiones, error claro en logs) sin necesidad de código adicional de
manejo de errores (Principio I: no agregar manejo de errores para escenarios ya cubiertos por el
comportamiento estándar de la plataforma).

**Alternatives considered**:
- *Capturar la excepción y escribir un mensaje "amigable" antes de salir* — descartado: el stack
  trace estándar de .NET ya es suficientemente claro para el público técnico que revisa logs de
  contenedor (a diferencia de un mensaje de error de UI dirigido a un usuario final no técnico);
  agregar una capa de manejo de errores aquí sería complejidad sin beneficio medible.

## 6. Dockerfiles de Backend y Frontend

**Decision**: Un `Dockerfile` multi-stage por proyecto (`src/TimeClockSystem.Api/Dockerfile` y
`src/TimeClockSystem.Web/Dockerfile`), usando `mcr.microsoft.com/dotnet/sdk:10.0` como imagen de
build y `mcr.microsoft.com/dotnet/aspnet:10.0` como imagen final de ejecución, siguiendo el patrón
estándar de Microsoft para contenerizar aplicaciones ASP.NET Core (restore + build + publish en la
etapa SDK, copia del resultado publicado a la etapa runtime).

**Rationale**: Es el patrón oficial y ampliamente documentado para .NET; produce imágenes finales
más pequeñas (sin el SDK completo) y es directamente reconocible por cualquier desarrollador .NET,
sin necesidad de herramientas de build adicionales.

**Alternatives considered**:
- *Imagen única `sdk` también para ejecución* — descartado: imagen final innecesariamente grande,
  sin ninguna ventaja para este caso de uso.

## 7. Comunicación Frontend → Backend dentro de la red de contenedores

**Decision**: En `docker-compose.yml`, el servicio `web` recibe `Api__BaseUrl=http://api:8080/`
como variable de entorno (usando el nombre del servicio `api` como host, resuelto por la red
interna de Docker Compose), sobrescribiendo el valor de `Api:BaseUrl` en `appsettings.json` que
hoy apunta a `http://localhost:5073/`. El puerto interno `8080` es el puerto por defecto en el que
escucha la imagen base `aspnet:10.0` dentro del contenedor (vía `ASPNETCORE_URLS`).

**Rationale**: El Frontend ya lee `Api:BaseUrl` desde configuración (confirmado en
`appsettings.json`, con un comentario que documenta explícitamente que se puede sobrescribir con
la variable de entorno `Api__BaseUrl`); no se requiere ningún cambio de código en el Frontend,
solo la variable de entorno correcta en Compose, cumpliendo FR-008 sin tocar reglas de negocio ni
pantallas (Principio III).

**Alternatives considered**: Ninguna considerada — la configuración existente ya está diseñada
para este caso (jerarquía de configuración estándar de ASP.NET Core con variables de entorno).

## 8. Manejo de secretos (contraseña SQL Server, clave de firma JWT)

**Decision**: Variables de entorno leídas por Docker Compose desde un archivo `.env` en la raíz
del repositorio (mecanismo nativo de `docker compose`, que carga automáticamente `.env` si existe),
agregado a `.gitignore`. Se versiona un `.env.example` con las claves requeridas y valores de
marcador de posición (nunca secretos reales), para que cualquier persona pueda copiarlo a `.env` y
completarlo antes de levantar el sistema. Las claves mínimas: `MSSQL_SA_PASSWORD`,
`JWT_SIGNING_KEY`. `docker-compose.yml` las referencia como `${MSSQL_SA_PASSWORD}` /
`${JWT_SIGNING_KEY}` y las inyecta a cada contenedor como `MSSQL_SA_PASSWORD`,
`ConnectionStrings__DefaultConnection` (construida con la contraseña) y `Jwt__SigningKey`
respectivamente.

**Rationale**: Es el mecanismo estándar y más simple soportado nativamente por Docker Compose para
inyectar secretos en desarrollo local (Principio I), cumple directamente FR-009/SC-005 (ningún
secreto en texto plano en imágenes ni archivos versionados), y es consistente con el patrón que
ya usa el proyecto para `Jwt:SigningKey` en `appsettings.json` (comentario explícito: "la clave de
firma real NUNCA debe vivir en este archivo en producción").

**Alternatives considered**:
- *Docker secrets (modo Swarm)* — descartado: requiere inicializar Docker en modo Swarm,
  complejidad innecesaria para un entorno de desarrollador único con `docker compose up`
  (Principio I).
- *Gestor de secretos externo (Vault, etc.)* — descartado: muy por encima de las necesidades de
  un entorno local/demo; fuera de alcance implícito.

## 9. Infraestructura de pruebas automatizadas (xUnit)

**Decision**: No modificar `CustomWebApiFactory` ni la infraestructura de pruebas existente; las
pruebas de integración del Backend siguen usando SQLite en un archivo temporal por ejecución.

**Rationale**: El proveedor de base de datos de producción es una preocupación ortogonal al
aislamiento de pruebas; la spec no pide cambiar cómo se prueban los módulos existentes, y hacerlo
(p. ej. levantar un contenedor SQL Server real para cada corrida de pruebas) añadiría tiempo y
complejidad no solicitados (Principio I, Principio III).

**Alternatives considered**:
- *Ejecutar las pruebas de integración contra un contenedor SQL Server real (Testcontainers)* —
  descartado por ahora: fuera del alcance declarado en la spec, y las pruebas actuales ya cubren
  el comportamiento de los casos de uso independientemente del proveedor de datos subyacente.
