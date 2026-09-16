using System.Text.Json.Serialization;

namespace Cuidanet.Models;

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
