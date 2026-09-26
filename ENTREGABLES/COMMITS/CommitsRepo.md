# Historial de Commits — TimeClockSystem

> **Entregable**: `CommitsRepo.md` — Historial completo de commits del repositorio.
> Generado directamente desde `git log` (no reconstruido ni resumido de memoria); cada dato de
> este documento es verificable ejecutando los mismos comandos citados en la sección 5.

## 1. Resumen general del repositorio

| Dato | Valor |
|---|---|
| Total de commits | 23 |
| Autor(es) | `DatanetBI <datanetbi@gmail.conm>` (único autor en todo el historial) |
| Primer commit | `115abb5` — 2026-09-22 19:08:14 -0600 |
| Último commit | `a603fa6` — 2026-09-25 00:40:50 -0600 |
| Ramas | `main`, `001-timeclock-spec-v1`, `002-split-backend-frontend`, `003-sqlserver-docker-migration` (actual) |
| Tags | Ninguno |
| Historial | **Completamente lineal** — no hay merges ni divergencias; cada rama es simplemente un puntero a un commit distinto de la misma cadena única de 23 commits (ver §2) |
| Archivos rastreados actualmente (`HEAD`) | 292 |
| Líneas insertadas (acumulado, todo el historial) | 28 624 |
| Líneas eliminadas (acumulado, todo el historial) | 5 158 |
| Cambios entre `main` y `HEAD` actual | 214 archivos, +11 876 líneas |

## 2. Ramas y su commit más reciente

Como el historial es lineal, cada rama es un punto de parada distinto sobre la misma secuencia de
commits — no hay líneas paralelas que se hayan bifurcado y vuelto a unir.

| Rama | Commit más reciente (tip) | Fecha | Mensaje |
|---|---|---|---|
| `main` | `e3073d5` | 2026-09-22 19:18 | `[Spec Kit] Add project constitution` |
| `001-timeclock-spec-v1` | `1bf93bd` | 2026-09-23 16:58 | `docs: add v1.0 user manual with mock credentials per role` |
| `002-split-backend-frontend` | `5ca7d2d` | 2026-09-24 23:14 | `[Spec Kit] Implementation progress` |
| `003-sqlserver-docker-migration` (actual, `HEAD`) | `a603fa6` | 2026-09-25 00:40 | `[Spec Kit] Implementation progress` |

## 3. Historial completo, commit por commit (orden cronológico)

| # | Hash corto | Hash completo | Fecha y hora | Mensaje | Archivos | Inserciones | Eliminaciones |
|---|---|---|---|---|---|---|---|
| 1 | `115abb5` | `115abb5d29202817af1008a0243098e2e93a1c3c` | 2026-09-22 19:08:14 -0600 | `chore: inicializar repositorio para speckit` | 78 | +11 533 | — |
| 2 | `e3073d5` | `e3073d571210d46277fb0f57b6d3ee3b39e23d5c` | 2026-09-22 19:18:11 -0600 | `[Spec Kit] Add project constitution` | 1 | +92 | -35 |
| 3 | `4342e0d` | `4342e0d7509599298b9f4d2129912d8d5671a16e` | 2026-09-22 19:56:32 -0600 | `[Spec Kit] Add specification` | 2 | +556 | — |
| 4 | `7aa389b` | `7aa389baa1ccfccb79ac6505c25d843281eebe87` | 2026-09-22 20:08:46 -0600 | `[Spec Kit] Clarify specification` | 2 | +53 | -21 |
| 5 | `c84ffb7` | `c84ffb72833eeb1feb556d2b18b386b11f846673` | 2026-09-22 21:05:15 -0600 | `[Spec Kit] Add implementation plan` | 6 | +778 | — |
| 6 | `7c4edbc` | `7c4edbc7f483f29a595e81887191b4f91033ca93` | 2026-09-22 21:40:42 -0600 | `[Spec Kit] Add implementation plan` | 6 | +429 | -546 |
| 7 | `2271fe3` | `2271fe308068ecb3c87352d45f03d4689ad9993f` | 2026-09-22 21:47:47 -0600 | `[Spec Kit] Add tasks` | 1 | +303 | — |
| 8 | `ab7d9c6` | `ab7d9c6b1ad72a0f2a2e39bfd2ccea087e927ed1` | 2026-09-22 22:02:59 -0600 | `[Spec Kit] Add analysis report` | 1 | +64 | — |
| 9 | `99005f1` | `99005f1c3c3c9bb33f819cd3a29ff676ac9cb5d9` | 2026-09-22 22:14:28 -0600 | `fix: remediate analysis findings from Specification-Analysis-Report-RC1` (ver cuerpo completo en §4) | 3 | +103 | -87 |
| 10 | `60356c2` | `60356c24d71b04080cec7950125c988f4abac937` | 2026-09-23 05:51:09 -0600 | `[Spec Kit] Implementation progress` | 106 | +7 058 | -69 |
| 11 | `1bf93bd` | `1bf93bd31d96573ad11a7b5586a71abe4c6a0bea` | 2026-09-23 16:58:18 -0600 | `docs: add v1.0 user manual with mock credentials per role` (ver cuerpo completo en §4) | 1 | +146 | — |
| 12 | `ceca399` | `ceca39913c433573f82f7525f1154c44171b0d48` | 2026-09-24 21:17:57 -0600 | `[Spec Kit] Clarify specification` | 2 | +160 | — |
| 13 | `4a7bc07` | `4a7bc076e65871a4af9c54570ce953f7be3664b2` | 2026-09-24 21:38:39 -0600 | `[Spec Kit] Add implementation plan` | 13 | +692 | — |
| 14 | `e09f1ac` | `e09f1ac7613c583fef6380abd905756443b26ec2` | 2026-09-24 21:43:37 -0600 | `[Spec Kit] Add tasks` | 1 | +243 | — |
| 15 | `29de522` | `29de522ed010c7933974e81904bed7dc01126135` | 2026-09-24 21:54:01 -0600 | `[Spec Kit] Add analysis report` | 1 | +77 | — |
| 16 | `608ae21` | `608ae21d98514f3c6892e6c7d64ffc9b24320459` | 2026-09-24 21:56:29 -0600 | `fix: remediate analysis findings from Specification-Analysis-Report-BackEnd` (ver cuerpo completo en §4) | 6 | +43 | -7 |
| 17 | `5ca7d2d` | `5ca7d2d8fb46aed45816fd7d3fe1b3619bdb4062` | 2026-09-24 23:14:31 -0600 | `[Spec Kit] Implementation progress` | 156 | +4 354 | -1 233 |
| 18 | `825f7db` | `825f7dbd132a0dda20786f317ead927b7be8fa4f` | 2026-09-24 23:32:48 -0600 | `[Spec Kit] Add specification` | 2 | +147 | — |
| 19 | `e44b4fa` | `e44b4fab012ff82072958a96d75149266ef9024a` | 2026-09-24 23:38:37 -0600 | `[Spec Kit] Clarify specification` | 2 | +10 | -4 |
| 20 | `01243c0` | `01243c03271187567ea5b5549dd5b3b26a1f7023` | 2026-09-24 23:45:55 -0600 | `[Spec Kit] Add implementation plan` | 5 | +545 | — |
| 21 | `345a291` | `345a291e4005e2146137a52befafc0863d60ca25` | 2026-09-24 23:50:30 -0600 | `[Spec Kit] Add tasks` | 1 | +191 | — |
| 22 | `65549e5` | `65549e589831726b54cdecd1fb9c17d7786349c7` | 2026-09-24 23:58:55 -0600 | `[Spec Kit] Save progress before implementation` | 1 | +77 | — |
| 23 | `a603fa6` | `a603fa6dd95135fb13fee67628cc660ac90a2e94` | 2026-09-25 00:40:50 -0600 | `[Spec Kit] Implementation progress` | 32 | +970 | -3 156 |

## 4. Cuerpo completo de los mensajes de commit que lo incluyen

La mayoría de los commits de Spec Kit tienen solo una línea de asunto (generada automáticamente
por el hook de auto-commit). Los siguientes 4 commits sí incluyen un cuerpo detallado, redactado
explicando el porqué del cambio — se transcriben íntegros:

### `99005f1` — fix: remediate analysis findings from Specification-Analysis-Report-RC1

```
- Add ConsentimientoGeolocalizacion field to Empleado and a new task
  (T049) to validate it before accepting a geolocated mark (FR-046, CL9)
- Correct plan.md's requirement-to-module mapping: FR-002/FR-003 belong
  to Modulo 5, not Modulo 2; document that FR-014's exclusion logic has
  no real effect in v1.0 (depends on deferred Incidencias)
- Explicitly defer RFID/physical terminal marking channels to v1.1
- Rename tasks.md module labels from [US1]-[US6] to [M1]-[M6] to avoid
  colliding with spec.md's own US1-US7 user story numbering
- Add a load-test task (T069) covering SC-010 (500 active employees)
- Add missing Views/Shared/ folder to plan.md's project structure tree

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
```

### `1bf93bd` — docs: add v1.0 user manual with mock credentials per role

```
Covers both v1.0 roles (Empleado, Administrador), step-by-step usage
of the 6 modules, the mock admin/employee logins and PIN seeded by
DbSeeder, and what is explicitly out of scope until v1.1.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
```

### `608ae21` — fix: remediate analysis findings from Specification-Analysis-Report-BackEnd

```
Resolves I1 (role-staleness edge case vs fixed-expiration JWT), U1
(missing empleadoId claim propagation for Marcaje/ConsultaAsistencias),
A1 (untestable SC-002 wording) and G1 (SC-007 performance criterion had
no task/validation coverage).

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
```

> Nota: `7aa389b`, `4342e0d`, `2271fe3`, `ab7d9c6`, `60356c2`, `ceca399`, `4a7bc07`, `e09f1ac`,
> `29de522`, `5ca7d2d`, `825f7db`, `e44b4fa`, `01243c0`, `345a291`, `65549e5`, `a603fa6`,
> `c84ffb7`, `7c4edbc`, `115abb5`, `e3073d5` no tienen cuerpo de mensaje — son commits generados
> automáticamente por los hooks `speckit.git.commit`/`speckit.git.feature` con solo la línea de
> asunto estándar del comando de Spec Kit ejecutado.

## 5. Agrupación por feature (para navegación rápida)

*Recordatorio: el agrupamiento es solo de lectura — la rama real de cada commit es la que se
muestra en la columna 1 de la tabla; no hay commits "compartidos" entre features salvo donde se
indica explícitamente (commits que cierran una feature y abren la siguiente en la misma sesión).*

| Feature | Commits (en orden) | Rango de fechas |
|---|---|---|
| *(pre-Spec Kit + init)* | `115abb5` | 2026-09-22 19:08 |
| `main` (constitución) | `e3073d5` | 2026-09-22 19:18 |
| `001-timeclock-spec-v1` | `4342e0d`, `7aa389b`, `c84ffb7`, `7c4edbc`, `2271fe3`, `ab7d9c6`, `99005f1`, `60356c2`, `1bf93bd` | 2026-09-22 19:56 → 2026-09-23 16:58 |
| `002-split-backend-frontend` | `ceca399`, `4a7bc07`, `e09f1ac`, `29de522`, `608ae21`, `5ca7d2d` | 2026-09-24 21:17 → 23:14 |
| `003-sqlserver-docker-migration` | `825f7db`, `e44b4fa`, `01243c0`, `345a291`, `65549e5`, `a603fa6` | 2026-09-24 23:32 → 2026-09-25 00:40 |

**Nota sobre `60356c2` y `5ca7d2d`**: ambos commits llevan el mensaje genérico `[Spec Kit]
Implementation progress`. `60356c2` cierra la implementación de `001-timeclock-spec-v1`;
`5ca7d2d` cierra la de `002-split-backend-frontend`. `1bf93bd` (manual de usuario) se creó
**después** del cierre de 001 pero antes de empezar 002 — por eso aparece cronológicamente entre
ambos aunque pertenece a la feature 001.

## 6. Comandos usados para generar este documento (reproducibilidad)

```bash
# Resumen general
git log --all --oneline | wc -l
git log --all --pretty=format:"%an <%ae>" | sort -u
git tag
git ls-files | wc -l
git log --all --pretty=tformat: --numstat | awk '{add+=$1; del+=$2} END {print add, del}'

# Tabla completa (hash, fecha, mensaje)
git log --all --reverse --pretty=format:"%H|%h|%an <%ae>|%ad|%s" --date=format:"%Y-%m-%d %H:%M:%S %z"

# Estadísticas de archivos por commit
git log --all --reverse --pretty=format:"COMMIT %h" --shortstat

# Cuerpo completo de cada mensaje (cuando existe)
git log --all --reverse --pretty=format:"###%H###%h###%an###%ad###%s###%b###ENDCOMMIT###"

# Ramas y su commit más reciente
git branch -a
git log -1 --pretty=format:"%h %s" <rama>

# Diferencia acumulada entre main y la rama actual
git diff --shortstat main HEAD
```

## 7. Documentos relacionados

- [`ENTREGABLES/EVIDENCIAS/Evidencias.md`](../EVIDENCIAS/Evidencias.md) — El prompt real y la decisión de negocio detrás de cada uno de estos commits (secciones 1 a 4).
- `specs/001-timeclock-spec-v1/`, `specs/002-split-backend-frontend/`, `specs/003-sqlserver-docker-migration/` — Artefactos de spec que cada grupo de commits produjo.
