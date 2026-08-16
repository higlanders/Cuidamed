using Cuidanet.Models;

namespace Cuidanet.Services;

    /// <summary>
    /// Resuelve la URL de visualización de imágenes vía ServirAdjunto (token),
    /// no la ruta física /online/Imagenes/...
    /// </summary>
    public static class ImagenViewer
    {
        public static string? ResolveUrl(UploadImagenResponse? imagen, string servirAdjuntoBase)
        {
            if (imagen is null)
                return null;

            var candidates = new[]
            {
                imagen.UrlServir,
                imagen.Url,
                imagen.Token,
                imagen.UrlPublica
            };

            foreach (var raw in candidates)
            {
                var resolved = Normalize(raw, servirAdjuntoBase);
                if (resolved is null)
                    continue;

                // Nunca devolver la ruta física: da 404 y no es como CuidaNet muestra adjuntos.
                if (IsDirectImagenesPath(resolved))
                    continue;

                return resolved;
            }

            return null;
        }

    private static string? Normalize(string? value, string servirAdjuntoBase)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var v = value.Trim();

        if (v.Contains("ServirAdjunto", StringComparison.OrdinalIgnoreCase))
            return ToAbsolute(v, servirAdjuntoBase);

        // Token cifrado solo (sin slashes ni esquema), como en logs de CuidaNet: ?t=...
        if (!v.Contains('/') && !v.Contains('\\') && !v.Contains(':') && v.Length >= 20)
        {
            var baseUrl = servirAdjuntoBase.TrimEnd('?', '&');
            var sep = baseUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            return $"{baseUrl}{sep}t={Uri.EscapeDataString(v)}";
        }

        if (v.StartsWith("t=", StringComparison.OrdinalIgnoreCase))
        {
            var baseUrl = servirAdjuntoBase.TrimEnd('?', '&');
            var sep = baseUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            return $"{baseUrl}{sep}{v}";
        }

        return ToAbsolute(v, servirAdjuntoBase);
    }

    private static string ToAbsolute(string value, string servirAdjuntoBase)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out _))
            return value;

        if (value.StartsWith('/'))
        {
            if (Uri.TryCreate(servirAdjuntoBase, UriKind.Absolute, out var baseUri))
                return new Uri(baseUri, value).ToString();
        }

        return value;
    }

    private static bool IsDirectImagenesPath(string url) =>
        url.Contains("/Imagenes/", StringComparison.OrdinalIgnoreCase)
        && !url.Contains("ServirAdjunto", StringComparison.OrdinalIgnoreCase);
}
