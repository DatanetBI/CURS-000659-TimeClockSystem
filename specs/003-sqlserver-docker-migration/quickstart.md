# Quickstart: Validar la Migración a SQL Server y la Contenerización con Docker

**Feature**: [spec.md](./spec.md) | **Contracts**: [contracts/environment-variables.md](./contracts/environment-variables.md)

Esta guía valida de extremo a extremo los criterios de aceptación de la spec (User Stories 1–3,
SC-001 a SC-007) usando únicamente Docker y la aplicación en ejecución, sin leer código.

## Prerrequisitos

- Docker Engine + Docker Compose v2 instalados y corriendo (`docker compose version`).
- Repositorio clonado, rama `003-sqlserver-docker-migration`.
- Ningún proceso local usando ya los puertos que expondrá Compose (Backend/Frontend/SQL Server).

## 1. Configurar secretos locales (una sola vez)

```bash
cp .env.example .env
# Editar .env y completar MSSQL_SA_PASSWORD y JWT_SIGNING_KEY con valores propios
```

**Validación**: `.env` existe localmente y **no aparece** en `git status` (debe estar en
`.gitignore`) — confirma SC-005 (0 secretos en archivos versionados).

## 2. Levantar el sistema completo con un solo comando (User Story 1)

```bash
docker compose up --build
```

**Validación esperada** (SC-001, SC-006):
- Los tres servicios (`sqlserver`, `api`, `web`) aparecen en los logs y llegan a estado
  "running"/"healthy" en menos de 10 minutos en una máquina limpia.
- En los logs de `api`, se observan los reintentos de conexión (si `sqlserver` aún no estaba
  lista) seguidos de la aplicación exitosa de migraciones y la siembra de datos (`DbSeeder`), sin
  ningún paso manual adicional.
- El Frontend (`web`) queda accesible desde el navegador del host (puerto publicado en
  `docker-compose.yml`) y sus llamadas al Backend funcionan correctamente (confirma FR-008: el
  Frontend le habla a `api` por nombre de servicio, no por `localhost`).

## 3. Confirmar continuidad de comportamiento (User Story 2)

Con el sistema arriba:

1. Abrir Swagger del Backend (`http://localhost:<puerto-api>/swagger`) y ejecutar `POST
   /api/auth/login` con un usuario mock sembrado por `DbSeeder`.
2. Desde el Frontend, iniciar sesión, registrar una marca de asistencia, y consultar el listado de
   asistencias.
3. Repetir un flujo de administración (crear/editar un Turno o Centro de Trabajo).

**Validación esperada** (SC-002, SC-007): cada acción produce el mismo resultado que en el sistema
sin contenedorizar sobre SQLite, y las acciones típicas (marcar asistencia, guardar un cambio) se
sienten instantáneas (por debajo de 5 segundos).

## 4. Confirmar persistencia entre reinicios (User Story 3)

```bash
docker compose restart sqlserver
```

**Validación esperada** (SC-003): tras el reinicio, los datos registrados en el paso 3 (marca,
turno editado) siguen presentes al volver a consultarlos desde el Frontend o Swagger — no se
perdieron ni se resembraron.

```bash
docker compose down        # detiene y elimina los contenedores, PERO conserva el volumen mssql-data
docker compose up --build  # se levanta de nuevo
```

**Validación esperada**: los datos siguen presentes (el volumen nombrado sobrevivió a
`docker compose down`). Solo `docker compose down -v` (que elimina volúmenes) debe provocar pérdida
de datos — comportamiento aceptado explícitamente como resultado de una acción explícita.

## 5. Confirmar manejo de secretos (SC-005)

```bash
docker compose config | grep -i "SA_PASSWORD\|SigningKey"
docker history <imagen-api> --no-trunc | grep -i "password\|signingkey"
```

**Validación esperada**: los valores reales de secretos no aparecen embebidos en el historial de
la imagen construida; solo se observan referencias a variables de entorno.

## 6. Confirmar que `dotnet run` sigue funcionando sin Docker (FR-013)

Con una instancia de SQL Server accesible localmente (puede ser la misma expuesta por
`docker compose` en el puerto del host, o una instalación local):

```bash
dotnet run --project src/TimeClockSystem.Api
dotnet run --project src/TimeClockSystem.Web
```

**Validación esperada**: ambos proyectos arrancan igual que hoy, usando
`appsettings.Development.json` / variables de entorno del host en vez de las de Compose, sin
necesidad de Docker.

## 7. Confirmar fallo explícito ante un secreto faltante (edge case)

```bash
docker compose run --rm -e JWT_SIGNING_KEY= api
```

**Validación esperada**: el contenedor `api` falla al iniciar con un mensaje de error claro (no
arranca "silenciosamente" con una clave insegura embebida).
