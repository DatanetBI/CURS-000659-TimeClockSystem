<!--
Sync Impact Report
- Version change: (sin versión previa / plantilla sin rellenar) → 1.0.0
- Modified principles:
  - [PRINCIPLE_1_NAME] → I. Simplicidad Ante Todo
  - [PRINCIPLE_2_NAME] → II. Idioma y Mercado (Español de México)
  - [PRINCIPLE_3_NAME] → III. Cero Alcance Fantasma
  - [PRINCIPLE_4_NAME] → IV. Verificable por una Persona No Técnica
  - [PRINCIPLE_5_NAME] → V. Datos del Usuario: Mínimos y Sin Secretos
- Added sections:
  - Estándares de Producto y Seguridad (antes [SECTION_2_NAME])
  - Flujo de Trabajo de Desarrollo (antes [SECTION_3_NAME])
  - Governance (reglas de enmienda, versionado y cumplimiento redactadas)
- Removed sections: ninguna
- Templates requiring follow-up: ninguno modificado por este comando (los templates dependientes
  leen la constitución en tiempo de ejecución y no se tocan aquí)
- Deferred TODOs: ninguno
-->

# TimeClockSystem Constitution

## Core Principles

### I. Simplicidad Ante Todo
Ante dos soluciones que resuelven el mismo problema, el equipo MUST elegir siempre la más
simple. Por tratarse de una versión inicial (MVP), MUST NOT diseñarse ni construirse
complejidad anticipada: sin capas de abstracción, configuraciones o mecanismos de extensión
pensados para necesidades futuras hipotéticas que no estén en la spec actual.

**Rationale**: Mantener el sistema fácil de entender, mantener y entregar rápido, evitando
sobre-ingeniería antes de validar el producto con usuarios reales.

### II. Idioma y Mercado (Español de México)
Todo el producto —interfaz de usuario, mensajes de error, correos, documentación visible al
usuario y reportes— MUST estar redactado en español de México. Todos los valores monetarios
MUST expresarse, calcularse y mostrarse en Pesos Mexicanos (MXN).

**Rationale**: El producto está dirigido a empresas y empleados en México; la consistencia de
idioma y moneda evita ambigüedad y errores de interpretación legal o financiera.

### III. Cero Alcance Fantasma
MUST NOT implementarse ninguna funcionalidad, campo, pantalla o regla de negocio que no esté
explícitamente escrita en la especificación (spec) vigente. Si durante el desarrollo surge una
idea nueva o una mejora, esta se documenta como propuesta separada para una futura iteración;
no se construye dentro del alcance actual.

**Rationale**: Previene la expansión descontrolada del alcance (scope creep), mantiene los
tiempos de entrega predecibles y asegura que lo construido sea exactamente lo acordado.

### IV. Verificable por una Persona No Técnica
Cada criterio de aceptación de cada funcionalidad MUST poder comprobarse usando la aplicación
(interfaz de usuario, correos recibidos, reportes exportados, etc.), sin necesidad de leer
código, consultar la base de datos directamente ni ejecutar comandos técnicos.

**Rationale**: Garantiza que cualquier interesado del negocio pueda validar que una
funcionalidad está completa, y obliga a que las specs sean claras y orientadas a comportamiento
observable.

### V. Datos del Usuario: Mínimos y Sin Secretos
El sistema MUST solicitar únicamente los datos estrictamente necesarios para operar la
funcionalidad en cuestión; no se piden datos "por si acaso". Ninguna clave, contraseña, token o
secreto MUST introducirse directamente en el código fuente; estos se gestionan mediante
configuración externa (variables de entorno, gestor de secretos, etc.).

**Rationale**: Reduce el riesgo de privacidad para los empleados y el riesgo de seguridad para
la empresa, y evita fugas de credenciales en el control de versiones.

## Estándares de Producto y Seguridad

- Toda nueva pantalla, mensaje o reporte MUST pasar por una revisión de idioma (español de
  México) y formato de moneda (MXN) antes de considerarse terminado.
- Ninguna clave, contraseña, cadena de conexión o token MUST aparecer en texto plano en el
  repositorio; MUST gestionarse vía variables de entorno o un gestor de secretos.
- Todo formulario o pantalla que capture datos de empleados MUST justificar cada campo
  solicitado en la spec correspondiente; los campos no usados por ninguna funcionalidad activa
  MUST eliminarse.

## Flujo de Trabajo de Desarrollo

- Ninguna tarea de implementación MUST iniciar sin una spec aprobada que describa el
  comportamiento esperado y sus criterios de aceptación verificables manualmente.
- Toda propuesta de funcionalidad fuera del alcance de la spec activa MUST documentarse por
  separado (por ejemplo, como una nueva spec o issue) en lugar de implementarse de forma
  oportunista.
- Antes de cerrar una tarea, MUST validarse manualmente en la aplicación que cada criterio de
  aceptación se cumple, sin depender de la revisión de código como única prueba.

## Governance

Esta constitución prevalece sobre cualquier otra práctica, guía o convención del proyecto.
Cualquier decisión de diseño o implementación que la contradiga MUST justificarse
explícitamente y ser aprobada antes de proceder.

**Enmiendas**: Cualquier cambio a esta constitución MUST proponerse por escrito (pull request o
documento equivalente), indicando el principio afectado y la razón del cambio. Los cambios se
documentan en el Sync Impact Report al inicio de este archivo.

**Versionado**: Esta constitución sigue versionado semántico (MAJOR.MINOR.PATCH): MAJOR para
eliminación o redefinición incompatible de principios existentes; MINOR para adición de nuevos
principios o secciones, o expansión material de guía existente; PATCH para aclaraciones,
correcciones de redacción y cambios no semánticos.

**Revisión de cumplimiento**: Toda spec, plan y lista de tareas generada por Spec Kit MUST
verificarse contra estos principios antes de pasar a implementación. Cualquier violación MUST
resolverse o justificarse explícitamente en el documento correspondiente antes de continuar.

**Version**: 1.0.0 | **Ratified**: 2026-09-22 | **Last Amended**: 2026-09-22
