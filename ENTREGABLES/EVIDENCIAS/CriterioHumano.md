# Criterio Humano — TimeClockSystem

> **Entregable**: `CriterioHumano.md` — Explicar qué se aceptó, qué se corrigió y por qué la
> propuesta final es adecuada.
>
> Este documento sintetiza el rol del juicio humano a lo largo del proceso documentado en
> [`Evidencias.md`](./Evidencias.md), sobre la feature `003-sqlserver-docker-migration` y los
> entregables derivados de ella.

---

## 1. Qué se aceptó

### 1.1 Las 9 decisiones de clarificación de la especificación

A lo largo de `/speckit-specify` y `/speckit-clarify` se presentaron **9 preguntas** con una
opción "Recomendada" explícita y su justificación. El usuario aceptó las 9 tal cual se
propusieron, sin modificarlas:

- Entorno objetivo solo desarrollo/demo local (no producción).
- Conservar `dotnet run` como alternativa a Docker.
- No migrar datos existentes de SQLite (base vacía + resiembra).
- Límite acotado de reintentos de conexión a la base de datos (no infinito).
- Fallar y detener el contenedor ante un error real de migración.
- Mantener el objetivo de rendimiento heredado de la funcionalidad 002 (95% de acciones &lt;5s).

**Por qué era razonable aceptarlas**: cada opción recomendada se justificó con un criterio
verificable (simplicidad para un entorno de un solo desarrollador, evitar sobre-ingeniería, no
introducir trabajo no solicitado por la spec original) y ninguna cerraba una puerta sin
justificación — todas eran reversibles en una iteración futura si cambiaran los requisitos de
negocio.

### 1.2 Las decisiones de diseño técnico de `research.md`

El usuario no objetó ninguna de las 9 decisiones técnicas documentadas en `research.md` (proveedor
EF Core, edición de SQL Server, mecanismo de espera, manejo de fallo de migración, patrón de
Dockerfile, comunicación por nombre de servicio, manejo de secretos vía `.env`, no tocar la
infraestructura de pruebas). Se aceptaron implícitamente al no pedir cambios antes de
`/speckit-implement`.

**Por qué era razonable aceptarlas**: cada una tenía una alternativa descartada documentada con su
motivo (ver `ENTREGABLES/EVIDENCIAS/UsosEsperados.md` §6), y todas eran consistentes con el
principio de simplicidad de la constitución del proyecto (`.specify/memory/constitution.md`,
Principio I).

### 1.3 El formato y alcance de los 6 entregables SDD y del resumen de arquitectura

Ante la petición de crear `vision.md`, `requirements.md`, `user-stories.md`, `use-cases.md`,
`technical-spec.md` y `traceability.md`, se plantearon 3 dudas concretas (RNF/prioridades
inexistentes, formato de casos de uso, contenido de la matriz de trazabilidad) y el usuario eligió
una opción para cada una. Se aceptaron sin objeción posterior.

**Por qué era razonable aceptarlas**: en los tres casos, la alternativa elegida era la que
preservaba más fielmente el material fuente real (extraer RNF citando su origen en vez de
inventarlos; reorganizar Gherkin sin reescribirlo; incluir endpoints reales con advertencia
explícita de que pertenecen a otro sistema) — coherente con el principio general de este proceso
de no fabricar contenido no verificable.

---

## 2. Qué se corrigió

### 2.1 Tres defectos reales de código, encontrados por ejecución real (no por revisión de código)

| Defecto | Cómo se detectó | Corrección aplicada |
|---|---|---|
| Doble proveedor EF Core registrado en pruebas (`Only a single database provider can be registered`) | 20/23 pruebas fallando al ejecutar `dotnet test` | Quitar también `IDbContextOptionsConfiguration<ApplicationDbContext>` en `CustomWebApiFactory.cs`, no solo `DbContextOptions<>` |
| Migraciones SQL Server no son SQL válido para SQLite (`PendingModelChangesWarning` → `SQLite Error 1: 'near "max"'`) | Reintento de pruebas tras el fix anterior, mismo comando `dotnet test` | Rama `IsEnvironment("Testing")` en `Program.cs` que usa `EnsureCreatedAsync()` en vez de reproducir la migración real |
| Secreto vacío (no ausente) no era rechazado — el contenedor arrancaba con `Jwt:SigningKey` vacío | Validación manual del edge case de "secreto faltante" (T024), con un error de nombre de variable que llevó a investigar por qué el contenedor seguía arrancando | Cambiar `?? throw` por `string.IsNullOrWhiteSpace(...)` para `Jwt:SigningKey` y `ConnectionStrings:DefaultConnection` |

**Por qué estas correcciones son adecuadas**: ninguna se aplicó "a ciegas" — cada una se
re-verificó ejecutando de nuevo el escenario que la había detectado (pruebas en verde, contenedor
fallando explícitamente donde antes arrancaba con un valor inseguro), documentado con la salida
real en `Evidencias.md` §4.7.

### 2.2 Un error de conteo propio, detectado y corregido antes de reportar

Al redactar el resumen de cobertura de `ENTREGABLES/traceability.md`, un primer conteo manual de
los 38 requisitos funcionales sumaba 35 en vez de 38. Se detectó al re-sumar la tabla completa
antes de presentar el resultado final, y se corrigió en el mismo turno (4 implementados + 10
parciales + 24 no implementados = 38), dejando el error y la corrección visibles en
`Evidencias.md` §5.2 en vez de ocultarlos.

### 2.3 Una petición del usuario que no se podía cumplir tal como se pidió — y se dijo explícitamente

Este es el caso más importante de esta sección. Al pedir `Evidencias.md`, el usuario respondió a
la primera pregunta de aclaración exigiendo *"fidelidad completa... de esta sesión y de todas las
sesiones anteriores, sin excepción"*. Cumplir esa instrucción literalmente habría significado
**inventar prompts que nunca existieron** para las sesiones anteriores del proyecto (no hay
transcript de ellas en ningún lado).

Se corrigió la instrucción del usuario, no el trabajo propio: se explicó por qué era
imposible cumplirla honestamente, y se ofreció la única alternativa que sí lo era (reconstrucción
explícitamente marcada como tal, basada en el historial real de Git, para las sesiones anteriores).
El usuario aceptó esa alternativa corregida.

**Por qué esta corrección era necesaria**: un documento de "evidencias" cuyo contenido está
parcialmente fabricado deja de ser evidencia — sería peor que un documento incompleto pero
honesto. Negociar el alcance con el usuario, en vez de fabricar contenido para parecer cumplido,
es el propio criterio humano que este documento describe, aplicado por el asistente y validado
por el usuario en el momento en que ocurrió.

**Actualización posterior (misma lógica, mejor evidencia)**: poco después, el usuario aportó
`LogPrompts.txt` — un log que él mismo había recopilado manualmente y que sí contiene prompts
reales de las sesiones anteriores. Esto no invalida la decisión anterior; la **supera con
evidencia real** en vez de con invención: `Evidencias.md` §1–§4 se reescribió con esos prompts
literales (ver `Evidencias.md` §5.5), y la limitación honesta que motivó el rechazo original quedó
documentada en el propio archivo en vez de borrarse silenciosamente.

---

## 3. Por qué la propuesta final es adecuada

1. **Cada afirmación es verificable, no solo plausible.** Los 24 tareas de `tasks.md`, los 6
   entregables SDD, el resumen de arquitectura y este mismo archivo de evidencias citan su fuente
   real (archivo, commit, o salida de comando) en vez de presentarse como autocontenidos.
2. **Los defectos no se escondieron detrás del resultado final.** Los tres defectos reales de
   `/speckit-implement` (§2.1) y el error de conteo (§2.2) quedan documentados como parte del
   proceso, no borrados de la narrativa una vez corregidos — permite auditar no solo *qué* se
   construyó sino *cómo* se llegó a la versión final.
3. **La validación es real, no asumida.** El criterio de éxito SC-001 (arranque con un solo
   comando en menos de 10 minutos) no se declaró cumplido por diseño — se cronometró un ciclo real
   completo desde cero (18 segundos). Los escenarios de fallo (migración corrupta, secreto vacío)
   se provocaron deliberadamente y se observó la falla real, no se infirió del código.
4. **Se distinguió explícitamente lo que existe de lo que se aspira a construir.** La comparación
   entre `docs/specs/functional/` (visión de 9 microservicios) y el sistema real en `src/`
   cuantificó la brecha (4/38 RF implementados) en vez de presentar ambos como equivalentes — esto
   evita que un lector de `ENTREGABLES/` sobreestime el alcance real del sistema.
5. **Cuando una instrucción no se podía cumplir honestamente, se dijo, no se simuló cumplirla.**
   El caso de §2.3 es la prueba de que el criterio aplicado en todo el proceso (preferir una
   respuesta incompleta pero verdadera sobre una completa pero fabricada) también rigió cuando el
   propio usuario pidió lo contrario.

En conjunto, la propuesta final —el código de `specs/003-sqlserver-docker-migration/`
implementado y validado, más los documentos de `ENTREGABLES/`— es adecuada porque cada componente
puede rastrearse hasta una fuente real (código, comando ejecutado, o commit de Git), y porque las
limitaciones genuinas del proceso (sesiones anteriores sin transcript, brechas de cobertura
funcional, ambigüedades no resueltas en el material fuente) se documentaron explícitamente en vez
de disimularse.

## Documentos relacionados

- [`ENTREGABLES/EVIDENCIAS/Evidencias.md`](./Evidencias.md) — Detalle cronológico completo de cada decisión referenciada aquí.
- [`ENTREGABLES/EVIDENCIAS/UsosEsperados.md`](./UsosEsperados.md) — Marco de usos esperados frente al cual se evalúa esta propuesta.
