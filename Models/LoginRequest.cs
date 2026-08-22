using System.Text.Json.Serialization;

namespace Cuidanet.Models
{
    public class LoginRequest
    {
        [JsonPropertyName("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class ValidateUserResponse
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }
    }

    public class EnviarSmsRequest
    {
        [JsonPropertyName("cedula")]
        public string Cedula { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>Hostname de la página (Web OTP).</summary>
        [JsonPropertyName("origen")]
        public string? Origen { get; set; }
    }

    public class VerificarSmsRequest
    {
        [JsonPropertyName("cedula")]
        public string Cedula { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;
    }

    public class SmsApiResponse
    {
        [JsonPropertyName("ok")]
        public bool? Ok { get; set; }

        [JsonPropertyName("success")]
        public bool? Success { get; set; }

        [JsonPropertyName("valid")]
        public bool? Valid { get; set; }

        [JsonPropertyName("mensaje")]
        public string? Mensaje { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("messageId")]
        public long? MessageId { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTimeOffset? ExpiresAt { get; set; }

        [JsonPropertyName("telefonoEnmascarado")]
        public string? TelefonoEnmascarado { get; set; }

        public bool IsSuccessful =>
            (Ok ?? true) && (Success ?? true) && (Valid ?? true);

        public bool IsExplicitFailure =>
            Ok == false || Success == false || Valid == false;

        public string? UserMessage => Mensaje ?? Message;
    }

    public class AfiliadoTokenDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("expiresAt")]
        public DateTimeOffset ExpiresAt { get; set; }
    }

    // Puedes expandir estas propiedades según el JSON real de tu API
    public class BeneficiarioDto
    {
        [JsonPropertyName("beneficiarioId")]
        public int BeneficiarioId { get; set; }

        [JsonPropertyName("titularId")]
        public int TitularId { get; set; }

        [JsonPropertyName("cedula")]
        public string Cedula { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("apellido")]
        public string? Apellido { get; set; }

        [JsonPropertyName("foto")]
        public string? FotoBase64 { get; set; } // Solo vendrá en el detalle unitario

        /// <summary>Nombre y apellido del asegurado, o etiqueta de respaldo si la API no los envía.</summary>
        [JsonIgnore]
        public string NombreCompleto
        {
            get
            {
                var full = $"{Nombre} {Apellido}".Trim();
                return string.IsNullOrWhiteSpace(full)
                    ? $"Afiliado #{BeneficiarioId}"
                    : full;
            }
        }
    }

    /// <summary>Proveedor de la red CuidaNet (GET Afiliado/red).</summary>
    public class ProveedorRedDto
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("direccion")]
        public string Direccion { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonPropertyName("contactoWhatsApp")]
        public string ContactoWhatsApp { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }

        [JsonPropertyName("ciudad")]
        public string? Ciudad { get; set; }

        [JsonPropertyName("tipo")]
        public string? Tipo { get; set; }
    }

    /// <summary>Combos de filtro para la red de proveedores.</summary>
    public class ProveedorRedFiltrosDto
    {
        [JsonPropertyName("estados")]
        public List<string> Estados { get; set; } = new();

        [JsonPropertyName("ciudades")]
        public List<string> Ciudades { get; set; } = new();

        [JsonPropertyName("tipos")]
        public List<string> Tipos { get; set; } = new();
    }

    /// <summary>POST /api/Consulta — SELECT parametrizado sobre tablas whitelist.</summary>
    public class ConsultaRequestDto
    {
        [JsonPropertyName("tabla")]
        public string Tabla { get; set; } = string.Empty;

        [JsonPropertyName("campos")]
        public List<string> Campos { get; set; } = new();

        [JsonPropertyName("filtros")]
        public List<ConsultaFiltroRequestDto> Filtros { get; set; } = new();

        [JsonPropertyName("orden")]
        public List<ConsultaOrdenRequestDto> Orden { get; set; } = new();

        [JsonPropertyName("top")]
        public int Top { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }

    public class ConsultaFiltroRequestDto
    {
        [JsonPropertyName("campo")]
        public string Campo { get; set; } = string.Empty;

        [JsonPropertyName("op")]
        public string Op { get; set; } = "eq";

        [JsonPropertyName("valor")]
        public string? Valor { get; set; }
    }

    public class ConsultaOrdenRequestDto
    {
        [JsonPropertyName("campo")]
        public string Campo { get; set; } = string.Empty;

        [JsonPropertyName("dir")]
        public string Dir { get; set; } = "asc";
    }

    public class ConsultaResponseDto
    {
        [JsonPropertyName("tabla")]
        public string Tabla { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("top")]
        public int Top { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("filas")]
        public List<System.Text.Json.JsonElement> Filas { get; set; } = new();
    }

    public class PwaInstalacionRequestDto
    {
        [JsonPropertyName("evento")]
        public string Evento { get; set; } = string.Empty;

        [JsonPropertyName("plataforma")]
        public string Plataforma { get; set; } = string.Empty;

        [JsonPropertyName("clientInstallId")]
        public string ClientInstallId { get; set; } = string.Empty;

        [JsonPropertyName("cedula")]
        public string? Cedula { get; set; }

        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("origen")]
        public string? Origen { get; set; }
    }

    public class PwaInstalacionResponseDto
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("duplicado")]
        public bool Duplicado { get; set; }

        [JsonPropertyName("pwaInstalacionId")]
        public long? PwaInstalacionId { get; set; }

        [JsonPropertyName("mensaje")]
        public string? Mensaje { get; set; }
    }

    /// <summary>Evento pendiente en localStorage (cuidanetPwa.drainPendingEvents).</summary>
    public class PwaPendingEventDto
    {
        [JsonPropertyName("evento")]
        public string Evento { get; set; } = string.Empty;

        [JsonPropertyName("plataforma")]
        public string Plataforma { get; set; } = string.Empty;

        [JsonPropertyName("clientInstallId")]
        public string ClientInstallId { get; set; } = string.Empty;

        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("origen")]
        public string? Origen { get; set; }
    }
}

