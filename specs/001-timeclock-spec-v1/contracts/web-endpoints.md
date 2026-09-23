# Mapa de acciones de la aplicación (interno)

Resumen de las acciones principales por área funcional, para guiar `/speckit-tasks`. No es un contrato
externo (la aplicación no expone una API pública para terceros en v1, salvo el archivo de
exportación/importación descrito en `export-import.md`); es la referencia de qué páginas/acciones debe
tener el único proyecto ASP.NET Core.

| Área (carpeta) | Acción | Requisitos cubiertos |
|---|---|---|
| Marcaje | Registrar marca (entrada/salida/receso) — web, o por número de empleado + PIN | FR-001 a FR-010 |
| Marcaje | Asignar/restablecer PIN de un empleado (RRHH/Admin) | FR-004 |
| Turnos | CRUD de catálogo de turnos | FR-011, FR-015 |
| Turnos | Asignación individual y masiva de turnos/calendarios | FR-013, FR-014 |
| Turnos | Configurar factores de pago y rangos horarios (RRHH) | FR-018 |
| Turnos | Ver/recalcular consolidado de un periodo | FR-016, FR-019 |
| Solicitudes | Crear solicitud (vacaciones/permiso/incapacidad/compensatorio) | FR-020 a FR-022 |
| Solicitudes | Aprobar/rechazar (supervisor, RRHH) | FR-023, FR-025 |
| Solicitudes | Escalamiento automático por vencimiento de plazo | FR-024 |
| Supervisor | Panel de presencia del equipo (polling) | FR-031, FR-032 |
| Supervisor | Aprobar/rechazar solicitudes del equipo | FR-033 |
| Supervisor | Reasignar cobertura de turno | FR-034, FR-035 |
| Empleado | Historial de marcas, incidencias, saldo de vacaciones | FR-027, FR-028 |
| Empleado | Marca omitida → iniciar corrección | FR-029 |
| Empleado | Notificaciones (marcas omitidas, resolución de solicitudes) | FR-030 |
| Integraciones | Disparar/consultar exportación a nómina | FR-036, FR-037, FR-039 |
| Integraciones | Subir archivo de altas/bajas | FR-038, FR-039 |
| Reportes | Reportes operativos (asistencia, ausentismo, horas extra) + KPIs | FR-040, FR-041 |
| Reportes | Exportar reporte (PDF/Excel/CSV) | FR-042 |
| Reportes | Consultar y filtrar bitácora de auditoría | FR-044 a FR-047 |
| Administración | Gestión de roles y permisos | FR-043 |
| Administración | Consentimiento de geolocalización/biometría | FR-046 |

Cada fila de esta tabla se convierte en una o más tareas concretas (controlador/vista/prueba) en
`tasks.md` cuando se ejecute `/speckit-tasks`.
