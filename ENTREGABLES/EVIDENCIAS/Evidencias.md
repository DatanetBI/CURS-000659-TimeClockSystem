# Evidencias del Proceso — TimeClockSystem

> **Entregable**: `Evidencias.md` — Prompts, respuestas relevantes, diffs, decisiones
> aceptadas/rechazadas y capturas del proceso, tal cual sucedieron.

## 0. Nota metodológica (léase antes que el resto del documento)

Este documento combina **dos fuentes de evidencia** de niveles de fidelidad distintos:

1. **Fidelidad completa por testigo directo** — todo lo ocurrido en **esta conversación**
   (Claude Code), desde la petición de artefactos SDD estándar en adelante (§5 y §6 de este
   documento). Prompts y respuestas transcritos literalmente, capturas de salidas de terminal
   reales.
2. **Prompts reales recopilados manualmente por el usuario** (§1–§4) — el archivo
   `E:\Courses\GalaxyTraining\2026\AISpectDrivenDevelopmentNET\Dia06\LogPrompts.txt`, que el
   propio usuario aportó y describió como *"la mayoría de los PROMPTS y respuestas que yo he
   recabado de forma manual"*. Es una fuente primaria real (no una reconstrucción de este
   asistente), pero con dos matices honestos:
   - El propio usuario advierte que es "la mayoría", no necesariamente el 100% — puede haber
     turnos intermedios no capturados en su recopilación manual.
   - Este asistente no presenció esas sesiones; no puede verificar independientemente que el log
     sea perfectamente literal, solo transcribirlo fielmente tal como el usuario lo entregó,
     dejando explícita esta procedencia en cada sección.

   Una referencia suelta al final de `LogPrompts.txt` (línea 1439) mencionaba un segundo archivo
   (`Log de Comandos y Prompts.txt`); se verificó que **no existe** en esa carpeta, y el usuario
   confirmó ignorarla.

**Corrección importante respecto a una versión anterior de este mismo documento**: una versión
previa de `Evidencias.md` afirmaba que "no existe ningún transcript" de las sesiones anteriores a
esta conversación, y por eso solo reconstruía su línea de tiempo a partir de commits de Git. Esa
afirmación quedó **superada** al recibir `LogPrompts.txt`, que sí contiene prompts reales de esas
sesiones. Se documenta aquí el cambio en vez de ocultarlo, siguiendo el mismo criterio de
transparencia que rige el resto de este archivo.

**Sobre "capturas del proceso"**: no hay capacidad de generar capturas de pantalla de sesiones de
terminal pasadas. Las "capturas" de este documento son transcripciones textuales literales
(citadas del log real o de la salida real de esta conversación), no imágenes — acordado
explícitamente con el usuario.

**Nivel de detalle**: log cronológico condensado pero fiel — cada entrada resume el prompt y la
respuesta, cita literalmente los fragmentos más relevantes, y evita pegar el contenido íntegro de
cada archivo generado (esos archivos ya existen en el repositorio y se referencian por ruta).

---

## 1. Fase previa a Spec Kit — Documentación funcional y arquitectónica

*Fuente: `LogPrompts.txt`, líneas 1–370. Esta fase ocurrió **antes** de `git init` — por eso
`docs/specs/functional/` y `docs/architecture/` aparecen completos desde el primer commit del
repositorio (`115abb5`): no faltaba historial de Git, esta fase simplemente no tenía repositorio
todavía.*

### 1.1 Documento de visión + historias de usuario

**Prompt real** (resumido; el original incluye las 9 descripciones de módulo completas y el
diagrama de capas ASCII): *"Actúa como Lead Requirements Engineer. Elabora la documentación
funcional del Time Clock System. Escribe la especificación para los módulos siguientes: [9 módulos
con su descripción funcional completa, diagrama de 3 capas] ... y guarda conceptualmente los
archivos en '/docs/specs/functional/'. Instrucciones: 1. Genera 01-vision-document.md... 2. Genera
02-user-stories.md con historias en formato Gherkin para: [13 historias US-001 a US-013 con su
Como/quiero/para completo] ... 3. Valida los diagramas de secuencia con `@mermaid-js/mermaid-cli`."*

**Resultado real**: `01-vision-document.md` (propósito, 7 objetivos con KPI, alcance de 9
módulos, fuera de alcance, supuestos, 8 perfiles, principios, glosario) y `02-user-stories.md`
(13 historias + US-011b, Gherkin con 3-4 escenarios cada una, 6 diagramas de secuencia Mermaid).
Validación real: los 6 diagramas se renderizaron con `mmdc` sin errores (exit code 0, SVG de
32–43 KB).

### 1.2 `VALIDATION.md`

**Prompt real**: *"Genera el documento de VALIDATION.md"*.

**Resultado real**: documento con metodología, tabla de los 6 diagramas validados, observaciones
de cobertura y comando de reproducibilidad.

### 1.3 Diagramas como archivos independientes

**Prompt real**: *"genera los diagramas por separado y guardalos en una carpeta llamada diagrams
dentro de la carpeta funtional"*.

**Resultado real**: carpeta `diagrams/` con 6 pares `.mmd`/`.svg` + `README.md` índice.
**Incidente real documentado por el propio log**: *"durante el render encontré una caché corrupta
de npx (`mermaid.esm.mjs` faltante en una instalación parcial de mermaid-cli); la borré y el
render se completó sin errores"* — un problema técnico real, diagnosticado y resuelto en el
momento, no anticipado.

### 1.4 Arquitectura: modelo de dominio (DDD) + C4 containers

**Prompt real**: *"Actúa como Software Architect. Analiza el código y la documentación disponible
en '/docs/specs/functional/' para generar el diseño de arquitectura en '/docs/architecture/'.
[...] 1. Diseña /docs/architecture/domain-model.md con diagramas Mermaid de [las 14 historias]
2. Genera /docs/architecture/c4-containers.md definiendo Web API (.NET 10), PostgreSql y Redis
Cache."*

**Resultado real**: `domain-model.md` (8 bounded contexts + shared kernel de Identidad,
trazabilidad HU→contexto→agregado) y `c4-containers.md` v1.0 (monolito modular sobre .NET 10 +
PostgreSQL 16 con esquema por contexto + Redis). 13 diagramas Mermaid, 13/13 válidos.

### 1.5 Primera iteración de arquitectura — microservicios (⚠️ resultado no deseado)

**Prompt real**: *"Ajusta /docs/architecture/c4-containers.md y los documentos que sean
necesarios, para que en la 'Decisión arquitectónica' cada módulo sea implementado en un servicio
independiente."*

**Resultado real**, marcado explícitamente por el usuario en su propio log como **"OJO DIO UN
RESULTADO NO DESEADO"**: `c4-containers.md` v2.0 pasó a microservicios con **una base de datos
PostgreSQL por servicio** (*database-per-service*, 9 instancias) + API Gateway + Redis como bus de
eventos.

**Decisión rechazada**: el usuario no aceptó *database-per-service* — lo dejó anotado como no
deseado y pidió una corrección en el siguiente prompt (§1.6). Este es el primer ejemplo real de
"decisión rechazada" del proyecto.

### 1.6 Segunda iteración — microservicios con una sola base de datos compartida

**Prompt real**: *"Ajusta /docs/architecture/c4-containers.md [...] para que en la 'Decisión
arquitectónica' cada módulo sea implemente en un servicio independiente, pero todos los servicios
apuntando a una sola base de datos."*

**Resultado real**: `c4-containers.md` v2.1 — se mantienen los 9 microservicios, pero **se
revierte** *database-per-service* a una única `timeclock_db` con permisos (`GRANT`) aislados por
esquema. Este cambio es la corrección directa de la decisión rechazada en §1.5.

### 1.7 Tercera iteración — acceso abierto entre esquemas + ADR-001

**Prompt real**: *"Ajusta [...] para corregir el principio §1.3 [...] con el objetivo de permitir
cualquier JOIN o transacción que sea necesario entre esquemas de la base de datos. Los esquemas
[...] son una decisión de agrupamiento de datos y NO para aislamiento [...]. Escribe ADR-001
justificando Clean Architecture + CQRS con multi servicios y una sola Base de datos vs
Microservicios y múltiples bases de datos."*

**Resultado real**: `c4-containers.md` v3.0 (esquemas como agrupación lógica, sin aislamiento por
permisos) + `ADR-001` nuevo (Opción A elegida: Clean Architecture + CQRS, multi-servicio, BD única
de acceso abierto; Opciones B y C descartadas con su justificación). Esta es la versión que
persiste hasta hoy en el repositorio.

### 1.8 Especificación consolidada (`03-especificacion-consolidada.md`)

**Prompt real**: una plantilla completa de 8 secciones (Objetivo y contexto, Usuarios, Escenarios,
Requisitos funcionales, Reglas de negocio, Criterios de aceptación, Casos límite, Fuera de
alcance), pidiendo sintetizar `c4-containers.md`, `domain-model.md`, `01-vision-document.md`,
`02-user-stories.md` y `VALIDATION.md` en ese formato exacto.

**Resultado real**: `03-especificacion-consolidada.md` con 14 HU, 38 RF, 14 RN (con ejemplo
numérico cada una), 20 CA y 12 CL — la síntesis que hoy alimenta `ENTREGABLES/requirements.md`.

### 1.9 Inicialización de Git

**Comandos reales**: `git init`, `git branch -m master main`, `git add .`, `git commit -m "chore:
inicializar repositorio para speckit"` → commit `115abb5`. El propio log anota: *"no eran
necesarios los comandos"* (juicio retrospectivo del usuario sobre su propia secuencia de
comandos).

---

## 2. Feature `001-timeclock-spec-v1` (con Spec Kit)

*Fuente: `LogPrompts.txt`, líneas 371–800.*

### 2.1 `/speckit-constitution`

**Prompt real** (íntegro): *"Crea los principios que gobiernan la aplicación [...] Los principios
serán los siguientes: 1. Simplicidad ante todo [...] 2. Idioma y mercado: Todo el producto debe
estar en español de México. Moneda: Pesos Mexicanos. 3. Cero alcance fantasma [...] 4. Verificable
por una persona no técnica [...] 5. Datos del usuario: Pedir solo lo necesario. No introducir
claves ni secretos en el código."* — estos 5 principios literales son los que hoy gobiernan
`.specify/memory/constitution.md` y que este asistente respetó en toda la feature 003.

### 2.2 `/speckit.specify` — especificación inicial

**Prompt real**: una plantilla de especificación completa (Objetivo, Usuarios, Escenarios, 38 RF,
14 RN, 20 CA, 12 CL, Fuera de alcance) idéntica en estructura a `03-especificacion-consolidada.md`,
cerrando con: *"Antes de redactar la especificación formal, hazme las preguntas que necesites para
resolver cualquier ambigüedad. No implementes nada todavía."*

**Resultado real**: `specs/001-timeclock-spec-v1/spec.md` + `checklists/requirements.md`.

### 2.3 `/speckit.specify` — modificación (marcaje por número + PIN)

**Prompt real**: *"Modifica la especificación 001-timeclock-spec-v1, para agregar un nuevo método
de registro de asistencia del empleado, adicional a la Biometría facial, este registro será
mediante un número de empleado y una contraseña."*

**Resultado real**: 2 escenarios nuevos en User Story 1, FR-002/003/004 nuevos (renumerando el
resto), entidad "Credencial de Marcaje" nueva, CL13 nuevo, checklist revalidado 16/16.

### 2.4 `/speckit.clarify`

**Resultado real**: 3/5 preguntas respondidas; se normalizó terminología ("contraseña" → "PIN"),
se resolvieron unicidad de credencial, escala (SC-010: hasta 500 empleados), disponibilidad y
seguridad (PIN 4-6 dígitos sin caducidad). Observabilidad se difirió explícitamente a
`/speckit-plan` por ser detalle técnico, no de negocio.

### 2.5 `/speckit-plan` — primer plan

**Prompt real**: *"Prioriza la simplicidad por encima de todo, según la constitución. Es una
versión 1.0 que debe poder publicarse online enseguida y funcionar de manera responsiva en el
móvil. Para simplificar la prueba de concepto por parte del usuario carga la base de datos con
datos mock. No agregues nada de infraestructura que la spec no requiera. Detalla las decisiones
importantes en lenguaje de negocio."*

**Decisión relevante señalada explícitamente por el asistente de esa sesión**: los documentos de
`docs/architecture/` (9 microservicios + Kubernetes + Redis + SignalR) **no se adoptaron** para
v1.0 — se construyó una sola aplicación web ASP.NET Core con SQLite, apartándose deliberadamente
de la arquitectura documentada, justificado por la instrucción explícita de simplicidad y el
Principio I de la constitución.

### 2.6 `/speckit-plan` — ajuste (alcance por módulos esenciales)

**Prompt real**: *"Modifica el plan para: [...] La prioridad debe ser la funcionalidad WEB ONLINE,
y dejamos para otra versión la disponibilidad OFFLINE. [...] Crea un plan donde se implementan
solo los microservicios esenciales [...] en el orden en que el usuario pueda ir probando la
aplicación paso a paso [...] Los módulos serán: Gestión de centros de trabajo, Gestión de
empleados y sus datos, Gestión turnos y asignación, Gestión de días festivos, Registro de
asistencias, Portal de consulta [...] Deja para otra versión: Incidencias/vacaciones y
Exportación/importación [...] Crea datos mock para centros de trabajo, empleados y sus
credenciales, usuarios administradores..."*

**Resultado real**: plan revisado con 6 módulos en orden de construcción secuencial (cada uno
probable apenas está listo), roles simplificados a 2 (Empleado/Administrador), datos mock
explícitos (3 centros, 2 administradores, ~12 empleados con PIN, 2-3 turnos, calendario de
festivos). `spec.md` no se tocó — el plan documenta qué subconjunto de la spec completa se
construye en v1.0.

### 2.7 `/speckit-tasks`

**Resultado real**: 67 tareas (T001–T067) en 9 fases; MVP sugerido hasta el Módulo 5 (Registro de
asistencias).

### 2.8 `/speckit-analyze` + guardado como `Specification-Analysis-Report-RC1.md`

**Prompts reales**: `/speckit-analyze` → *"guarda este reporte de como
Specification-Analysis-Report-RC1.md"* → *"haz la propuestas de las ediciones concretas de
remediación para estos 8 hallazgos del reporte."*

**Resultado real**: 8 hallazgos (G1 consentimiento de geolocalización faltante, I1 atribución
incorrecta de FR, I2 colisión de etiquetas [US1]-[US6], I3 canales no diferidos, U1 mecanismo de
FR-014, G2 sin prueba de escala, U2 árbol de estructura incompleto, G3 sin acción) con su
remediación concreta propuesta por archivo.

### 2.9 Confirmación de remediación

**Prompt real**: *"confirma el commit"* → commit `99005f1` con las remediaciones de los 7
hallazgos accionables.

### 2.10 `/speckit-implement`

**Resultado real**: 69 tareas completas. Build 0 errores/0 advertencias, 24/24 pruebas (incluida
la prueba de carga de 500+ empleados de SC-010). **Dos defectos reales encontrados y corregidos,
no anticipados en `tasks.md`**: (1) `HtmlEncoder` por defecto convertía acentos/eñes a entidades
HTML — corregido; (2) el aislamiento de base de datos entre pruebas de integración no funcionaba
con el modelo de hosting mínimo (datos acumulándose entre corridas) — corregido con el patrón
recomendado por Microsoft de reemplazar `DbContextOptions` en `ConfigureServices`. Verificación
manual-equivalente vía HTTP de todo `quickstart.md` (el asistente de esa sesión aclaró
explícitamente que no tenía navegador real disponible).

### 2.11 Manual de usuario

**Prompt real**: *"Utiliza la documentación specs\001-timeclock-spec-v1, para generar un manual de
usuario, para cada rol del sistema, incluye usuarios y contraseñas mock usadas para levantar la
base de datos."* → *"Todo Correcto guarda el manual"*.

**Resultado real**: `MANUAL-USUARIO.md` (commit `1bf93bd`).

---

## 3. Feature `002-split-backend-frontend` (con Spec Kit)

*Fuente: `LogPrompts.txt`, líneas 802–1045.*

### 3.1 Patrón de "borrador sin ejecutar" (primera aparición documentada)

**Prompt real**: *"Necesito separar el proyecto en dos, uno será el BackEnd en WebAPI y el otro
será el mismo FrontEnd del Proyecto MVC. Muéstrame solo la especificación para usarla con
'/speckit.specify', si así yo lo decido. No hagas nada solo dame la especificación."*

**Respuesta real**: un texto completo de especificación candidata (contexto actual real del
código, 6 puntos de "qué debe pasar", fuera de alcance, 5 preguntas abiertas), presentado **sin
ejecutar nada**, con la instrucción de que el usuario decidiera si usarlo.

**Este es el mismo patrón que luego se repitió, casi literalmente, antes de la feature 003** (ver
§4.1) — un flujo deliberado de dos pasos: primero un borrador de spec para revisión humana, luego
la ejecución real de `/speckit.specify` con ese texto (posiblemente ajustado).

### 3.2 `/speckit.specify` real

**Prompt real**: el mismo texto de §3.1, pegado como argumento de `/speckit.specify`, sin cambios.

**Resultado real**: `specs/002-split-backend-frontend/spec.md`.

### 3.3 `/speckit.clarify`

Ejecutado sin texto adicional (commit `ceca399`, que agrupó specify+clarify en un solo commit).

### 3.4 `/speckit-plan`

**Prompt real**: *"Asegúrate de que el WebAPI tenga las capas Domain, Application e Infrastructure
de Clean Architecture. Si tienes alguna otra duda pregúntame."*

**Resultado real**: `plan.md` + `research.md` (8 decisiones: capas Clean Architecture, puente
cookie↔JWT, Swagger, sin contratos compartidos, auditoría, reintentos, migración de datos,
proyectos de prueba) + `data-model.md` + 8 archivos de `contracts/` + `quickstart.md`. Se agregó
una aclaración adicional a `spec.md` (pantalla de auditoría, FR-011a) surgida durante la
planificación.

### 3.5 `/speckit-tasks`

**Resultado real**: 63 tareas (T001–T063) en 6 fases; MVP sugerido hasta el final de US1.

### 3.6 `/speckit.analyze`

**Resultado real**: 5 hallazgos — **I1 (HIGH)**: el Edge Case exigía revalidar el rol vigente en
cada operación, pero el diseño de JWT sin refresh hacía que un cambio de rol a mitad de sesión no
se reflejara hasta expirar el token — una **contradicción real entre un edge case y una decisión
de diseño ya tomada**, no solo un hueco de cobertura. **U1 (HIGH)**: no estaba especificado cómo
el Backend resuelve "cuál Empleado soy" a partir del JWT (claim `empleadoId` faltante en el
diseño). **A1 (HIGH)**: SC-002 pedía un porcentaje estadístico (95%) no demostrable con una sola
corrida de prueba. **G1/G2 (MEDIUM/LOW)**: SC-007 y FR-008 sin tarea de verificación.

### 3.7 Remediación y confirmación

**Prompt real**: *"confirma el commit y luego aplica las remediaciones I1, U1, A1, G1"*.

**Resultado real**: los 4 hallazgos de mayor severidad remediados antes de implementar (G2 quedó
pendiente/de menor prioridad, consistente con la recomendación del propio análisis).

### 3.8 `/speckit.implement`

**Resultado real**: 64/64 tareas. Backend nuevo con 4 proyectos Clean Architecture, JWT de
expiración fija, 9 controllers REST. Frontend con 0 referencias de proyecto al Backend
(verificado), puente cookie↔JWT server-side (token nunca llega al navegador, verificado
inspeccionando la cookie). 32/32 pruebas (23 + 9). **Un ajuste real de contrato durante la
implementación**: la pantalla de "Consulta de Asistencias" del Administrador necesitaba navegar
todos los empleados con filtros (no el historial de uno solo) → se agregó
`GET /api/consulta-asistencias/admin`, actualizando `contracts/consulta-asistencias.md` para
reflejar el endpoint real construido, no al revés. **Un bug real encontrado y corregido durante la
validación manual**: el Frontend no mostraba un mensaje amigable cuando el Backend no respondía —
corregido con un filtro global.

---

## 4. Feature `003-sqlserver-docker-migration` (con Spec Kit) — esta conversación

*Desde aquí, fidelidad completa por testigo directo de este asistente, con una excepción marcada
explícitamente en §4.1.*

### 4.1 Patrón de "borrador sin ejecutar" (repetido) — ocurrió en una sesión previa a esta conversación

*Fuente: `LogPrompts.txt`, líneas 1046–1104 — no presenciado por este asistente.*

**Prompt real**: *"Necesito que el backend ahora se conecte a Microsoft SQLServer y que la base de
datos, el backend y el frontend se levanten en contenedores docker. Muéstrame solo la
especificación para usarla con '/speckit.specify', si así yo lo decido. No hagas nada solo dame la
especificación."*

**Respuesta real**: el texto completo de especificación candidata que luego **es exactamente el
mismo texto** con el que comenzó esta conversación (ver §4.2) — confirma que el primer mensaje de
esta conversación no fue redactado "desde cero" por el usuario, sino que provino de este mismo
patrón de dos pasos usado en 002 y en la fase previa a Spec Kit.

### 4.2 `/speckit-specify` — primer mensaje real de esta conversación

**Prompt del usuario** (íntegro, coincide con `LogPrompts.txt` líneas 1109–1161): el texto de
migración a SQL Server + Docker con 6 puntos de "qué debe pasar", fuera de alcance, y 3 preguntas
abiertas.

**Respuesta / proceso real**: hook `before_specify` → `/speckit-git-feature` creó la rama
`003-sqlserver-docker-migration`; se generó `spec.md` con 3 historias, 14 FR, 7 SC, y las 3
preguntas como `[NEEDS CLARIFICATION]`, presentadas con tabla de opciones.

**Decisiones aceptadas** (las 3, todas la opción "Recomendado"): entorno solo dev/demo local; se
conserva `dotnet run`; no se migran datos de SQLite (base vacía + resiembra). Checklist 16/16.
Commit `after_specify` aceptado.

### 4.3 `/speckit-clarify` — segunda ronda

**Respuesta real**: 3 preguntas adicionales — límite acotado de reintentos de conexión; fallar y
detener el contenedor ante error real de migración; mantener el objetivo de 95% de acciones &lt;5s
heredado de 002. Las 3 aceptadas. Checklist 16/16 tras integrar. Commit `after_clarify` aceptado.

### 4.4 `/speckit-plan`

**Respuesta real**: `plan.md`, `research.md` (9 decisiones), `data-model.md`, `contracts/environment-variables.md`, `quickstart.md` (7 pasos). Constitution Check 5/5 PASS. Decisión clave: una única migración inicial en vez de reescribir las 6 existentes, justificada por la Clarification de no migrar datos. Commit `after_plan` aceptado.

### 4.5 `/speckit-tasks`

**Respuesta real**: `tasks.md`, 24 tareas en 6 fases (Setup/Foundational/US1/US2/US3/Polish), cada
una citando su FR/SC de origen. Commit `after_tasks` aceptado.

### 4.6 `/speckit-analyze`

**Respuesta real**: 0 CRITICAL, 0 HIGH; 4 MEDIUM (huecos de cita FR-011/FR-012/FR-014/SC-004,
corregidos en el mismo turno), 1 MEDIUM (inconsistencia: `quickstart.md` sin paso para el
escenario de T018), 2 LOW (ambigüedad de ventana de reintento; 2 tareas sin cita). Remediación de
citas aplicada de inmediato por ser cambios de una línea sin riesgo.

**Prompt real del usuario**: *"guarda este reporte como Specification-Analysis-Report-DBMigration.md"* → guardado replicando la convención de `-RC1.md` y `-BackEnd.md` de las features anteriores.

### 4.7 `/speckit-implement` — la fase con más correcciones reales

Resumen (detalle completo de capturas de terminal reales ya documentado, sin cambios respecto a
la versión anterior de este archivo):

**a) Foundational (T001–T008)**: swap de proveedor EF Core, 1 migración inicial (`InitialSqlServer`), reintento acotado en `Program.cs`. Build limpio confirmado.

**b) User Story 1 (T009–T015)**: Dockerfiles, `docker-compose.yml`, `.env.example`. Validación real: `docker compose up --build -d` → los 3 contenedores sanos; login real vía el Frontend MVC confirmando comunicación por nombre de servicio Docker.

**c) User Story 2 (T016–T018) — 3 defectos reales encontrados y corregidos**:
1. Doble proveedor EF Core registrado en pruebas (`Only a single database provider can be registered`) — 20/23 pruebas fallando. Corregido quitando también `IDbContextOptionsConfiguration<ApplicationDbContext>`.
2. Migraciones SQL Server no son SQL válido para SQLite (`PendingModelChangesWarning` → `SQLite Error 1: 'near "max"'`) — corregido con rama `IsEnvironment("Testing")` que usa `EnsureCreatedAsync()`.
3. Secreto vacío (no ausente) no era rechazado (`?? throw` no cubre cadena vacía) — corregido con `string.IsNullOrWhiteSpace(...)`, detectado al validar manualmente el edge case de secreto faltante.

**d) User Story 3 (T019–T020)**: volumen `mssql-data`, validado con ciclo real `down`/`up` (datos intactos, sin duplicar).

**e) Polish (T021–T024)**: `README.md`, manejo de secretos validado, `dotnet run` validado fuera de Docker, fallo explícito ante secreto faltante confirmado.

**Validación final real**: ciclo completo desde cero (`down -v` → `up --build`, 18 segundos), 23/23 y 9/9 pruebas en verde. Commit `after_implement` aceptado.

### 4.8 `/speckit-converge`

**Respuesta real**: 12 puntos verificados contra el código real (subagente de solo lectura) → **"✅ Converged"**, 0 hallazgos, `tasks.md` sin modificar.

---

## 5. Después de Spec Kit — Preguntas libres, ENTREGABLES y este mismo documento

*Fidelidad completa, esta conversación.*

### 5.1 Pregunta libre — Equivalencias de artefactos SDD estándar

El usuario preguntó si existían `vision.md`, `requirements.md`, `user-stories.md`, `use-cases.md`,
`technical-spec.md`, `traceability.md`. Se entregó una tabla de equivalencias contra `spec.md` /
`plan.md`+`research.md`+`data-model.md` / `Specification-Analysis-Report-*.md`. Luego el usuario
pidió comparar además contra `docs/specs/functional/`, encontrando 2 cumplimientos completos, 2
parciales, y confirmando que `traceability.md` no existe en ningún lado del repositorio.

### 5.2 Creación de `ENTREGABLES/` (6 archivos)

Ante la petición explícita de crear los 6 archivos con la información original y **"cualquier duda
pregúntame"**, se plantearon y resolvieron 3 dudas antes de escribir nada:

| Duda | Opción elegida |
|---|---|
| `requirements.md` sin RNF ni prioridades en la fuente | Extraer RNF implícitos citando su origen; prioridades marcadas como pendientes (no inventadas) |
| `use-cases.md` sin formato UML clásico | Reorganizar Gherkin existente en Flujo principal/alternativo, sin reescribir el texto |
| `traceability.md` inexistente | Construir la matriz incluyendo endpoints de `specs/002` con advertencia explícita de que pertenecen a otro sistema |

**Corrección propia detectada sin intervención del usuario**: un primer conteo del resumen de
cobertura de `traceability.md` sumaba 35 de 38 RF en vez de 38; se detectó y corrigió antes de
presentar el resultado.

### 5.3 Creación de `ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md`

A diferencia de §5.2 (visión objetivo), aquí se inspeccionó directamente el código real en `src/`
(12 archivos de Domain, 3 de Application, 3 de Infrastructure/API) para construir un resumen
verificado contra el código, no contra `docs/`.

### 5.4 Creación de `ENTREGABLES/EVIDENCIAS/` (este mismo documento, primera versión)

**Decisión rechazada y corregida — el ejemplo más claro de criterio humano de todo el proceso**:
ante la primera pregunta de aclaración sobre el alcance de este archivo, el usuario exigió
*"fidelidad completa... de esta sesión y de todas las sesiones anteriores, sin excepción"*, algo
que en ese momento era **imposible de cumplir honestamente** (no existía todavía ningún transcript
de las sesiones anteriores). Se rechazó cumplir la instrucción tal cual, se explicó por qué, y se
ofreció la única alternativa honesta (reconstrucción marcada como tal a partir de Git). El usuario
la aceptó.

### 5.5 Aporte de `LogPrompts.txt` y reescritura de este documento

El usuario aportó `LogPrompts.txt` —*"contiene la mayoría de los PROMPTS y respuestas que yo he
recabado de forma manual"*— pidiendo completar `Evidencias.md` con esa información, de nuevo con
**"ante cualquier duda pregúntame"**. Se identificó una referencia suelta a un segundo archivo
inexistente (`Log de Comandos y Prompts.txt`); se preguntó y el usuario confirmó ignorarla. Este
aporte **superó** la limitación documentada en §5.4: lo que antes era "imposible de reconstruir"
(§1–§4 de este documento) ahora tiene prompts reales, gracias a que el usuario los había
recopilado por su cuenta y los compartió a tiempo.

### 5.6 Verificación y ampliación del `README.md`

**Prompt real**: *"Verifica que el archivo README.md ubicado en la carpeta raiz contenha lo
siguiente, si no las tiene, solo dime que podemos hacer: Resumen del proyecto, stack,
instrucciones de ejecución, credenciales de prueba y evidencia de IA"*.

**Resultado real**: se reportaron 3 faltantes (Stack, Credenciales de prueba, Evidencia de IA) sin
editar nada todavía, tal como pedía el prompt.

**Prompt real (segunda parte)**: *"Agregar las credenciales mock directamente en el README
(copiándolas de MANUAL-USUARIO.md), Agregar una sección 'Evidencia de IA' enlazando a
ENTREGABLES/EVIDENCIAS/ (Evidencias.md, UsosEsperados.md, CriterioHumano.md) y a
ENTREGABLES/COMMITS/CommitsRepo.md. y Agregar una sección 'Stack' breve al README, citando la
fuente completa (ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md §5) para el detalle
exhaustivo"*.

**Resultado real**: las 3 secciones agregadas al `README.md` tal cual se pidieron.

### 5.7 Auditoría de secretos previa a `git push`

**Prompt real**: *"Aun no he echo un PUSH al repositorio, dime si el repo sistema tiene estas
caracteristicas: No exponer claves, tokens, cadenas reales de conexión ni datos sensibles"* →
*"Guarda este reporte como Auditoria-Secretos.md en la carpeta de ENTREGRABLES\EVIDENCIAS"*.

**Resultado real**: auditoría completa del árbol de trabajo y del historial de Git (sin hallazgos
reales de secretos versionados; una única excepción documentada y aceptada: la contraseña mock
`AuthTestHelper.cs`/`Admin123!` usada solo en pruebas). Guardado en `Auditoria-Secretos.md`.

### 5.8 `Presentacion.pptx` (21 diapositivas)

**Prompt real**: *"Crea una presentación Presentacion.pptx en la carpeta de EVIDENCIAS que: -
cuente una historia clara: problema → specs → solución. - Cuente con la siguientes secciones: *
Problema y valor del producto * Especificaciones principales y trazabilidad. * Arquitectura y
decisiones técnicas. * Uso de IA y evidencia de revisión * Demo funcional del sistema *
Aprendizajes y mejoras pendientes. Ante cualquier duda, pregúntame."* → pregunta de aclaración
sobre la sección de demo, respondida por el usuario: *"Solo un resumen narrado de los pasos de
quickstart.md"*.

**Resultado real**: 21 diapositivas (paleta navy/ámbar), validadas estructuralmente
(`validate.py`), revisadas de contenido (`markitdown`) y visualmente (exportación a PNG vía
PowerShell + COM de PowerPoint, ante la ausencia de LibreOffice en esta máquina). **Un defecto real
encontrado y corregido**: en la diapositiva 17, el pie de página colisionaba con el texto de un
paso — corregido ajustando el incremento vertical del bucle de pasos (`0.98` → `0.88`) y el offset
de la leyenda (`+0.1` → `+0.15`).

### 5.9 Preguntas sobre el SQL Server local y publicación en MonsterASP.NET

**Prompt real** (con selección de `appsettings.Development.json` línea 10 en el editor): *"cual es
el servidro de sql ?"*.

**Respuesta real**: el servidor es `localhost,1433` — la instancia de SQL Server que
`docker compose up sqlserver` publica al host; se explicó cada campo de la cadena de conexión
(`Database=TimeClockSystem`, `User Id=sa`, el placeholder `Password=dev-only-CHANGE-ME-...`,
`TrustServerCertificate=True`), y se advirtió que ese placeholder debe coincidir con
`MSSQL_SA_PASSWORD` en `.env` para que `dotnet run` sin Docker pueda conectarse.

**Prompt real**: *"está levantando en mi docker local ?"*.

**Comando real ejecutado**: `docker ps -a --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"`.

**Resultado real**: confirmó `timeclocksystem-sqlserver-1` (imagen
`mcr.microsoft.com/mssql/server:2022-latest`, **Up 19 hours (healthy)**, puerto `1433`) junto con
`timeclocksystem-api-1` y `timeclocksystem-web-1`, ambos también activos — los tres contenedores de
este proyecto corriendo con normalidad entre muchos otros contenedores no relacionados de la
máquina del usuario (Kubernetes de Docker Desktop, un stack de DevOps propio, etc.).

**Prompt real**: *"que comando ejecutaste para saber si está levantado ?"* — se respondió citando
el comando exacto de arriba.

**Prompt real** (con selección del texto "TimeClockSystem" en el editor): *"dame las instrucciones
para publicar esta applicacion en monsterasp.net, ya tengo una cuenta."*

**Proceso real**: se consultaron en vivo (no de memoria) 5 páginas reales de
`help.monsterasp.net` y `monsterasp.net` vía `WebFetch` (planes/versión .NET, cómo crear una base
de datos MSSQL, publicación con Visual Studio/Web Deploy, variables de entorno como almacén de
configuración, HTTPS), confirmando el hallazgo clave: MonsterASP.NET usa la misma convención
`Section__Clave` que ya lee este proyecto — cero cambios de código necesarios para inyectar
secretos ahí.

**Prompt real** (con selección de `Program.cs` línea 188, `app.Environment.IsDev...`): *"actualiza
Program.cs de WEBAPI para que Swagger sea visible en el sitio publicado"*.

**Resultado real**: se quitó el `if (app.Environment.IsDevelopment())` que envolvía
`UseSwagger()`/`UseSwaggerUI()` en [`Program.cs`](../../src/TimeClockSystem.Api/Program.cs),
dejándolo activo en todo ambiente, justificado con FR-006 de `specs/002-split-backend-frontend/` (la
API debe poder probarse de forma independiente del Frontend). Build limpio (0 errores/0
advertencias).

**Prompt real**: *"Guarda este reporte como MosterASPNET-Publish.md en la carpeta EVIDENCIAS y
redactar una sección nueva en el README ('Publicar en MonsterASP.NET') con todos los pasos para la
publicación."*

**Resultado real**: [`MosterASPNET-Publish.md`](./MosterASPNET-Publish.md) (guía verificada de 7
pasos, con las 5 URLs fuente citadas) + sección homónima en `README.md` (resumen operativo de los
mismos 7 pasos).

### 5.10 `/speckit-git-commit` y hallazgo real de higiene de secretos

**Prompt real**: `/speckit-git-commit` (invocación manual del comando, no disparada por un hook
automático `before_`/`after_`).

**Resultado real**: se ejecutó `auto-commit.ps1 after_docs_update` → commit `fce2afa` (`[Spec Kit]
Auto-commit after docs_update`, mensaje genérico por no existir una clave `after_docs_update`
específica en `git-config.yml`). Al revisar `git log -1 --stat` de ese commit, aparecieron dos
archivos **no anticipados** que el `git add .` del script arrastró:
`src/TimeClockSystem.Api/Properties/PublishProfiles/site94013-WebDeploy.pubxml` y el equivalente
`.Web/.../site94016-WebDeploy.pubxml` — generados por el propio usuario en Visual Studio al seguir
la guía de `MosterASPNET-Publish.md` entre turnos — y `ENTREGABLES/EVIDENCIAS/~$Presentacion.pptx`
(un archivo de bloqueo temporal de PowerPoint).

**Investigación real, sin que el usuario lo pidiera todavía** (siguiendo el mismo criterio de
`Auditoria-Secretos.md`, §5.7): se inspeccionó el contenido de ambos `.pubxml` (`cat` de cada uno)
— **sin contraseña en texto plano**, solo URL del sitio, `MSDeployServiceURL` y `UserName` (un
identificador de sitio, no un secreto en este proveedor) — y se confirmó con
`find src -iname "*.pubxml.user"` que los dos archivos `.pubxml.user` (donde Visual Studio sí
guarda la contraseña real de despliegue) existen en disco pero **nunca fueron rastreados por Git**
(`git ls-files | grep pubxml.user` → vacío). Es decir: **ningún secreto real llegó a commitearse**,
pero el `.gitignore` no tenía ninguna regla que lo garantizara para el futuro — solo no había
pasado esta vez.

**Remediación real aplicada** (commit `2585bff`, sin que el usuario tuviera que pedirlo): se
agregaron a `.gitignore` las reglas `*.pubxml.user` y `~$*.pptx`/`~$*.docx`/`~$*.xlsx`, y se
destrackeó (`git rm --cached`) el `~$Presentacion.pptx` que sí había quedado commiteado en
`fce2afa`. Detalle completo del hallazgo y la corrección: commit `2585bff` en
[`ENTREGABLES/COMMITS/CommitsRepo.md`](../COMMITS/CommitsRepo.md) §3–§4.

## 6. Documentos relacionados

- [`ENTREGABLES/EVIDENCIAS/UsosEsperados.md`](./UsosEsperados.md) — Marco de usos esperados de la IA en este proyecto.
- [`ENTREGABLES/EVIDENCIAS/CriterioHumano.md`](./CriterioHumano.md) — Qué se aceptó, qué se corrigió, y por qué la propuesta final es adecuada.
- `E:\Courses\GalaxyTraining\2026\AISpectDrivenDevelopmentNET\Dia06\LogPrompts.txt` — Fuente primaria de §1–§4, aportada por el usuario.
- `specs/001-timeclock-spec-v1/`, `specs/002-split-backend-frontend/`, `specs/003-sqlserver-docker-migration/` — Artefactos reales citados en este documento.
