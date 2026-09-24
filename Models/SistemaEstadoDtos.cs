using System.Text.Json.Serialization;

namespace Cuidanet.Models;

public sealed class ServicioEstadoItemDto
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("lastBeatUtc")]
    public DateTime? LastBeatUtc { get; set; }
}

public sealed class CoraNetCadenaEstadoDto
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("coraNetApi")]
    public ServicioEstadoItemDto? CoraNetApi { get; set; }

    [JsonPropertyName("cuidamedIa")]
    public ServicioEstadoItemDto? CuidamedIa { get; set; }

    [JsonPropertyName("ollama")]
    public ServicioEstadoItemDto? Ollama { get; set; }
}

public sealed class SistemaEstadoResponse
{
    [JsonPropertyName("checkedAtUtc")]
    public DateTime CheckedAtUtc { get; set; }

    [JsonPropertyName("apilis")]
    public ServicioEstadoItemDto? Apilis { get; set; }

    [JsonPropertyName("workerApilis")]
    public ServicioEstadoItemDto? WorkerApilis { get; set; }

    [JsonPropertyName("coraNetIaWorker")]
    public CoraNetCadenaEstadoDto? CoraNetIaWorker { get; set; }
}
