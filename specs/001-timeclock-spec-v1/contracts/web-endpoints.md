# Mapa de acciones de la aplicación — v1.0 (6 módulos)

Resumen de las acciones principales por módulo, en el mismo orden de construcción de `plan.md`, para
guiar `/speckit-tasks`. No hay API pública para terceros en v1.0 (la Exportación/Importación está
diferida — ver `export-import.md`).

| # | Módulo (carpeta) | Acción | Requisitos cubiertos | Rol |
|---|---|---|---|---|
| 1 | CentrosTrabajo | CRUD de centro de trabajo (nombre, coordenadas, radio) | Soporta FR-006 | Administrador |
| 2 | Empleados | CRUD de empleado (número, nombre, centro de trabajo, estado) | Entidad Empleado | Administrador |
| 2 | Empleados | Asignar/restablecer PIN de marcaje de un empleado | FR-004 | Administrador |
| 3 | Turnos | CRUD de catálogo de turnos (horario, receso, tolerancia) | FR-011, FR-015 | Administrador |
| 3 | Turnos | Asignación individual y masiva de turnos a empleados | FR-013, FR-014 | Administrador |
| 4 | DiasFestivos | CRUD de calendario de festivos | Soporta clasificación informativa en módulo 6 | Administrador |
| 5 | Marcaje | Registrar marca (entrada/salida/receso) desde el portal, con geolocalización | FR-001, FR-005, FR-006, FR-010 | Empleado |
| 5 | Marcaje | Registrar marca por número de empleado + PIN (modo kiosco) | FR-002, FR-003 | Empleado (sin sesión de portal) |
| 6 | ConsultaAsistencias | Filtrar/consultar marcas por empleado, fecha o centro de trabajo, con indicador puntual/tardío y día festivo | Versión acotada de US5/US7 | Administrador |

## Fuera de v1.0

Ver `plan.md` §"Diferido explícitamente a v1.1" para la lista completa (Incidencias, Exportación/
Importación, offline, motor de pre-nómina, panel de presencia en tiempo real, reportes/KPIs exportables,
bitácora de auditoría, roles adicionales).
