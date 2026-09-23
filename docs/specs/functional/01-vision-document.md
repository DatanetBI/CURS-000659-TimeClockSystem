# Documento de Visión — Time Clock System

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Estado | Borrador para revisión |
| Autor | Lead Requirements Engineer |
| Fecha | 2026-09-21 |
| Audiencia | Product Owners, RRHH, TI, Legal/Compliance, Comité de Nómina |

---

## 1. Propósito del documento

Este documento establece la visión funcional del **Time Clock System (TCS)**: el problema de negocio que resuelve, los objetivos medibles del proyecto, los perfiles de usuario que interactúan con la solución y el alcance de los nueve módulos funcionales que la componen. Sirve como marco de referencia para las especificaciones detalladas (historias de usuario, reglas de negocio y diagramas) que se derivan de él.

## 2. Contexto y problema de negocio

Las organizaciones con fuerza laboral distribuida (oficina, planta, campo y teletrabajo) enfrentan tres problemas recurrentes:

1. **Captura de asistencia fragmentada**: relojes biométricos físicos no cubren a personal remoto o de campo, generando marcas manuales propensas a error y fraude (compañerismo/buddy punching).
2. **Pre-nómina manual y propensa a error**: el cálculo de horas extra, recargos, atrasos y ausencias se realiza hoy con hojas de cálculo, generando reprocesos, reclamos laborales y riesgo de incumplimiento normativo.
3. **Falta de trazabilidad y autoservicio**: los empleados no tienen visibilidad de su propio historial de asistencia, y RRHH concentra tareas operativas (correcciones, consultas, aprobaciones) que podrían resolverse mediante autoservicio.

El TCS centraliza la captura, el cálculo y la gestión de excepciones de asistencia, integrándose con los sistemas de Nómina/ERP y HRIS existentes, bajo un marco de seguridad y cumplimiento normativo (incluyendo protección de datos biométricos).

## 3. Objetivos del proyecto

| # | Objetivo | Métrica de éxito (KPI) |
|---|---|---|
| O1 | Unificar la captura de marcas en todos los canales (biometría, RFID, web, móvil) | ≥ 95% de empleados activos con al menos un canal de marcaje configurado |
| O2 | Automatizar el cálculo de pre-nómina (horas extra, recargos, ausencias) | Reducción ≥ 80% del tiempo de procesamiento manual de pre-nómina por ciclo |
| O3 | Reducir el fraude de marcaje (suplantación, marcas fuera de perímetro) | Detección y bloqueo de ≥ 99% de intentos de suplantación facial (liveness) |
| O4 | Habilitar autoservicio para empleados y supervisores | ≥ 70% de solicitudes de permisos/ajustes gestionadas sin intervención directa de RRHH |
| O5 | Garantizar cumplimiento normativo y auditabilidad | 100% de modificaciones manuales sobre marcas/permisos/horarios registradas en bitácora inalterable |
| O6 | Integrar de forma automatizada con ERP/Nómina y HRIS | Exportación de horas consolidadas sin intervención manual en ≥ 95% de los ciclos de nómina |
| O7 | Operar de forma confiable en zonas de baja conectividad | 100% de marcas offline sincronizadas correctamente al recuperar conexión, sin pérdida de datos |

## 4. Alcance funcional

El sistema se organiza en tres capas y nueve módulos funcionales:

```
┌────────────────────────────────────────────────────────────────────────┐
│                   SISTEMA DE CONTROL DE ASISTENCIA                     │
├───────────────────┬────────────────────┬───────────────────────────────┤
│    CAPTURA Y UI   │  MOTOR MÓDULOS HR  │    INTEGRACIÓN Y AUDITORÍA    │
├───────────────────┼────────────────────┼───────────────────────────────┤
│ • Dispositivos/   │ • Horarios y       │ • Integración ERP / Nómina    │
│   Biometría       │   Turnos           │ • Auditoría y Seguridad       │
│ • Portal ESS      │ • Pre-Nómina y     │ • Business Intelligence       │
│   (Empleado)      │   Reglas           │   y Reportes                  │
│ • Portal MSS      │ • Incidencias y    │                               │
│   (Supervisor)    │   Permisos         │                               │
└───────────────────┴────────────────────┴───────────────────────────────┘
```

| Capa | Módulo | Documento de referencia |
|---|---|---|
| Captura y UI | 1. Gestión de Marcas y Marcaje (Clocking & Attendance Capture) | 02-user-stories.md §2.1 |
| Motor Módulos HR | 2. Gestión de Horarios y Turnos (Scheduling & Shift Management) | 02-user-stories.md §2.2 |
| Motor Módulos HR | 3. Pre-Nómina y Motor de Reglas (Calculations & Pre-Payroll Engine) | 02-user-stories.md §2.3 |
| Motor Módulos HR | 4. Incidencias, Justificaciones y Permisos (Absence Management) | 02-user-stories.md §2.4 |
| Captura y UI | 5. Portal del Empleado y Autoservicio (ESS) | 02-user-stories.md §2.5 |
| Captura y UI | 6. Portal del Supervisor / Manager (MSS) | 02-user-stories.md §2.5 |
| Integración y Auditoría | 7. Integración y Exportación (Integrations & Payroll API) | 02-user-stories.md §2.6 |
| Integración y Auditoría | 8. Reportes y Business Intelligence (Analytics & Audit) | 02-user-stories.md §2.6 |
| Integración y Auditoría | 9. Administración, Seguridad y Auditoría (Admin & Compliance) | 02-user-stories.md §2.6 |

### 4.1 Fuera de alcance (release inicial)

- Cálculo de nómina completo (bruto a neto, deducciones legales, impuestos): el TCS **exporta** insumos calculados; el cálculo de nómina final permanece en el ERP/Nómina destino.
- Gestión de reclutamiento, evaluación de desempeño u otros módulos de HRIS no relacionados con asistencia.
- Emisión de dispositivos biométricos físicos (el sistema integra hardware de terceros vía SDK/API, no fabrica hardware).
- Negociación o modelado de convenios colectivos complejos fuera de las reglas parametrizables de horas extra/recargos definidas en el Módulo 3.

### 4.2 Supuestos y restricciones

- La legislación laboral aplicable (clasificación de horas extra diurnas/nocturnas/festivas/dominicales, topes de jornada) es parametrizable por país/entidad legal, dado que la organización opera en múltiples jurisdicciones.
- El tratamiento de datos biométricos requiere consentimiento explícito y cumplimiento de normativa de protección de datos equivalente a GDPR (o la ley local de datos personales/biométricos aplicable).
- La conectividad en campo puede ser intermitente; toda funcionalidad de captura móvil debe tolerar operación offline con sincronización diferida.
- El sistema debe interoperar con al menos un ERP/Nómina (ej. SAP, Oracle, Workday, Softland) y un proveedor de identidad (LDAP/Active Directory) ya existentes en la organización.

## 5. Perfiles de usuario (actores)

| Perfil | Descripción | Objetivos principales | Módulos que utiliza principalmente |
|---|---|---|---|
| **Empleado (Employee)** | Colaborador operativo, administrativo, de campo o teletrabajador sujeto a control de asistencia | Registrar marcas correctamente, consultar su historial, solicitar permisos/vacaciones, corregir omisiones | 1, 5 |
| **Supervisor / Jefatura (Manager)** | Líder de equipo o de centro de costo responsable de la cobertura operativa | Aprobar/rechazar solicitudes, monitorear presencia en tiempo real, reasignar turnos ante ausencias | 4, 6, 2 |
| **Analista de Recursos Humanos (HR Analyst)** | Responsable de la administración de asistencia a nivel organizacional | Configurar turnos, tolerancias, revisar incidencias, preparar la pre-nómina | 2, 3, 4, 8 |
| **Encargado de Nómina (Payroll Specialist)** | Responsable de procesar el pago de remuneraciones | Consumir el consolidado de horas/incidencias calculado, validar exportaciones al ERP | 3, 7, 8 |
| **Administrador del Sistema (System Administrator)** | Responsable de la configuración técnica y de seguridad del TCS | Gestionar dispositivos, geofencing, roles y permisos, integración con directorio activo | 1, 7, 9 |
| **Oficial de Cumplimiento / Auditor de TI (Compliance Officer / IT Auditor)** | Responsable de verificar el cumplimiento normativo y la integridad de los datos | Consultar bitácoras de auditoría inalterables, verificar consentimientos biométricos | 9, 8 |
| **Especialista de TI / Integraciones (IT Integration Specialist)** | Responsable de la interoperabilidad con otros sistemas corporativos | Configurar y monitorear conectores ERP/HRIS, resolver incidentes de sincronización | 7 |
| **Ejecutivo / Gerencia (Executive Sponsor)** | Consumidor de reportes estratégicos sobre costo laboral y cumplimiento | Visualizar KPIs de ausentismo, costo de horas extra y tendencias por departamento | 8 |

## 6. Principios de diseño funcional

1. **Multicanal por diseño**: toda regla de negocio de captura (geofencing, liveness, tolerancia) debe aplicarse de forma consistente sin importar el canal (biometría física, RFID, web, móvil).
2. **Offline-first en captura móvil**: la app móvil nunca debe bloquear el registro de una marca por falta de conectividad; la validación de reglas de servidor ocurre en la sincronización.
3. **Trazabilidad total**: ninguna modificación manual a una marca, permiso o turno puede persistir sin quedar registrada en la bitácora de auditoría (quién, cuándo, qué valor previo/nuevo).
4. **Configurabilidad normativa**: las reglas de horas extra, recargos y tolerancias son datos de configuración, no lógica de código, para adaptarse a distintas legislaciones y convenios.
5. **Autoservicio como primera opción**: toda consulta o solicitud que no requiera juicio de RRHH (consulta de saldo, marcaje, solicitud de permiso) debe estar disponible en ESS/MSS antes de requerir intervención manual.
6. **Privacidad por diseño**: la captura de datos biométricos y de geolocalización requiere consentimiento explícito, minimización de datos y políticas de retención definidas.

## 7. Glosario

| Término | Definición |
|---|---|
| Marcaje / Marca | Registro de un evento de entrada, salida, inicio o fin de receso |
| Geofencing | Validación de que una marca ocurre dentro de un perímetro geográfico autorizado |
| Liveness Detection | Técnica anti-spoofing que confirma que el reconocimiento facial corresponde a una persona presente en vivo, no a una foto/video |
| Pre-nómina | Consolidado de horas trabajadas, extras, recargos e incidencias, previo al cálculo final de nómina |
| ESS | Employee Self-Service — Portal de autoservicio del empleado |
| MSS | Manager Self-Service — Portal de autoservicio del supervisor |
| Tolerancia / Gracia | Margen de minutos permitido en una marca sin generar incidencia |
| On-call | Turno de disponibilidad donde el empleado no trabaja activamente pero debe estar localizable |
| RBAC | Role-Based Access Control — control de acceso basado en roles |
| Audit Log | Bitácora de auditoría inalterable de cambios sobre datos críticos |

## 8. Documentos relacionados

- `02-user-stories.md` — Historias de usuario en formato Gherkin por módulo, con diagramas de secuencia de los flujos críticos.
