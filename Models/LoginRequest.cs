using System.Text.Json.Serialization;

namespace Cuidamed.Models
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

        public bool IsSuccessful =>
            (Ok ?? true) && (Success ?? true) && (Valid ?? true);

        public bool IsExplicitFailure =>
            Ok == false || Success == false || Valid == false;

        public string? UserMessage => Mensaje ?? Message;
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

        [JsonPropertyName("foto")]
        public string? FotoBase64 { get; set; } // Solo vendrá en el detalle unitario
    }
}
