using System.Net.Http.Json;
using System.Text.Json;
using Cuidanet.Models;

namespace Cuidanet.Services;

/// <summary>Cliente HTTP hacia CuidamedIA (extracción OCR + estructurada de tratamiento).</summary>
public sealed class CuidamedIaClient(
    HttpClient httpClient,
    CuidanetAppSettings appSettings)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ExtraccionTratamientoDto> ExtraerTratamientoAsync(
        IReadOnlyList<(string FileName, string? ContentType, byte[] Bytes)> documentos,
        CancellationToken ct = default)
    {
        if (!appSettings.EsCuidamedIaConfigurada)
            throw new InvalidOperationException("CuidamedIA no está configurada (CuidanetApp:CuidamedIaBaseUrl).");

        if (documentos.Count == 0)
            throw new ArgumentException("Se requiere al menos un documento.", nameof(documentos));

        var baseUrl = appSettings.CuidamedIaBaseUrl.TrimEnd('/') + "/";
        var path = appSettings.CuidamedIaExtraerTratamientoPath.TrimStart('/');
        var url = new Uri(new Uri(baseUrl), path);

        var payload = new
        {
            documentos = documentos.Select(d => new
            {
                nombreArchivo = d.FileName,
                tipoContenido = d.ContentType,
                contenidoBase64 = Convert.ToBase64String(d.Bytes)
            }).ToList()
        };

        using var response = await httpClient.PostAsJsonAsync(url, payload, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"[CuidamedIA] Extraer {(int)response.StatusCode} {Trim(body)}");
            throw new HttpRequestException("No se pudo analizar los documentos con CuidamedIA. Intenta de nuevo.");
        }

        var dto = JsonSerializer.Deserialize<ExtraccionTratamientoDto>(body, JsonOpts);
        return dto ?? new ExtraccionTratamientoDto
        {
            Advertencias = ["Respuesta vacía de CuidamedIA."]
        };
    }

    private static string Trim(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= 200 ? text : text[..200] + "…";
    }
}
