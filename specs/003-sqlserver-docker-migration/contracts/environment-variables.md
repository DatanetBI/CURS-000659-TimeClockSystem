# Contrato: Variables de Entorno entre `docker-compose.yml` y los Servicios

**Feature**: [../spec.md](../spec.md) | **Research**: [../research.md](../research.md)

Esta funcionalidad no agrega ni cambia ningún endpoint HTTP existente (los contratos de API de la
funcionalidad 002 siguen vigentes sin cambios). El único "contrato" nuevo que introduce es la
interfaz de configuración entre `docker-compose.yml`, el archivo `.env` (no versionado) y cada
contenedor: qué variables espera cada servicio, cuáles son secretas, y de dónde las toma.

## Variables provistas por la persona que levanta el ambiente (`.env`, no versionado)

| Variable | Requerida | Descripción | Ejemplo (solo en `.env.example`, nunca un valor real) |
|---|---|---|---|
| `MSSQL_SA_PASSWORD` | Sí | Contraseña de la cuenta `sa` de SQL Server. MUST cumplir la política de complejidad de contraseñas de SQL Server (mínimo 8 caracteres, 3 de 4 categorías). | `ChangeMe_2026!` |
| `JWT_SIGNING_KEY` | Sí | Clave simétrica usada para firmar y validar los tokens JWT emitidos por el Backend. | `replace-with-a-long-random-value` |

Si alguna de estas variables falta al ejecutar `docker compose up`, el servicio correspondiente
MUST fallar de forma explícita y explicable al arrancar (edge case de la spec), nunca arrancar con
un valor por defecto embebido en la imagen.

## Variables definidas por `docker-compose.yml` hacia cada servicio

### Servicio `sqlserver`

| Variable | Origen | Valor |
|---|---|---|
| `ACCEPT_EULA` | Fijo en compose | `Y` |
| `MSSQL_PID` | Fijo en compose | `Developer` |
| `MSSQL_SA_PASSWORD` | Reenviada desde `.env` | `${MSSQL_SA_PASSWORD}` |

### Servicio `api` (TimeClockSystem.Api)

| Variable | Origen | Valor |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Compuesta en compose a partir de `.env` | `Server=sqlserver,1433;Database=TimeClockSystem;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True` |
| `Jwt__SigningKey` | Reenviada desde `.env` | `${JWT_SIGNING_KEY}` |
| `ASPNETCORE_ENVIRONMENT` | Fijo en compose | `Development` (entorno objetivo: desarrollo/demo local, FR-012) |

### Servicio `web` (TimeClockSystem.Web)

| Variable | Origen | Valor |
|---|---|---|
| `Api__BaseUrl` | Fijo en compose | `http://api:8080/` (nombre del servicio Backend dentro de la red de Compose, FR-008) |
| `ASPNETCORE_ENVIRONMENT` | Fijo en compose | `Development` |

## Contrato de disponibilidad entre servicios

- `api` MUST esperar a que `sqlserver` reporte estar saludable (`depends_on: sqlserver: condition:
  service_healthy` en `docker-compose.yml`) antes de que Compose lo inicie.
- Independientemente del punto anterior, `api` MUST reintentar su propia conexión a la base de
  datos dentro de una ventana acotada antes de aplicar migraciones (ver
  [research.md #4](../research.md#4-espera-acotada-del-backend-a-que-la-base-de-datos-esté-lista)),
  para cubrir reinicios aislados del contenedor `api` sin pasar de nuevo por `depends_on`.
- `web` no tiene una dependencia de arranque estricta sobre `api` (Compose no bloquea su inicio),
  ya que el Frontend ya maneja mostrar un error claro cuando el Backend no está disponible
  (comportamiento existente de la funcionalidad 002).
