using System.Text.Json.Serialization;

namespace Cuidanet.Models;

/// <summary>Documento anexado en el flujo de reembolso (un archivo).</summary>
public sealed class ReembolsoAnexoVm
{
    public required string Tipo { get; init; }
    public required string Etiqueta { get; init; }
    public string? FileName { get; set; }
    public string? PreviewDataUrl { get; set; }
    public byte[]? Bytes { get; set; }
    public string? ContentType { get; set; }
    public UploadImagenResponse? Upload { get; set; }
    /// <summary>Fuerza remount de InputFile al quitar/reemplazar el archivo.</summary>
    public int InputKey { get; set; }

    public bool TieneArchivo =>
        !string.IsNullOrEmpty(FileName) || Bytes is { Length: > 0 } || Upload is not null;

    public void Clear()
    {
        FileName = null;
        PreviewDataUrl = null;
        Bytes = null;
        ContentType = null;
        Upload = null;
        InputKey++;
    }
}

/// <summary>Sección de anexos (récipe / indicaciones / informe) con varios archivos.</summary>
public sealed class ReembolsoSeccionVm
{
    public required string Tipo { get; init; }
    public required string Etiqueta { get; init; }
    public List<ReembolsoAnexoVm> Archivos { get; } = [];

    public bool TieneArchivos => Archivos.Any(a => a.TieneArchivo);

    public ReembolsoAnexoVm CrearArchivo() =>
        new() { Tipo = Tipo, Etiqueta = Etiqueta };

    public void Clear() => Archivos.Clear();
}

public sealed class ReembolsoSolicitudImagenDto
{
    [JsonPropertyName("imagenesId")]
    public int ImagenesId { get; set; }

    [JsonPropertyName("tipoAnexo")]
    public string? TipoAnexo { get; set; }
}

public sealed class ReembolsoSolicitudRequest
{
    [JsonPropertyName("comentario")]
    public string? Comentario { get; set; }

    [JsonPropertyName("beneficiarioId")]
    public int? BeneficiarioId { get; set; }

    [JsonPropertyName("imagenes")]
    public List<ReembolsoSolicitudImagenDto> Imagenes { get; set; } = [];
}

public sealed class ReembolsoSolicitudResponse
{
    [JsonPropertyName("solicitudId")]
    public int SolicitudId { get; set; }

    [JsonPropertyName("tratamientoId")]
    public int TratamientoId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }
}

public sealed class ReembolsoSolicitudListaDto
{
    [JsonPropertyName("solicitudId")]
    public int SolicitudId { get; set; }

    [JsonPropertyName("tratamientoId")]
    public int TratamientoId { get; set; }

    [JsonPropertyName("beneficiarioId")]
    public int BeneficiarioId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("comentario")]
    public string? Comentario { get; set; }

    [JsonPropertyName("ultimoError")]
    public string? UltimoError { get; set; }

    [JsonPropertyName("fechaCrea")]
    public DateTime FechaCrea { get; set; }

    [JsonPropertyName("fechaProceso")]
    public DateTime? FechaProceso { get; set; }

    [JsonPropertyName("cantidadAdjuntos")]
    public int CantidadAdjuntos { get; set; }

    [JsonPropertyName("puedeEditar")]
    public bool PuedeEditar { get; set; }
}

public sealed class ReembolsoAdjuntoDto
{
    [JsonPropertyName("imagenesId")]
    public int ImagenesId { get; set; }

    [JsonPropertyName("tipoAnexo")]
    public string? TipoAnexo { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("urlContenido")]
    public string UrlContenido { get; set; } = string.Empty;
}

public sealed class ReembolsoReemplazarImagenesRequest
{
    [JsonPropertyName("comentario")]
    public string? Comentario { get; set; }

    [JsonPropertyName("imagenes")]
    public List<ReembolsoSolicitudImagenDto> Imagenes { get; set; } = [];
}

public sealed class ReembolsoAnularResponse
{
    [JsonPropertyName("solicitudId")]
    public int SolicitudId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }
}

/// <summary>Contrato canónico v1 — borrador post-extracción IA.</summary>
public sealed class ResultadoExtraccionTratamientoDto
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("medico")]
    public string? Medico { get; set; }

    [JsonPropertyName("fechaRecipe")]
    public string? FechaRecipe { get; set; }

    [JsonPropertyName("diagnosticoTexto")]
    public string? DiagnosticoTexto { get; set; }

    [JsonPropertyName("diagnosticoId")]
    public int? DiagnosticoId { get; set; }

    [JsonPropertyName("confianza")]
    public string? Confianza { get; set; }

    [JsonPropertyName("advertencias")]
    public List<string> Advertencias { get; set; } = [];

    [JsonPropertyName("matched")]
    public List<MedicamentoMatchDto> Matched { get; set; } = [];

    [JsonPropertyName("unmatched")]
    public List<MedicamentoOmitidoDto> Unmatched { get; set; } = [];

    [JsonPropertyName("encabezadoAplicado")]
    public bool EncabezadoAplicado { get; set; }

    [JsonPropertyName("motivoSinEncabezado")]
    public string? MotivoSinEncabezado { get; set; }

    [JsonPropertyName("origen")]
    public string? Origen { get; set; }
}

public sealed class MedicamentoMatchDto
{
    [JsonPropertyName("nombreExtraido")]
    public string NombreExtraido { get; set; } = "";

    [JsonPropertyName("medicamentoId")]
    public int MedicamentoId { get; set; }

    [JsonPropertyName("nombreCatalogo")]
    public string? NombreCatalogo { get; set; }

    [JsonPropertyName("cantidad")]
    public decimal Cantidad { get; set; }

    [JsonPropertyName("presentacion")]
    public string? Presentacion { get; set; }

    [JsonPropertyName("posologia")]
    public string? Posologia { get; set; }
}

public sealed class MedicamentoOmitidoDto
{
    [JsonPropertyName("nombreExtraido")]
    public string NombreExtraido { get; set; } = "";

    [JsonPropertyName("motivo")]
    public string Motivo { get; set; } = "";
}

public sealed class TratamientoDetalleBorradorDto
{
    [JsonPropertyName("medicamentoId")]
    public int MedicamentoId { get; set; }

    [JsonPropertyName("nombreMedicamento")]
    public string? NombreMedicamento { get; set; }

    [JsonPropertyName("cantidad")]
    public decimal? Cantidad { get; set; }

    [JsonPropertyName("presentacion")]
    public string? Presentacion { get; set; }

    [JsonPropertyName("posologia")]
    public string? Posologia { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class ReembolsoBorradorResponse
{
    [JsonPropertyName("solicitudId")]
    public int SolicitudId { get; set; }

    [JsonPropertyName("tratamientoId")]
    public int TratamientoId { get; set; }

    [JsonPropertyName("beneficiarioId")]
    public int BeneficiarioId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("comentario")]
    public string? Comentario { get; set; }

    [JsonPropertyName("ultimoError")]
    public string? UltimoError { get; set; }

    [JsonPropertyName("fechaCrea")]
    public DateTime FechaCrea { get; set; }

    [JsonPropertyName("fechaProceso")]
    public DateTime? FechaProceso { get; set; }

    [JsonPropertyName("nombreMedico")]
    public string? NombreMedico { get; set; }

    [JsonPropertyName("fechaRecipe")]
    public DateTime? FechaRecipe { get; set; }

    [JsonPropertyName("diagnosticoId")]
    public int? DiagnosticoId { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("tratamientoStatus")]
    public string TratamientoStatus { get; set; } = "";

    [JsonPropertyName("extraccion")]
    public ResultadoExtraccionTratamientoDto? Extraccion { get; set; }

    [JsonPropertyName("detalle")]
    public List<TratamientoDetalleBorradorDto> Detalle { get; set; } = [];

    [JsonPropertyName("imagenes")]
    public List<ReembolsoAdjuntoDto> Imagenes { get; set; } = [];

    [JsonPropertyName("puedeEditar")]
    public bool PuedeEditar { get; set; }
}

/// <summary>PUT Reembolso/solicitud/{id}/lectura-manual — admin N1/N2.</summary>
public sealed class ReembolsoLecturaManualRequest
{
    [JsonPropertyName("nombreMedico")]
    public string? NombreMedico { get; set; }

    [JsonPropertyName("fechaRecipe")]
    public DateTime? FechaRecipe { get; set; }

    [JsonPropertyName("diagnosticoId")]
    public int? DiagnosticoId { get; set; }

    [JsonPropertyName("diagnosticoTexto")]
    public string? DiagnosticoTexto { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("medicamentos")]
    public List<ReembolsoLecturaManualMedicamentoDto> Medicamentos { get; set; } = [];
}

public sealed class ReembolsoLecturaManualMedicamentoDto
{
    [JsonPropertyName("medicamentoId")]
    public int MedicamentoId { get; set; }

    [JsonPropertyName("cantidad")]
    public decimal? Cantidad { get; set; }

    [JsonPropertyName("presentacion")]
    public string? Presentacion { get; set; }

    [JsonPropertyName("posologia")]
    public string? Posologia { get; set; }

    /// <summary>Solo UI; no se envía al API.</summary>
    [JsonIgnore]
    public string? NombreCatalogo { get; set; }
}

public sealed class ReembolsoMedicamentoBusquedaDto
{
    [JsonPropertyName("medicamentoId")]
    public int MedicamentoId { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }
}
