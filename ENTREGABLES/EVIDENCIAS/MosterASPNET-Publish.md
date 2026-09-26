# Publicación en MonsterASP.NET — TimeClockSystem

> **Propósito**: guía verificada para publicar `TimeClockSystem.Api` y `TimeClockSystem.Web` en
> MonsterASP.NET (hosting IIS + SQL Server tradicional — **no es Docker**), sin exponer secretos
> ni modificar el código más allá de lo ya necesario.
>
> **Fecha**: 2026-09-25
> **Fuentes verificadas** (consultadas en vivo antes de escribir esta guía, no asumidas de memoria):
> - [help.monsterasp.net/books/deploy/page/how-to-deploy-net-core-web-application-using-visual-studio](https://help.monsterasp.net/books/deploy/page/how-to-deploy-net-core-web-application-using-visual-studio)
> - [help.monsterasp.net/books/databases/page/create-database](https://help.monsterasp.net/books/databases/page/create-database)
> - [help.monsterasp.net/books/development/page/environment-variables-as-configuration-store](https://help.monsterasp.net/books/development/page/environment-variables-as-configuration-store)
> - [help.monsterasp.net/books/https](https://help.monsterasp.net/books/https)
> - [monsterasp.net](https://monsterasp.net) (planes, versiones de .NET soportadas)

## Diferencia clave con el resto del proyecto

Todo lo documentado hasta ahora (`docker-compose.yml`, `.env`, `specs/003-sqlserver-docker-migration/`)
asume **contenedores Docker**. MonsterASP.NET es hosting **IIS tradicional**: el mismo código
compilado de `TimeClockSystem.Api`/`TimeClockSystem.Web` corre directamente sobre Windows/IIS,
contra una instancia real de SQL Server que el proveedor administra — no hay `Dockerfile` ni
`docker-compose.yml` involucrados en este camino de publicación.

## 0. Prerrequisito — plan de hosting

Esta solución necesita **2 sitios independientes** (Api + Web), cada uno con su propia URL. Revisa
en tu panel qué plan tienes:

| Plan | Sitios / subdominios incluidos | ¿Alcanza? |
|---|---|---|
| Premium Single | 1 sitio + 3 subdominios | Sí, usando 2 de los 3 subdominios |
| Premium Multi | 10 sitios + 10 subdominios | Sí, con margen |

Verifica también que **.NET Core 10** aparezca seleccionable para tu plan al crear un sitio — el
sitio público de MonsterASP.NET publicita soporte hasta ".NET Core 11/10/9/8/7/6/5", pero la
disponibilidad real por plan puede variar y conviene confirmarla en el panel antes de continuar.

## 1. Crear la base de datos SQL Server

Panel → **Databases** → **Add database** → tipo **MSSQL** → completar el asistente.

Al terminar, el panel muestra la cadena de conexión real (servidor, base de datos, usuario,
contraseña). Formato esperado por el proyecto (mismo que usa
`src/TimeClockSystem.Api/appsettings.Development.json` para desarrollo local, pero con valores
reales del proveedor en vez del placeholder `dev-only-CHANGE-ME`):

```
Server=<servidor-del-panel>;Database=<db-del-panel>;User Id=<usuario-del-panel>;Password=<password-del-panel>;TrustServerCertificate=True
```

`TrustServerCertificate=True` se agrega defensivamente por si el certificado del servidor no es
verificable desde el cliente EF Core; quítalo si el proveedor confirma que no hace falta.

## 2. Crear los dos sitios (uno por proyecto)

Panel → **Websites** → **Add website**, dos veces:

| Sitio | Proyecto | Ejemplo de subdominio |
|---|---|---|
| Backend | `TimeClockSystem.Api` | `timeclock-api.tudominio.runasp.net` |
| Frontend | `TimeClockSystem.Web` | `timeclock-app.tudominio.runasp.net` |

Selecciona runtime **.NET Core 10** al crear cada uno.

## 3. Configurar secretos — sin tocar código

MonsterASP.NET usa exactamente la misma convención de ASP.NET Core que ya usa este proyecto para
leer configuración por variable de entorno: `Section__Clave` equivale a `Section:Clave` en
`appsettings.json` (doble guion bajo = anidamiento JSON). Fuente: artículo oficial ["Environment
variables as configuration
store"](https://help.monsterasp.net/books/development/page/environment-variables-as-configuration-store).
**No se requiere ningún cambio de código para esto** — `Program.cs` ya lee
`ConnectionStrings:DefaultConnection` y `Jwt:SigningKey` desde `IConfiguration`, sin importar de
qué proveedor de configuración vengan.

Panel → **Websites → Manage website → Scripting → Environment Variables**:

**En el sitio del Backend:**

| Variable | Valor |
|---|---|
| `ConnectionStrings__DefaultConnection` | La cadena real del paso 1 |
| `Jwt__SigningKey` | Una clave larga y aleatoria propia — **nunca** el valor de ejemplo de `appsettings.Development.json` |

**En el sitio del Frontend:**

| Variable | Valor |
|---|---|
| `Api__BaseUrl` | URL pública del sitio del Backend, ej. `https://timeclock-api.tudominio.runasp.net/` |

> Nota de seguridad: estas variables cumplen el mismo Principio V de la constitución que ya rige
> `.env`/`docker-compose.yml` en el entorno Docker — ningún secreto real vive en un archivo
> versionado del repositorio; aquí simplemente cambia el mecanismo de inyección (panel de
> MonsterASP.NET en vez de `.env`).

## 4. Swagger visible en el sitio publicado

`Program.cs` se actualizó para registrar `UseSwagger()`/`UseSwaggerUI()` en **todo ambiente**, no
solo en `Development` (antes: `if (app.Environment.IsDevelopment()) { ... }`). Esto es intencional
y coherente con FR-006 de `specs/002-split-backend-frontend/spec.md` (la API debe poder probarse
de forma independiente del Frontend) — Swagger expone el contrato HTTP, no secretos ni datos. Por
eso **ya no hace falta** definir `ASPNETCORE_ENVIRONMENT=Development` en el sitio publicado solo
para ver Swagger.

## 5. Publicar desde Visual Studio (uno por proyecto)

1. En el panel de **cada sitio**, activa **WebDeploy** y descarga su archivo `.publishSettings`.
2. En Visual Studio: clic derecho sobre `TimeClockSystem.Api` → **Publish** → **Import Profile** →
   selecciona el `.publishSettings` del sitio del Backend → **Publish**.
3. Repite con `TimeClockSystem.Web` y el `.publishSettings` del sitio del Frontend.

Al primer arranque, el propio `Program.cs` aplica la migración `InitialSqlServer` y siembra los
datos mock automáticamente contra la base de datos real (mismo `DbSeeder` que en Docker) — no se
ejecuta ningún comando de migración manual en el servidor.

## 6. Activar HTTPS

Panel → **HTTPS** → un clic para activar Let's Encrypt (gratis), en **ambos** sitios.

## 7. Verificación

- Backend: `https://<sitio-api>/swagger` — debe cargar sin necesidad de variables adicionales.
- Backend: `POST /api/auth/login` con `admin1` / `Admin123!` (credenciales mock, ver README →
  "Credenciales de prueba (mock)").
- Frontend: abrir la URL del sitio Web, iniciar sesión, marcar asistencia y consultar el
  historial — confirma que el Frontend le habla al Backend por su URL pública real (variable
  `Api__BaseUrl`), no por `localhost` ni por nombre de servicio Docker.

## Documentos relacionados

- [`README.md`](../../README.md) — sección "Publicar en MonsterASP.NET" (resumen operativo de esta guía).
- [`src/TimeClockSystem.Api/Program.cs`](../../src/TimeClockSystem.Api/Program.cs) — punto donde se lee cada variable de entorno citada aquí.
- [`ENTREGABLES/EVIDENCIAS/Auditoria-Secretos.md`](./Auditoria-Secretos.md) — por qué ningún valor real de esta guía debe versionarse.
- [`specs/003-sqlserver-docker-migration/`](../../specs/003-sqlserver-docker-migration/) — el camino de publicación alternativo (Docker), para contrastar con este.
