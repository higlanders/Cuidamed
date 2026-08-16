using System.Text.Json.Serialization;

namespace Cuidanet.Models
{
    public class UploadImagenResponse
    {
        [JsonPropertyName("imagenesId")]
        public int ImagenesId { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>URL preferida para ver (suele ser ServirAdjunto con token).</summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>Ruta pública directa a /Imagenes/... (no usar para vista en app).</summary>
        [JsonPropertyName("urlPublica")]
        public string UrlPublica { get; set; } = string.Empty;

        [JsonPropertyName("rutaFisica")]
        public string RutaFisica { get; set; } = string.Empty;

        [JsonPropertyName("carpeta")]
        public string Carpeta { get; set; } = string.Empty;

        [JsonPropertyName("periodo")]
        public string Periodo { get; set; } = string.Empty;

        /// <summary>Token cifrado para ServirAdjunto.aspx?t=...</summary>
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("urlServir")]
        public string? UrlServir { get; set; }
    }
}
