using System.Text.Json.Serialization;

namespace Cuidanet.Models;

/// <summary>Línea de medicamento para solicitud de reembolso / tratamiento.</summary>
public sealed class MedicamentoLineaVm
{
    public string Nombre { get; set; } = string.Empty;
    public string? Dosis { get; set; }
    public string? Cantidad { get; set; }
}

/// <summary>Respuesta estructurada de CuidamedIA al extraer datos de recipe/indicaciones/informe.</summary>
public sealed class ExtraccionTratamientoDto
{
    [JsonPropertyName("medico")]
    public string? Medico { get; set; }

    [JsonPropertyName("fechaRecipe")]
    public string? FechaRecipe { get; set; }

    [JsonPropertyName("medicamentos")]
    public List<MedicamentoExtraidoDto> Medicamentos { get; set; } = [];

    [JsonPropertyName("confianza")]
    public string? Confianza { get; set; }

    [JsonPropertyName("advertencias")]
    public List<string> Advertencias { get; set; } = [];
}

public sealed class MedicamentoExtraidoDto
{
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("dosis")]
    public string? Dosis { get; set; }

    [JsonPropertyName("cantidad")]
    public string? Cantidad { get; set; }
}

/// <summary>Documento anexado en el flujo de reembolso.</summary>
public sealed class ReembolsoAnexoVm
{
    public required string Tipo { get; init; }
    public required string Etiqueta { get; init; }
    public string? FileName { get; set; }
    public string? PreviewDataUrl { get; set; }
    public byte[]? Bytes { get; set; }
    public string? ContentType { get; set; }
    public UploadImagenResponse? Upload { get; set; }
}
