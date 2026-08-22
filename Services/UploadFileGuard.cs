namespace Cuidanet.Services;

/// <summary>Validación de documentos en el cliente (extensión, tamaño y magic bytes).</summary>
public static class UploadFileGuard
{
    public static bool TryValidate(
        string originalName,
        byte[] content,
        long maxBytes,
        int maxMb,
        out string safeFileName,
        out string? error)
    {
        safeFileName = SanitizeFileName(originalName);
        error = null;

        if (content.Length == 0)
        {
            error = "El archivo está vacío.";
            return false;
        }

        if (content.Length > maxBytes)
        {
            error = $"El archivo excede el límite permitido de {maxMb} MB.";
            return false;
        }

        var ext = Path.GetExtension(originalName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".pdf"))
        {
            error = "Solo se permiten archivos JPG, PNG o PDF.";
            return false;
        }

        var matches = ext switch
        {
            ".jpg" or ".jpeg" => IsJpeg(content),
            ".png" => IsPng(content),
            ".pdf" => IsPdf(content),
            _ => false
        };

        if (!matches)
        {
            error = "El contenido del archivo no coincide con un JPG, PNG o PDF válido.";
            return false;
        }

        return true;
    }

    public static string SanitizeFileName(string? name)
    {
        var file = Path.GetFileName(name ?? string.Empty);
        if (string.IsNullOrWhiteSpace(file))
            file = "documento";

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(file.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray())
            .Trim('.', ' ');

        if (string.IsNullOrWhiteSpace(cleaned))
            cleaned = "documento";

        var ext = Path.GetExtension(cleaned);
        var stem = Path.GetFileNameWithoutExtension(cleaned);
        if (stem.Length > 64)
            stem = stem[..64];

        return string.IsNullOrEmpty(ext) ? stem : stem + ext.ToLowerInvariant();
    }

    private static bool IsJpeg(byte[] content) =>
        content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF;

    private static bool IsPng(byte[] content) =>
        content.Length >= 8
        && content[0] == 0x89
        && content[1] == 0x50
        && content[2] == 0x4E
        && content[3] == 0x47
        && content[4] == 0x0D
        && content[5] == 0x0A
        && content[6] == 0x1A
        && content[7] == 0x0A;

    private static bool IsPdf(byte[] content) =>
        content.Length >= 5
        && content[0] == (byte)'%'
        && content[1] == (byte)'P'
        && content[2] == (byte)'D'
        && content[3] == (byte)'F'
        && content[4] == (byte)'-';
}
