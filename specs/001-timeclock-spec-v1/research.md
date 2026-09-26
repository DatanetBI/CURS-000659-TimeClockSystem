# Research & Decisiones — TimeClockSystem v1.0

Cada decisión resuelve una pregunta abierta del Technical Context de `plan.md`, para el alcance reducido
de 6 módulos (ver `plan.md` §"Alcance de v1.0 vs. v1.1"). Todas están redactadas primero en lenguaje de
negocio y luego con el detalle técnico necesario para diseñar `data-model.md` y `contracts/`.

---

## 1. Arquitectura de despliegue: una aplicación vs. microservicios

**Decisión**: Un único proyecto ASP.NET Core desplegable como un solo contenedor/proceso. *(Sin cambios
respecto a la revisión anterior de este documento.)*

**En palabras de negocio**: Con solo 6 módulos en esta primera entrega, construir 9 microservicios sería
aún menos justificable que antes. Un solo despliegue permite publicar en línea de inmediato.

**Rationale técnico**: Ver historial de este documento (versión anterior) para la comparación completa
contra `docs/architecture/c4-containers.md`/`ADR-001`. Ninguna conclusión cambia con el alcance reducido.

---

## 2. Almacenamiento de datos

**Decisión**: SQLite (archivo único) vía Entity Framework Core. *(Sin cambios.)*

**En palabras de negocio**: No hay que contratar ni administrar un servidor de base de datos aparte.

---

## 3. Autenticación y control de acceso — simplificado a 2 roles

**Decisión**: ASP.NET Core Identity con exactamente dos roles en v1.0: **Empleado** y **Administrador**.
El marcaje por número de empleado + PIN (módulo 5) sigue siendo un flujo separado y más ligero, no una
sesión de portal completa.

**En palabras de negocio**: En v1.0 no existen todavía aprobaciones (supervisor/RRHH), exportaciones
(TI/nómina) ni auditoría (auditor) — por lo tanto no hay ninguna pantalla que esos roles necesiten usar
todavía. Se agregan cuando se construya la funcionalidad que los requiere, en vez de crear permisos para
pantallas que no existen.

**Rationale técnico**: `Microsoft.AspNetCore.Identity` con `IdentityRole` para "Empleado" y
"Administrador"; el PIN de marcaje se guarda con hash en un campo separado de la contraseña de portal.

**Alternativas consideradas**: Implementar los 8 roles de FR-043 desde ahora — descartado porque
anticiparía permisos y menús para funciones que no se construyen hasta v1.1 (viola el Principio I).

---

## 4. Portal de consulta de asistencias — sin tiempo real

**Decisión**: El módulo 6 (Portal de consulta) es una pantalla de filtro/búsqueda tradicional (el
administrador elige empleado/fecha/centro de trabajo y ve una tabla de resultados al enviar el filtro).
No hay actualización automática ni sondeo (*polling*) en vivo.

**En palabras de negocio**: El panel de presencia en tiempo real (para que un supervisor vea quién está
trabajando "ahora mismo") es una función distinta, ligada al rol de supervisor y a la reasignación de
cobertura — ambas diferidas a v1.1. El administrador de v1.0 necesita **consultar** asistencias ya
registradas, no monitorear presencia en vivo; una pantalla de consulta estándar resuelve eso sin
necesitar ninguna infraestructura de actualización en tiempo real.

**Alternativas consideradas**: Sondeo cada 5-8 segundos (decisión de la versión anterior de este
documento) — ya no aplica, porque el caso de uso que lo justificaba (panel de presencia del supervisor,
User Story 4) está diferido a v1.1.

---

## 5. Marcaje: solo en línea, sin modo offline en v1.0

**Decisión**: El módulo 5 (Registro de asistencias) requiere conexión a internet en el momento de
marcar. Si la petición falla (sin red, o el servidor no responde), se muestra un error y se le pide al
empleado reintentar — no se guarda nada localmente ni se sincroniza después.

**En palabras de negocio**: El caso del empleado de campo sin señal (FR-008/FR-009 de `spec.md`) sigue
siendo parte de la visión completa del producto, pero se pospone a v1.1 para poder publicar la versión en
línea más rápido. v1.0 cubre el caso principal: un empleado con conexión a internet marca su asistencia
y la ve confirmada de inmediato.

**Rationale técnico**: Sin cola offline, el flujo de marcaje es una sola llamada síncrona
petición-respuesta; no se necesita JavaScript de reintento, `localStorage` ni ninguna lógica de
sincronización en el cliente. Esto es una simplificación real del código de v1.0, no solo de la
infraestructura.

**Alternativas consideradas**: `localStorage` + reintento automático (decisión de la versión anterior de
este documento) — se pospone a v1.1 junto con el resto del soporte offline, por instrucción explícita del
negocio para esta revisión del plan.

---

## 6. Reconocimiento facial / liveness (FR-007)

**Decisión**: Sin cambios — se define una interfaz de extensión (`IBiometricVerificationProvider`) con
una implementación por defecto que no aplica esa validación, sin integrar ningún proveedor real.

**En palabras de negocio**: No cuesta nada dejar la puerta abierta para un proveedor biométrico futuro;
no se construye ni se paga por esa pieza ahora.

---

## 7. Exportación a nómina e importación de altas/bajas — diferido por completo

**Decisión**: Ninguna funcionalidad de exportación/importación se construye en v1.0. No se agrega ningún
`BackgroundService` ni motor de trabajos en segundo plano en esta versión.

**En palabras de negocio**: Sin este módulo, no hay ninguna tarea programada que ejecutar todavía; se
elimina así una pieza más de infraestructura que la revisión anterior de este plan sí incluía. Se
construye junto con el módulo de Exportación/Importación en v1.1.

**Alternativas consideradas**: Ninguna — el módulo completo está fuera de v1.0 por instrucción explícita
del negocio (ver `plan.md` §"Diferido explícitamente a v1.1").

---

## 8. Turnos: catálogo y asignación, sin motor de horas extra

**Decisión**: El módulo 3 (Turnos) permite crear turnos (horario, receso, tolerancia) y asignarlos a
empleados (individual o masivamente). No calcula horas extra, no clasifica tipos de hora (diurna,
nocturna, festiva, dominical) ni reparte automáticamente las horas de un turno que cruza la medianoche
entre dos fechas.

**En palabras de negocio**: Ese cálculo (el "motor de pre-nómina") es exactamente lo que alimenta la
exportación a nómina, que está diferida — construirlo ahora sería adelantar trabajo para un consumidor
(la exportación) que todavía no existe. v1.0 solo necesita saber qué turno tiene cada empleado para que
el Portal de consulta (módulo 6) pueda mostrar, de forma simple, si una marca fue puntual o tardía
comparando la hora de la marca contra la hora de entrada y la tolerancia del turno — sin necesitar el
resto del motor de reglas legales.

**Alternativas consideradas**: Implementar FR-016 a FR-019 completos desde v1.0 — descartado, ese cálculo
depende de Incidencias (para saber qué ausencias están justificadas) y del calendario de festivos con
factores de pago, ambos fuera de esta entrega.

---

## 9. Días festivos: catálogo simple

**Decisión**: El módulo 4 (Días festivos) es un catálogo CRUD de fechas festivas, sin factores de pago
asociados. El Portal de consulta (módulo 6) lo usa únicamente para mostrar, junto a cada marca, si ese
día era festivo — un dato informativo, no un cálculo de "hora festiva pagada distinto".

**En palabras de negocio**: Permite que el administrador ya empiece a mantener su calendario de festivos
desde v1.0 (trabajo que no se perderá cuando se construya el motor de pre-nómina en v1.1), sin necesitar
todavía las reglas de pago que dependen de ese calendario.

---

## 10. Datos de ejemplo (mock) para la prueba de concepto

**Decisión**: Un `DbSeeder` se ejecuta al iniciar la aplicación (si la base de datos está vacía) y crea
los datos descritos en `plan.md` §"Datos mock para la prueba de concepto": centros de trabajo, usuarios
administradores, empleados con PIN, turnos, festivos y algunas marcas de ejemplo.

**En palabras de negocio**: Cualquier persona del negocio puede entrar a la aplicación recién publicada y
probar de inmediato los 6 módulos, sin dar de alta manualmente decenas de registros antes de poder
evaluar el sistema.

**Rationale técnico**: `DbSeeder` corre en el arranque del proceso, revisa si ya existen datos y, si no,
inserta el conjunto de ejemplo dentro de una transacción; es idempotente.

---

## 11. Responsividad móvil

**Decisión**: Sin cambios — Bootstrap 5 vía CDN, layout "mobile-first"; sin app nativa ni frontend
separado.

---

## Resumen de resolución de "NEEDS CLARIFICATION"

Ninguna casilla del Technical Context de `plan.md` quedó como `NEEDS CLARIFICATION`. Las decisiones 1, 2,
6, 10 y 11 se heredan sin cambios de la revisión anterior; las decisiones 3, 4, 5, 7, 8 y 9 se ajustaron
para el alcance reducido de 6 módulos de esta revisión.
