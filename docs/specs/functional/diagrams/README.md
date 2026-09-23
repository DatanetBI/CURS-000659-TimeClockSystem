# Diagramas de Secuencia — Time Clock System

Fuente independiente (`.mmd`) y renderizado (`.svg`) de los diagramas de secuencia embebidos en `../02-user-stories.md`. Cada `.mmd` es la fuente de verdad editable; el `.svg` es un artefacto generado para revisión visual rápida.

| Archivo | Historia | Flujo | Sección |
|---|---|---|---|
| `US-002_marcaje-biometrico-geofencing.mmd` | US-002 | Marcaje biométrico con geofencing y liveness detection | 2.1 Clocking |
| `US-003_offline-sync.mmd` | US-003 | Marcaje offline y sincronización automática | 2.1 Clocking |
| `US-006_calculo-horas-extra.mmd` | US-006 | Cálculo automático de horas extra y recargos | 2.3 Pre-Payroll Engine |
| `US-009_workflow-aprobacion.mmd` | US-009 | Workflow de aprobación de incidencias (2 niveles) | 2.4 Absence Management |
| `US-012_integracion-erp.mmd` | US-012 | Exportación e integración con ERP/Nómina | 2.6 Integraciones |
| `US-013_bitacora-auditoria.mmd` | US-013 | Bitácora de auditoría inalterable (append-only) | 2.6 Auditoría |

## Regenerar los `.svg`

```bash
cd docs/specs/functional/diagrams
for f in *.mmd; do
  npx @mermaid-js/mermaid-cli -i "$f" -o "${f%.mmd}.svg"
done
```

Ver `../VALIDATION.md` para la bitácora de validación sintáctica de estos diagramas.

> Nota: si `mmdc` falla con `ENOENT ... mermaid.esm.mjs`, el paquete quedó parcialmente instalado en la caché de `npx`. Solución: borrar la carpeta de esa versión bajo `%LOCALAPPDATA%\npm-cache\_npx\` y volver a ejecutar el comando.
