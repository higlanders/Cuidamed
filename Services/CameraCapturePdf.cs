using System.Text;

namespace Cuidanet.Services;

/// <summary>
/// Convierte una foto JPEG de cámara a PDF de una página (JPEG embebido, sin recompresión).
/// </summary>
public static class CameraCapturePdf
{
    /// <summary>Valida el JPEG de cámara y lo envuelve en un PDF de una página.</summary>
    public static bool TryFromJpeg(
        byte[] jpegBytes,
        string jpegFileName,
        long maxBytes,
        int maxMb,
        out string pdfFileName,
        out byte[] pdfBytes,
        out string? error)
    {
        pdfFileName = string.Empty;
        pdfBytes = [];
        error = null;

        if (!UploadFileGuard.TryValidate(jpegFileName, jpegBytes, maxBytes, maxMb, out var safeJpegName, out error))
            return false;

        if (!TryWrapJpeg(jpegBytes, out pdfBytes, out error))
            return false;

        pdfFileName = Path.ChangeExtension(safeJpegName, ".pdf");
        return UploadFileGuard.TryValidate(pdfFileName, pdfBytes, maxBytes, maxMb, out pdfFileName, out error);
    }

    private static bool TryWrapJpeg(byte[] jpeg, out byte[] pdf, out string? error)
    {
        pdf = [];
        error = null;

        if (!TryReadJpegInfo(jpeg, out var width, out var height, out var colorSpace))
        {
            error = "No se pudo leer la captura para convertirla a PDF.";
            return false;
        }

        var contents = Encoding.ASCII.GetBytes($"q\n{width} 0 0 {height} 0 0 cm\n/Im0 Do\nQ\n");
        using var ms = new MemoryStream();
        void Ascii(string text) => ms.Write(Encoding.ASCII.GetBytes(text));

        Ascii("%PDF-1.4\n");
        ms.WriteByte((byte)'%');
        ms.WriteByte(0xE2);
        ms.WriteByte(0xE3);
        ms.WriteByte(0xCF);
        ms.WriteByte(0xD3);
        ms.WriteByte((byte)'\n');

        var offsets = new long[6];

        offsets[1] = ms.Position;
        Ascii("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets[2] = ms.Position;
        Ascii("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets[3] = ms.Position;
        Ascii($"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Contents 4 0 R /Resources << /XObject << /Im0 5 0 R >> >> >>\nendobj\n");

        offsets[4] = ms.Position;
        Ascii($"4 0 obj\n<< /Length {contents.Length} >>\nstream\n");
        ms.Write(contents);
        Ascii("endstream\nendobj\n");

        offsets[5] = ms.Position;
        Ascii($"5 0 obj\n<< /Type /XObject /Subtype /Image /Width {width} /Height {height} /ColorSpace {colorSpace} /BitsPerComponent 8 /Filter /DCTDecode /Length {jpeg.Length} >>\nstream\n");
        ms.Write(jpeg);
        Ascii("\nendstream\nendobj\n");

        var xref = ms.Position;
        Ascii("xref\n0 6\n");
        Ascii("0000000000 65535 f \n");
        for (var i = 1; i <= 5; i++)
            Ascii($"{offsets[i]:D10} 00000 n \n");

        Ascii($"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        pdf = ms.ToArray();
        return true;
    }

    private static bool TryReadJpegInfo(byte[] jpeg, out int width, out int height, out string colorSpace)
    {
        width = 0;
        height = 0;
        colorSpace = "/DeviceRGB";

        if (jpeg.Length < 4 || jpeg[0] != 0xFF || jpeg[1] != 0xD8)
            return false;

        var i = 2;
        while (i < jpeg.Length - 1)
        {
            if (jpeg[i] != 0xFF)
            {
                i++;
                continue;
            }

            var marker = jpeg[i + 1];
            if (marker == 0xFF)
            {
                i++;
                continue;
            }

            if (marker is 0xD8 or 0xD9 || marker is >= 0xD0 and <= 0xD7)
            {
                i += 2;
                continue;
            }

            if (i + 3 >= jpeg.Length)
                return false;

            var length = (jpeg[i + 2] << 8) | jpeg[i + 3];
            if (length < 2 || i + 2 + length > jpeg.Length)
                return false;

            if (IsStartOfFrame(marker))
            {
                if (length < 8)
                    return false;

                height = (jpeg[i + 5] << 8) | jpeg[i + 6];
                width = (jpeg[i + 7] << 8) | jpeg[i + 8];
                colorSpace = jpeg[i + 9] switch
                {
                    1 => "/DeviceGray",
                    4 => "/DeviceCMYK",
                    _ => "/DeviceRGB"
                };
                return width > 0 && height > 0;
            }

            i += 2 + length;
        }

        return false;
    }

    private static bool IsStartOfFrame(byte marker) =>
        marker is >= 0xC0 and <= 0xC3
            or >= 0xC5 and <= 0xC7
            or >= 0xC9 and <= 0xCB
            or >= 0xCD and <= 0xCF;
}
