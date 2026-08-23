using Microsoft.Extensions.Configuration;

namespace Cuidanet.Services;

/// <summary>Parámetros públicos de la app (empresa LIS, enlaces y WhatsApp).</summary>
public sealed class CuidanetAppSettings(IConfiguration configuration)
{
    public int LisClienteId => ReadInt("CuidanetApp:LisClienteId", 10);

    public string VenemergenciaUrl =>
        FirstNonEmpty(configuration["CuidanetApp:VenemergenciaUrl"], "https://venemergencia.com/");

    public string SymptomateUrl =>
        FirstNonEmpty(configuration["CuidanetApp:SymptomateUrl"], "https://symptomate.com/es");

    /// <summary>Valor de Afiliado.Tipo para el módulo de farmacias.</summary>
    public string TipoAfiliadoFarmacia =>
        FirstNonEmpty(configuration["CuidanetApp:TipoAfiliadoFarmacia"], "Farmacia");

    /// <summary>
    /// Filtro opcional de Status en Afiliado/red (p. ej. Activo). Vacío = sin filtro (igual que antes).
    /// </summary>
    public string? RedStatusFiltro
    {
        get
        {
            var raw = configuration["CuidanetApp:RedStatusFiltro"];
            return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
        }
    }

    /// <summary>
    /// Caducidad de la sesión local (minutos). No es autorización de servidor:
    /// solo acota localStorage en dispositivos compartidos.
    /// </summary>
    public int SessionMinutes
    {
        get
        {
            var minutes = ReadInt("CuidanetApp:SessionMinutes", 30);
            return minutes < 1 ? 30 : minutes;
        }
    }

    /// <summary>Tope de subida de documentos en el cliente (MB).</summary>
    public int MaxUploadMb
    {
        get
        {
            var mb = ReadInt("CuidanetApp:MaxUploadMb", 10);
            return mb < 1 ? 10 : mb;
        }
    }

    public long MaxUploadBytes => MaxUploadMb * 1024L * 1024L;

    public string WhatsAppRedyplan =>
        FirstNonEmpty(configuration["CuidanetApp:WhatsAppRedyplan"], "+584241271422");

    public string WhatsAppCuidamed =>
        FirstNonEmpty(configuration["CuidanetApp:WhatsAppCuidamed"], "+584142387774");

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

    /// <summary>Enlaces de redes (mismos de CuidaNet.Web / MasterPage.master).</summary>
    public string TwitterUrl =>
        FirstNonEmpty(configuration["CuidanetApp:TwitterUrl"], "https://twitter.com/Cuidamed");

    public string FacebookUrl =>
        FirstNonEmpty(
            configuration["CuidanetApp:FacebookUrl"],
            "https://www.facebook.com/people/Servicios-Cuidamed-CA/61564849946533/");

    public string InstagramUrl =>
        FirstNonEmpty(configuration["CuidanetApp:InstagramUrl"], "https://instagram.com/Servicios_cuidamed");

    public string LinkedInUrl =>
        FirstNonEmpty(
            configuration["CuidanetApp:LinkedInUrl"],
            "https://www.linkedin.com/company/servicios-cuidamed-c-a/");

    public IReadOnlyList<RedSocial> RedesSociales
    {
        get
        {
            var list = new List<RedSocial>(4);
            AddRed(list, "Twitter", TwitterUrl, "bi-twitter-x");
            AddRed(list, "Facebook", FacebookUrl, "bi-facebook");
            AddRed(list, "Instagram", InstagramUrl, "bi-instagram");
            AddRed(list, "LinkedIn", LinkedInUrl, "bi-linkedin");
            return list;
        }
    }

    private static void AddRed(List<RedSocial> list, string nombre, string url, string icono)
    {
        if (EsHttpUrl(url))
            list.Add(new RedSocial(nombre, url, icono));
    }

    private int ReadInt(string key, int fallback) =>
        int.TryParse(configuration[key], out var value) ? value : fallback;

    private static string FirstNonEmpty(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    public bool EsUrlVenemergenciaValida => EsHttpUrl(VenemergenciaUrl);

    public bool EsUrlSymptomateValida => EsHttpUrl(SymptomateUrl);

    private static bool EsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}

public sealed record RedSocial(string Nombre, string Url, string Icono);
