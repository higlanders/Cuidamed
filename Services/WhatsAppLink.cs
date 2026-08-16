using System.Text;

namespace Cuidanet.Services;

/// <summary>Arma enlaces wa.me a partir de un teléfono en formato internacional.</summary>
public static class WhatsAppLink
{
    public static string? Chat(string? phoneE164, string? message = null)
    {
        var digits = DigitsOnly(phoneE164);
        if (digits.Length < 10)
            return null;

        var url = new StringBuilder("https://wa.me/").Append(digits);
        if (!string.IsNullOrWhiteSpace(message))
            url.Append("?text=").Append(Uri.EscapeDataString(message.Trim()));

        return url.ToString();
    }

    public static string DigitsOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = value.Where(char.IsDigit).ToArray();
        return new string(chars);
    }
}
