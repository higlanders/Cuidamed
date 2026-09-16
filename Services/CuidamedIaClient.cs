using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuidanet.Models;

namespace Cuidanet.Services;

/// <summary>
/// Extracción de tratamiento vía cola CoraNet (mismo patrón que CuidaNet.Web):
/// Cuidamed → CoraNet.Api → IaWorker (oficina) → CuidamedIA.
/// </summary>
public sealed class CuidamedIaClient(
    HttpClient httpClient,
    CuidanetAppSettings appSettings)
{
    private const int TipoExtraerTratamiento = 3;
    private const int EstadoListo = 2;
    private const int EstadoError = 3;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly object TokenSync = new();
    private static string? CachedToken;
    private static DateTime CachedTokenExpiraUtc = DateTime.MinValue;

    public async Task<ExtraccionTratamientoDto> ExtraerTratamientoAsync(
        IReadOnlyList<(string FileName, string? ContentType, byte[] Bytes)> documentos,
        CancellationToken ct = default)
    {
        if (!appSettings.EsCuidamedIaConfigurada)
            throw new InvalidOperationException(
                "CuidamedIA no está configurada (CoraNetApi:BaseUrl / Email / Password).");

        if (documentos.Count == 0)
            throw new ArgumentException("Se requiere al menos un documento.", nameof(documentos));
        if (documentos.Count > 6)
            throw new ArgumentException("Máximo 6 documentos por solicitud.", nameof(documentos));

        var requestObj = new
        {
            documentos = documentos.Select(d => new
            {
                nombreArchivo = d.FileName,
                tipoContenido = d.ContentType,
                contenidoBase64 = Convert.ToBase64String(d.Bytes)
            }).ToList()
        };

        var create = new
        {
            tipo = TipoExtraerTratamiento,
            requestJson = JsonSerializer.Serialize(requestObj, JsonOpts)
        };

        var baseUrl = appSettings.CoraNetApiBaseUrl;
        var token = await ObtenerTokenAsync(baseUrl, ct);

        using var postMsg = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "v1/clinico/trabajos"));
        postMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        postMsg.Content = JsonContent.Create(create, options: JsonOpts);

        using var createdResp = await httpClient.SendAsync(postMsg, ct);
        var createdBody = await createdResp.Content.ReadAsStringAsync(ct);
        if (!createdResp.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"[CuidamedIA] Encolar {(int)createdResp.StatusCode} {Trim(createdBody)}");
            throw new HttpRequestException(
                LeerError(createdBody) ?? "No se pudo encolar la extracción en CoraNet.Api.");
        }

        using var createdDoc = JsonDocument.Parse(createdBody);
        if (!TryGetInt(createdDoc.RootElement, "id", out var trabajoId) || trabajoId <= 0)
            throw new HttpRequestException("CoraNet.Api no devolvió id de trabajo.");

        return await EsperarResultadoAsync(baseUrl, trabajoId, ct);
    }

    private async Task<ExtraccionTratamientoDto> EsperarResultadoAsync(
        string baseUrl, int trabajoId, CancellationToken ct)
    {
        var timeoutSec = Math.Clamp(appSettings.CoraNetApiTimeoutSeconds, 60, 300);
        var pollMs = Math.Clamp(appSettings.CoraNetApiPollMilliseconds, 500, 10_000);
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            var token = await ObtenerTokenAsync(baseUrl, ct);

            using var getMsg = new HttpRequestMessage(
                HttpMethod.Get, new Uri(new Uri(baseUrl), $"v1/clinico/trabajos/{trabajoId}"));
            getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await httpClient.SendAsync(getMsg, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"[CuidamedIA] Poll {(int)resp.StatusCode} {Trim(body)}");
                throw new HttpRequestException(
                    LeerError(body) ?? $"Error al consultar trabajo IA #{trabajoId}.");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            TryGetInt(root, "estado", out var estado);

            if (estado == EstadoListo)
            {
                var resultadoJson = GetString(root, "resultadoJson");
                if (string.IsNullOrWhiteSpace(resultadoJson))
                    throw new HttpRequestException("El worker marcó listo sin resultado.");

                var dto = JsonSerializer.Deserialize<ExtraccionTratamientoDto>(resultadoJson, JsonOpts);
                return dto ?? new ExtraccionTratamientoDto
                {
                    Advertencias = ["Respuesta vacía de CuidamedIA."]
                };
            }

            if (estado == EstadoError)
            {
                var err = GetString(root, "error");
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(err) ? "El worker reportó error sin detalle." : err);
            }

            await Task.Delay(pollMs, ct);
        }

        throw new TimeoutException(
            "La cola IA superó el tiempo de espera. ¿Está corriendo CoraNet.IaWorker en la oficina?");
    }

    private async Task<string> ObtenerTokenAsync(string baseUrl, CancellationToken ct)
    {
        lock (TokenSync)
        {
            if (!string.IsNullOrWhiteSpace(CachedToken)
                && DateTime.UtcNow < CachedTokenExpiraUtc.AddMinutes(-2))
                return CachedToken!;
        }

        var login = new
        {
            email = appSettings.CoraNetApiEmail,
            password = appSettings.CoraNetApiPassword
        };

        using var resp = await httpClient.PostAsJsonAsync(
            new Uri(new Uri(baseUrl), "v1/auth/login"), login, JsonOpts, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"[CuidamedIA] Login CoraNet {(int)resp.StatusCode} {Trim(body)}");
            throw new HttpRequestException(
                LeerError(body) ?? "Login CoraNet.Api falló.");
        }

        using var doc = JsonDocument.Parse(body);
        var token = GetString(doc.RootElement, "token");
        if (string.IsNullOrWhiteSpace(token))
            throw new HttpRequestException("CoraNet.Api no devolvió token.");

        var expira = DateTime.UtcNow.AddHours(1);
        var expRaw = GetString(doc.RootElement, "expiraEn");
        if (!string.IsNullOrWhiteSpace(expRaw) && DateTime.TryParse(expRaw, out var parsed))
            expira = parsed.ToUniversalTime();

        lock (TokenSync)
        {
            CachedToken = token;
            CachedTokenExpiraUtc = expira;
        }

        return token;
    }

    private static bool TryGetInt(JsonElement root, string name, out int value)
    {
        value = 0;
        if (root.TryGetProperty(name, out var el) && el.TryGetInt32(out value))
            return true;
        var pascal = char.ToUpperInvariant(name[0]) + name[1..];
        return root.TryGetProperty(pascal, out el) && el.TryGetInt32(out value);
    }

    private static string? GetString(JsonElement root, string name)
    {
        if (root.TryGetProperty(name, out var el))
            return el.GetString();
        var pascal = char.ToUpperInvariant(name[0]) + name[1..];
        return root.TryGetProperty(pascal, out el) ? el.GetString() : null;
    }

    private static string? LeerError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var e))
                return e.GetString();
            if (doc.RootElement.TryGetProperty("title", out var t))
                return t.GetString();
            if (doc.RootElement.TryGetProperty("detail", out var d))
                return d.GetString();
        }
        catch (JsonException)
        {
            // cuerpo no JSON
        }

        return Trim(body);
    }

    private static string Trim(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= 200 ? text : text[..200] + "…";
    }
}
