# Ejemplos imprimibles — Reembolso / extracción IA

Documentos de prueba para adjuntar en **Reembolso** (récipe, indicaciones e informe).

## Archivos

| Archivo | Usar en el slot |
|---------|-----------------|
| [01-recipe-medico.html](01-recipe-medico.html) | Récipe médico |
| [02-indicaciones.html](02-indicaciones.html) | Indicaciones |
| [03-informe-medico.html](03-informe-medico.html) | Informe médico |

## Cómo imprimir

1. Abre cada HTML en el navegador (doble clic o arrastrar al Chrome/Edge).
2. `Ctrl+P` → destino **Microsoft Print to PDF** (o impresora real).
3. Sube el PDF o una foto del papel impreso en la app.

## Flujo backend (contrato v1)

```
App → Imagenes/upload → POST Reembolso/solicitud
  → Tratamiento Status=Pendiente + cola ReembolsoAppSolicitud
  → Worker APILIS → CoraNet tipo 3 → CuidamedIA
  → ResultadoExtraccionTratamiento (schemaVersion=1)
  → Match EXACTO Medicamento → TratamientoDetalle
  → Encabezado (médico/fecha/diagnóstico/vigencia) solo si confianza=alta
  → GET Reembolso/solicitud/{id}/borrador (módulo nuevo)
```

### Reglas de apply

| Campo | Regla |
|-------|--------|
| Medicamentos | Solo si `Nombre` del catálogo coincide **exacto** (CI + sin acentos). Si no, queda en `unmatched` y no se inserta. |
| Encabezado | Autocompleta solo con `confianza = alta`. |
| Diagnóstico | Lookup exacto en `Diagnostico.Nombre` si viene texto. |
| Dosis / entrega | No se escribe en este paso (post-aprobación legacy). |

### JSON canónico (ResultadoJson)

Campos principales: `schemaVersion`, `medico`, `fechaRecipe`, `diagnosticoTexto`, `diagnosticoId`, `vigenciaDias`, `vigenciaMeses`, `confianza`, `advertencias`, `medicamentos[]`, `matched[]`, `unmatched[]`, `encabezadoAplicado`.

## Datos que debe extraer la IA

- **Médico:** Dr. Carlos Enrique Mendoza Rivas  
- **Fecha del récipe:** 12/09/2026 (o `2026-09-12`)  
- **Medicamentos:**
  1. Losartán potásico 50 mg — 30 tabletas  
  2. Amlodipino 5 mg — 30 tabletas  
  3. Atorvastatina 20 mg — 30 tabletas  

Son documentos ficticios solo para pruebas. Para que entren a `TratamientoDetalle`, el **nombre debe coincidir exacto** con `dbo.Medicamento.Nombre` en CuidaNet.
