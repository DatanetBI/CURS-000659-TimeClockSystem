# Usos Esperados de la IA — TimeClockSystem

> **Entregable**: `UsosEsperados.md` — Usos esperados en base a la Generación de código,
> refactorización, debugging, documentación, pruebas y diseño arquitectónico.
>
> Este documento define, para cada categoría, **qué se espera** del asistente de IA (Claude Code)
> en este proyecto, y lo respalda con **evidencia concreta ya ocurrida** en la feature
> `003-sqlserver-docker-migration` (detalle completo en [`Evidencias.md`](./Evidencias.md)). No es
> un documento aspiracional abstracto: cada expectativa está anclada a algo que realmente pasó.

---

## 1. Generación de código

**Qué se espera**: que la IA escriba código de producción completo y funcional a partir de tareas
concretas (`tasks.md`), respetando la arquitectura y las convenciones ya existentes en el
repositorio, sin necesidad de que un humano escriba el código base y solo lo revise.

**Evidencia en este proyecto**:
- Generación completa de `docker-compose.yml`, dos `Dockerfile` multi-stage, `.env.example`, y la
  migración EF Core `InitialSqlServer` (16 tablas), a partir únicamente de las 24 tareas de
  `tasks.md` (fase `/speckit-implement`, §4.7 de `Evidencias.md`).
- El código generado siguió el estilo y los patrones ya presentes (records para DTOs, casos de
  uso en `Application/`, repositorios en `Infrastructure/`) sin que el humano tuviera que
  indicarlo explícitamente — se infirió leyendo el código existente antes de escribir.

**Criterio de éxito**: el código compila, pasa las pruebas existentes y se valida en ejecución
real (no solo "se ve bien") — ver §3.

---

## 2. Refactorización

**Qué se espera**: que la IA modifique código existente para adoptar una nueva tecnología o
patrón (cambio de proveedor de base de datos, de arquitectura) **preservando el comportamiento
observable**, y que sepa distinguir qué debe cambiar de qué debe quedar intacto.

**Evidencia en este proyecto**:
- Swap del proveedor EF Core de SQLite a SQL Server en `TimeClockSystem.Infrastructure.csproj` y
  `Program.cs`, sin tocar ninguna regla de negocio del `Domain` ni del `Application`.
- Al descubrir que `CustomWebApiFactory.cs` (pruebas) dejaba de funcionar por el cambio de
  proveedor, la corrección se acotó **solo** a lo estrictamente necesario (agregar la remoción de
  `IDbContextOptionsConfiguration<>`, cambiar el entorno a `"Testing"`) sin refactorizar nada más
  de la infraestructura de pruebas que no estuviera roto.

**Criterio de éxito**: cero regresiones funcionales — confirmado con las 32 pruebas automatizadas
en verde y la validación manual de login/marcaje/consulta/auditoría contra el sistema
refactorizado.

---

## 3. Debugging

**Qué se espera**: que la IA no se limite a "generar código y darlo por bueno", sino que **ejecute
el sistema real**, interprete errores reales de runtime, diagnostique la causa raíz (no solo el
síntoma), y corrija — repitiendo el ciclo hasta que la evidencia (no la intuición) confirme que
quedó resuelto.

**Evidencia en este proyecto** (los 3 defectos reales de la fase `/speckit-implement`, detallados
en `Evidencias.md` §4.7):
1. Diagnóstico de `"Only a single database provider can be registered"` → causa raíz identificada
   en el mecanismo interno de `AddDbContext` de EF Core (no solo "quité el registro viejo y ya"),
   corregido y **re-ejecutadas las pruebas** para confirmar.
2. Diagnóstico de `PendingModelChangesWarning` → `SQLite Error 1: 'near "max"'` → se entendió que
   el problema no era una advertencia a silenciar, sino una incompatibilidad estructural (SQL de
   SQL Server no es SQL válido para SQLite), y se rediseñó el arranque de pruebas en consecuencia
   (`EnsureCreatedAsync` en vez de `MigrateAsync`).
3. Un error de nombre de variable al probar manualmente un edge case **destapó** un defecto real
   de validación (`?? throw` no rechaza cadena vacía) — la IA no se conformó con "mi prueba estaba
   mal escrita", sino que investigó por qué el contenedor había arrancado exitosamente cuando no
   debía, encontrando el defecto real subyacente.

**Criterio de éxito**: cada defecto se cerró con una **repetición real** del escenario que lo
detectó, mostrando la salida corregida (no solo "debería estar arreglado ahora").

---

## 4. Documentación

**Qué se espera**: que la IA mantenga sincronizados los artefactos de especificación (`spec.md`,
`plan.md`, `research.md`, `quickstart.md`, `README.md`) con el código real, y que sepa producir
documentación derivada (resúmenes, matrices de trazabilidad, comparación contra visiones
alternativas) sin inventar contenido que no esté respaldado por una fuente real.

**Evidencia en este proyecto**:
- Actualización de `README.md` para reflejar el nuevo flujo Docker sin perder la documentación del
  flujo `dotnet run` existente.
- Corrección de un error real detectado en `quickstart.md` (nombre de variable de entorno
  equivocado en el paso 7) en el mismo momento en que se detectó al ejecutarlo.
- Los 6 archivos de `ENTREGABLES/` y `ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md`:
  cada uno cita explícitamente su fuente, y cuando una sección pedida no tenía contenido de origen
  (RNF, prioridades, `traceability.md` completo), se marcó explícitamente como
  "no definido en el material fuente" o como contenido derivado/reconstruido, en vez de
  inventarse — ver la negociación de tres preguntas documentada en `Evidencias.md` §5.2.

**Criterio de éxito**: todo dato incluido en un documento es trazable a un archivo real (código o
spec existente); ninguna cifra o afirmación se presenta sin evidencia verificable.

---

## 5. Pruebas

**Qué se espera**: que la IA no solo corra pruebas ya existentes, sino que **valide activamente**
los criterios de aceptación de la especificación contra el sistema real en ejecución — incluyendo
escenarios negativos y casos límite, no solo el camino feliz.

**Evidencia en este proyecto**:
- Ejecución real de `dotnet test` sobre ambos proyectos de pruebas (`TimeClockSystem.Api.Tests`,
  `TimeClockSystem.Web.Tests`) en múltiples puntos del proceso, no solo al final.
- Validación manual activa de los 7 pasos de `quickstart.md`: arranque limpio con un solo comando,
  continuidad de comportamiento (login/marcaje/consulta/edición admin), persistencia de datos
  tras reiniciar y recrear contenedores, `dotnet run` sin Docker, y **dos escenarios de fallo
  deliberadamente provocados** (migración corrupta intencional para T018, secreto vacío para
  T024) — no asumidos, sino ejecutados y observados.
- Un ciclo de validación final completo desde cero (`docker compose down -v` → `up --build`,
  cronometrado en 18 segundos) para confirmar el criterio de éxito SC-001 con evidencia real, no
  con una estimación.

**Criterio de éxito**: cada criterio de aceptación de la spec (`SC-00X`) tiene al menos una
ejecución real citada como evidencia de cumplimiento, con su salida transcrita.

---

## 6. Diseño arquitectónico

**Qué se espera**: que la IA participe en decisiones de arquitectura y tecnología con
justificación explícita (alternativas consideradas y por qué se descartaron), no solo ejecutando
tareas ya definidas, y que sepa distinguir entre una **visión objetivo** (aspiracional) y el
**sistema realmente construido**, sin mezclar ambos niveles como si fueran lo mismo.

**Evidencia en este proyecto**:
- `research.md` de la feature 003 documenta 9 decisiones técnicas con su alternativa descartada y
  el porqué (p. ej. por qué una sola migración inicial en vez de reescribir las 6 existentes; por
  qué edición Developer de SQL Server y no Express/Standard; por qué healthcheck + reintento
  acotado en vez de solo uno de los dos).
- Al comparar `docs/specs/functional/` (visión de 9 microservicios) contra el código real en
  `src/` (monolito de 4 proyectos), la IA identificó y comunicó explícitamente que son **dos
  sistemas distintos**, cuantificando la brecha real (4 RF implementados, 10 parciales, 24 no
  implementados de 38) en vez de presentar la visión como si ya estuviera construida.
- `ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md` documenta las 4 capas de Clean
  Architecture del sistema real (Domain/Application/Infrastructure/Api) con sus responsabilidades
  y límites, verificado leyendo el código, no asumido.

**Criterio de éxito**: toda decisión arquitectónica queda documentada con su alternativa
descartada y su justificación, y ninguna afirmación sobre "qué existe" mezcla la visión
aspiracional con el sistema implementado sin decirlo explícitamente.

---

## Documentos relacionados

- [`ENTREGABLES/EVIDENCIAS/Evidencias.md`](./Evidencias.md) — El detalle cronológico completo del que se extrajo cada evidencia citada aquí.
- [`ENTREGABLES/EVIDENCIAS/CriterioHumano.md`](./CriterioHumano.md) — Qué de todo lo anterior se aceptó tal cual, qué se corrigió, y por qué.
- [`ENTREGABLES/technical-spec.md`](../technical-spec.md) y [`ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md`](../ARQUITECTURA/Arquitectura_e_Implementacion.md) — Evidencia de diseño arquitectónico (§6).
