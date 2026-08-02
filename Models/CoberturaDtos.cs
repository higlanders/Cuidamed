using System.Text.Json.Serialization;

namespace Cuidanet.Models;

/// <summary>Cliente y plan activo del asegurado (GET Cobertura/plan).</summary>
public class CoberturaPlanDto
{
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("apellido")]
    public string? Apellido { get; set; }

    [JsonPropertyName("cedula")]
    public string? Cedula { get; set; }

    [JsonPropertyName("parentesco")]
    public string? Parentesco { get; set; }

    [JsonPropertyName("nombrePlan")]
    public string? NombrePlan { get; set; }

    [JsonPropertyName("nombreCliente")]
    public string? NombreCliente { get; set; }

    [JsonPropertyName("planesId")]
    public int? PlanesId { get; set; }

    [JsonPropertyName("clienteId")]
    public int? ClienteId { get; set; }

    [JsonPropertyName("beneficiarioId")]
    public int? BeneficiarioId { get; set; }

    [JsonIgnore]
    public string NombreCompleto
    {
        get
        {
            var full = $"{Nombre} {Apellido}".Trim();
            return string.IsNullOrWhiteSpace(full) ? $"Cédula {Cedula}" : full;
        }
    }
}

/// <summary>Consumos consolidados (GET Cobertura/consumos).</summary>
public class CoberturaConsumoDto
{
    [JsonPropertyName("cedula")]
    public string? Cedula { get; set; }

    [JsonPropertyName("asegurado")]
    public string? Asegurado { get; set; }

    [JsonPropertyName("clienteId")]
    public int? ClienteId { get; set; }

    [JsonPropertyName("beneficiarioId")]
    public int? BeneficiarioId { get; set; }

    [JsonPropertyName("consumoAps")]
    public decimal ConsumoAps { get; set; }

    [JsonPropertyName("casosAps")]
    public int CasosAps { get; set; }

    [JsonPropertyName("consumoReembolso")]
    public decimal ConsumoReembolso { get; set; }

    [JsonPropertyName("casosReembolso")]
    public int CasosReembolso { get; set; }

    [JsonPropertyName("consumoCartaAval")]
    public decimal ConsumoCartaAval { get; set; }

    [JsonPropertyName("casosCartaAval")]
    public int CasosCartaAval { get; set; }

    [JsonPropertyName("consumoMedicamentos")]
    public decimal ConsumoMedicamentos { get; set; }

    [JsonPropertyName("casosMedicamentos")]
    public int CasosMedicamentos { get; set; }

    [JsonPropertyName("consumoTotal")]
    public decimal ConsumoTotal { get; set; }

    [JsonPropertyName("fechaDesde")]
    public DateTime? FechaDesde { get; set; }

    [JsonPropertyName("fechaHasta")]
    public DateTime? FechaHasta { get; set; }
}
