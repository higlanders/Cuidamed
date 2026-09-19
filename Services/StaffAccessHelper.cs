using System.Security.Claims;

namespace Cuidanet.Services;

/// <summary>
/// Detecta sesión staff admin nivel 1 o 2 (claim Role del login APILIS GetUsuarioAPP).
/// Hoy Cuidamed suele autenticar solo afiliados; con JWT staff el panel de lectura manual aparece.
/// </summary>
public static class StaffAccessHelper
{
    public static bool EsAdminNivel1o2(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        // JWT afiliado Cuidamed
        if (string.Equals(user.FindFirst("token_use")?.Value, "afiliado", StringComparison.OrdinalIgnoreCase))
            return false;

        var role = user.FindFirst(ClaimTypes.Role)?.Value?.Trim()
            ?? user.FindFirst("role")?.Value?.Trim();
        if (role is "1" or "2")
            return true;

        var nivel = user.FindFirst("nivel")?.Value?.Trim()
            ?? user.FindFirst("Nivel")?.Value?.Trim();
        return nivel is "1" or "2";
    }
}
