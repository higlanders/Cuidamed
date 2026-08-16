using Microsoft.Extensions.Configuration;

namespace Cuidanet.Services;

/// <summary>Parámetros públicos de la app (empresa LIS, enlaces y WhatsApp).</summary>
public sealed class CuidanetAppSettings(IConfiguration configuration)
{
    public int LisClienteId => ReadInt("CuidanetApp:LisClienteId", 10);

    public string VenemergenciaUrl =>
        FirstNonEmpty(configuration["CuidanetApp:VenemergenciaUrl"], "https://venemergencia.com/");

    public string WhatsAppRedyplan =>
        FirstNonEmpty(configuration["CuidanetApp:WhatsAppRedyplan"], "+584241271422");

    public string WhatsAppCuidamed =>
        FirstNonEmpty(configuration["CuidanetApp:WhatsAppCuidamed"], "+584242186707");

    public int ServicioImagenDocumentos =>
        ReadInt("CuidanetApp:ServicioImagenDocumentos", 1024);

    /// <summary>Origen de la imagen para carpetas que exigen Fuente (Reembolsos, PresupuestoCA).</summary>
    public string ImagenFuente =>
        FirstNonEmpty(configuration["CuidanetApp:ImagenFuente"], "App");

    /// <summary>Página de CuidaNet que sirve adjuntos con token (oculta la ruta física).</summary>
    public string ServirAdjuntoUrl =>
        FirstNonEmpty(
            configuration["CuidanetApp:ServirAdjuntoUrl"],
            "https://admin.cuidanet.net/online/ServirAdjunto.aspx");

    private int ReadInt(string key, int fallback) =>
        int.TryParse(configuration[key], out var value) ? value : fallback;

    private static string FirstNonEmpty(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    public bool EsUrlVenemergenciaValida =>
        Uri.TryCreate(VenemergenciaUrl, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
