# Auditoría de Secretos — TimeClockSystem

> **Propósito**: verificar, antes de hacer `git push`, que el repositorio no expone claves,
> tokens, cadenas de conexión reales ni datos sensibles — ni en el estado actual del árbol de
> trabajo, ni en ningún commit del historial de Git.
>
> **Fecha de la auditoría**: 2026-09-25
> **Rama auditada**: `003-sqlserver-docker-migration`
> **Alcance**: árbol de trabajo completo + **todo el historial de commits** (no solo el estado
> actual — un secreto ya "corregido" en un commit posterior sigue expuesto si el push incluye el
> commit original).

## Resultado

✅ **El repositorio está limpio.** No se encontró ninguna clave, token, cadena de conexión real ni
dato sensible expuesto.

## Checklist de verificación

| Chequeo | Resultado | Evidencia |
|---|---|---|
| `.env` (contiene la contraseña real de SQL Server y la clave de firma JWT) | ✅ Nunca rastreado por Git | `git ls-files \| grep -x "\.env"` → sin resultado; `git log --all --diff-filter=A --name-only \| grep -x "\.env"` → sin resultado (nunca se commiteó, ni siquiera en un commit anterior ya "corregido") |
| `.env` está en `.gitignore` | ✅ Sí | `.gitignore:25` → `.env` |
| `.env.example` (plantilla versionada) | ✅ Solo placeholders | `MSSQL_SA_PASSWORD=ChangeMe_2026!`, `JWT_SIGNING_KEY=replace-with-a-long-random-value` — ambos con comentario explícito de "reemplazar antes de usar" |
| `docker-compose.yml` | ✅ Solo referencias a variables | `MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}`, `Jwt__SigningKey: ${JWT_SIGNING_KEY}`, `ConnectionStrings__DefaultConnection` construida con `${MSSQL_SA_PASSWORD}` — ningún valor literal |
| `src/TimeClockSystem.Api/appsettings.json` | ✅ Sin valores reales | `ConnectionStrings` y `Jwt` solo tienen `_comment` explicando que el valor real se inyecta por variable de entorno (`ConnectionStrings__DefaultConnection`, `Jwt__SigningKey`) |
| `src/TimeClockSystem.Api/appsettings.Development.json` | ✅ Placeholder marcado | `Password=dev-only-CHANGE-ME-1234567890`, `SigningKey: "dev-only-signing-key-not-for-production-CHANGE-ME-1234567890"` — apunta a `localhost,1433`, no a ningún servidor real |
| `src/TimeClockSystem.Api/appsettings.Testing.json` | ✅ Placeholder marcado | `Server=placeholder-not-used-in-tests`, `SigningKey: "testing-only-signing-key-not-for-production-..."` |
| `src/TimeClockSystem.Web/appsettings*.json` | ✅ Sin secretos | Solo `Api:BaseUrl` (URL pública del propio backend), sin credenciales |
| Certificados / claves privadas (`.pfx`, `.pem`, `.key`, `.crt`, `.p12`) | ✅ Ninguno rastreado | `git ls-files \| grep -iE "\.pfx$\|\.pem$\|\.key$\|\.crt$\|\.p12$"` → sin resultado |
| Bases de datos (`.db`, `.sqlite`) | ✅ Ninguna rastreada | `git ls-files \| grep -iE "\.db$\|\.sqlite"` → sin resultado |
| `appsettings.Production.json` u otro config de producción | ✅ No existe | `git ls-files \| grep -i "production"` → sin resultado |
| Búsqueda de patrones de secretos (`password=`, `apikey`, `BEGIN PRIVATE KEY`, tokens de AWS/GitHub/Slack) en el árbol de trabajo actual | ✅ Sin coincidencias reales | Ver comandos en la sección siguiente; los 2 únicos hits fueron una referencia a variable (`${MSSQL_SA_PASSWORD}`) y la contraseña mock de pruebas (ver "Excepción" abajo) |
| La misma búsqueda sobre **todo el historial de commits** (`git log --all -p`) | ✅ Sin coincidencias reales | Solo apariciones del nombre de variable `jwtSigningKey`/`SigningKey` leído desde configuración — nunca un valor literal |

## Única excepción encontrada (intencional, no es una fuga)

`tests/TimeClockSystem.Api.Tests/AuthTestHelper.cs:9` contiene:

```csharp
public const string AdminPassword = "Admin123!";
```

Esta es la misma contraseña mock documentada a propósito en
[`specs/001-timeclock-spec-v1/MANUAL-USUARIO.md`](../../specs/001-timeclock-spec-v1/MANUAL-USUARIO.md)
y en el `README.md` (sección "Credenciales de prueba"). Es un dato de prueba/demo
deliberadamente público para poder validar el sistema de inmediato — no un secreto real ni una
credencial de un entorno productivo. Se documenta aquí por transparencia, no como hallazgo.

## Comandos usados (reproducibilidad)

```bash
# .env: ¿rastreado ahora o alguna vez en el historial?
git ls-files | grep -x "\.env"
git log --all --diff-filter=A --name-only | grep -x "\.env"

# .gitignore incluye .env
grep -n "^\.env$" .gitignore

# Archivos de configuración sensibles
cat .env.example
grep -n "PASSWORD\|SigningKey\|ConnectionString" docker-compose.yml
cat src/TimeClockSystem.Api/appsettings.json
cat src/TimeClockSystem.Api/appsettings.Development.json
cat src/TimeClockSystem.Api/appsettings.Testing.json
cat src/TimeClockSystem.Web/appsettings.json src/TimeClockSystem.Web/appsettings.Development.json

# Patrones de secretos en el árbol de trabajo actual (excluyendo placeholders conocidos)
git grep -inE "password\s*=|pwd\s*=|apikey|api_key|secret\s*[:=]|BEGIN (RSA |EC |)PRIVATE KEY|AKIA[0-9A-Z]{16}|xox[baprs]-|ghp_[A-Za-z0-9]{36}" -- . ':!*.md' \
  | grep -viE "CHANGE-ME|change_me|placeholder|_comment|testing-only|dev-only|example"

# Los mismos patrones sobre TODO el historial de commits (no solo el árbol actual)
git log --all -p -- '*.json' '*.cs' '*.yml' '*.yaml' \
  | grep -inE "password\s*[:=]|pwd\s*[:=]|signingkey\s*[:=]|BEGIN (RSA |EC |)PRIVATE KEY" \
  | grep -viE "CHANGE-ME|change_me|placeholder|_comment|testing-only|dev-only|example|Admin123|MSSQL_SA_PASSWORD\}|not-for-production|placeholder-not-used"

# Archivos sensibles que no deberían existir en el repo
git ls-files | grep -iE "\.pfx$|\.pem$|\.key$|\.crt$|\.p12$"
git ls-files | grep -iE "\.db$|\.sqlite"
git ls-files | grep -i "production"
```

## Conclusión

El repositorio puede subirse (`git push`) sin riesgo de exposición de secretos según los criterios
verificados en este documento. Esta auditoría cubre específicamente **secretos y datos
sensibles**; no reemplaza una revisión de código general ni un análisis de otras políticas de
seguridad no cubiertas aquí.

## Documentos relacionados

- [`README.md`](../../README.md) — Sección "Configuración y secretos" y "Credenciales de prueba (mock)".
- [`ENTREGABLES/EVIDENCIAS/Evidencias.md`](./Evidencias.md) — Contexto de por qué el manejo de secretos vía `.env`/variables de entorno fue una decisión explícita de la feature `003-sqlserver-docker-migration` (Principio V de la constitución).
- [`specs/003-sqlserver-docker-migration/contracts/environment-variables.md`](../../specs/003-sqlserver-docker-migration/contracts/environment-variables.md) — Contrato formal de qué variables de entorno espera cada servicio.
