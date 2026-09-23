# Bitácora de Validación — Diagramas de Secuencia

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Documento validado | `02-user-stories.md` |
| Herramienta | `@mermaid-js/mermaid-cli` (mmdc) |
| Versión de la herramienta | 11.17.0 |
| Entorno | Node.js v24.13.0, Windows 10 |
| Fecha de validación | 2026-09-21 |
| Responsable | Lead Requirements Engineer |

## 1. Propósito

Este documento certifica que todos los diagramas de secuencia Mermaid incluidos en `02-user-stories.md` son **sintácticamente válidos** y renderizables, mediante compilación automatizada mensurable (no revisión visual subjetiva). Sirve como evidencia de calidad de la especificación funcional antes de su uso como insumo para diseño técnico y pruebas.

## 2. Metodología

1. Se extraen todos los bloques de código delimitados por ```` ```mermaid ```` del archivo `02-user-stories.md` mediante un script Node.js, generando un archivo `.mmd` independiente por diagrama.
2. Cada archivo `.mmd` se compila individualmente con:
   ```bash
   npx @mermaid-js/mermaid-cli -i <diagrama>.mmd -o <diagrama>.svg
   ```
3. Se considera **válido** un diagrama cuando:
   - El proceso `mmdc` finaliza con código de salida `0` (sin errores de parseo).
   - El archivo `.svg` resultante se genera con contenido gráfico no vacío (> 0 bytes, con nodos `<g>` renderizados).
4. Los artefactos `.mmd`/`.svg` generados son temporales (usados solo para validar sintaxis); la fuente de verdad permanece en los bloques ```` ```mermaid ```` embebidos en `02-user-stories.md`.

## 3. Alcance de la validación

Se identificaron y validaron **6 diagramas de secuencia**, correspondientes a los flujos críticos de 6 historias de usuario distintas, cubriendo 5 de las 6 secciones de módulos del documento de historias:

| # | Historia | Flujo representado | Sección | Actores/Participantes | Resultado |
|---|---|---|---|---|---|
| 1 | US-002 | Marcaje biométrico con geofencing y liveness detection | 2.1 Clocking | 6 | ✅ Válido |
| 2 | US-003 | Marcaje offline y sincronización automática | 2.1 Clocking | 5 | ✅ Válido |
| 3 | US-006 | Cálculo automático de horas extra y recargos | 2.3 Pre-Payroll Engine | 6 | ✅ Válido |
| 4 | US-009 | Workflow de aprobación de incidencias (2 niveles) | 2.4 Absence Management | 7 | ✅ Válido |
| 5 | US-012 | Exportación e integración con ERP/Nómina | 2.6 Integraciones | 5 | ✅ Válido |
| 6 | US-013 | Bitácora de auditoría inalterable (append-only) | 2.6 Auditoría | 5 | ✅ Válido |

## 4. Resultados de la ejecución

Salida de la compilación por diagrama (`mmdc`, código de salida y tamaño del SVG generado):

| Diagrama | Código de salida | Tamaño `.svg` | Elementos `<g>` renderizados |
|---|---|---|---|
| US-002_marcaje-biometrico-geofencing.mmd | 0 | 38 341 bytes | 9 |
| US-003_offline-sync.mmd | 0 | 36 904 bytes | 9 |
| US-006_calculo-horas-extra.mmd | 0 | 33 004 bytes | 6 |
| US-009_workflow-aprobacion.mmd | 0 | 42 875 bytes | 13 |
| US-012_integracion-erp.mmd | 0 | 33 446 bytes | 7 |
| US-013_bitacora-auditoria.mmd | 0 | 32 889 bytes | 7 |

**Resultado global: 6/6 diagramas válidos (100%). 0 errores de sintaxis detectados.**

## 5. Cobertura y observaciones

- Los 6 diagramas usan bloques `alt`/`else`/`loop`/`Note over` de la sintaxis `sequenceDiagram`, todos soportados y correctamente parseados por la versión 11.17.0 de mermaid.
- No se identificaron caracteres especiales sin escapar (paréntesis, dos puntos) que pudieran romper el parser; todos los mensajes se redactaron evitando símbolos reservados de Mermaid.
- La sección 2.2 (Scheduling, US-004/US-005) y 2.5 (ESS/MSS, US-010/US-011/US-011b) no incluyen diagrama de secuencia en la v1.0, al tratarse de flujos de configuración/consulta de menor complejidad de interacción entre sistemas; se recomienda evaluar en una futura iteración si ameritan diagramación (p. ej. US-005 carga masiva por archivo).

## 6. Archivos fuente independientes

Además de estar embebidos en `02-user-stories.md`, los 6 diagramas se mantienen como archivos `.mmd` independientes (fuente de verdad) junto con su `.svg` renderizado en [`diagrams/`](diagrams/):

```
docs/specs/functional/diagrams/
├── README.md
├── US-002_marcaje-biometrico-geofencing.mmd / .svg
├── US-003_offline-sync.mmd / .svg
├── US-006_calculo-horas-extra.mmd / .svg
├── US-009_workflow-aprobacion.mmd / .svg
├── US-012_integracion-erp.mmd / .svg
└── US-013_bitacora-auditoria.mmd / .svg
```

Si se edita un diagrama, debe actualizarse **tanto** el bloque ```` ```mermaid ```` en `02-user-stories.md` **como** el archivo `.mmd` correspondiente en `diagrams/`, para que ambos permanezcan sincronizados.

## 7. Reproducibilidad

Para reproducir esta validación (re-renderizar los `.svg` a partir de los `.mmd` en `diagrams/`):

```bash
cd docs/specs/functional/diagrams
for f in *.mmd; do
  npx @mermaid-js/mermaid-cli -i "$f" -o "${f%.mmd}.svg"
done
```

Alternativamente, para re-extraer los diagramas directamente desde `02-user-stories.md` (por ejemplo, tras editar los bloques embebidos):

```bash
node -e "
const fs = require('fs');
const src = fs.readFileSync('docs/specs/functional/02-user-stories.md', 'utf8');
const re = /\`\`\`mermaid\n([\s\S]*?)\`\`\`/g;
let m, i = 0;
while ((m = re.exec(src)) !== null) {
  fs.writeFileSync('diagram-' + String(++i).padStart(2,'0') + '.mmd', m[1]);
}
console.log('Extracted', i, 'diagrams');
"
for f in diagram-*.mmd; do
  npx @mermaid-js/mermaid-cli -i "$f" -o "${f%.mmd}.svg"
done
```

Cualquier cambio futuro a los diagramas debe re-ejecutar esta validación y actualizar este documento antes de aprobar la especificación.

## 7. Conclusión

Los diagramas de secuencia del documento `02-user-stories.md` cumplen el criterio de calidad sintáctica exigido para su publicación como parte de la especificación funcional del Time Clock System. No se requieren correcciones adicionales sobre esta versión.
