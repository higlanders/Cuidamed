namespace Cuidanet.Models;

/// <summary>Módulo visible en el Home (iconos del documento APP Cuidanet).</summary>
public sealed class AppModule
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string IconPath { get; init; }
    public required string Route { get; init; }
}

/// <summary>
/// Catálogo de funcionalidades.
/// LIS (empresa 10 / Internacional de Seguros): solo coberturas, proveedores, farmacias y medicamentos.
/// </summary>
public static class AppModules
{
    public static IReadOnlyList<AppModule> All { get; } =
    [
        new() { Id = "coberturas", Label = "Coberturas y consumos", IconPath = "icons/coberturas.svg", Route = "beneficios" },
        new() { Id = "proveedores", Label = "Red de proveedores", IconPath = "icons/proveedores.svg", Route = "proveedores" },
        new() { Id = "citas-aps", Label = "Agendar cita APS", IconPath = "icons/citas-aps.svg", Route = "Citas-aps" },
        new() { Id = "telemedicina", Label = "Telemedicina", IconPath = "icons/telemedicina.svg", Route = "telemedicina" },
        new() { Id = "amd", Label = "AMD / Ambulancia", IconPath = "icons/amd.svg", Route = "amd" },
        new() { Id = "reembolso", Label = "Medicamentos", IconPath = "icons/reembolso.svg", Route = "reembolso" },
        new() { Id = "mis-solicitudes", Label = "Mis solicitudes", IconPath = "icons/reembolso.svg", Route = "mis-solicitudes" },
        new() { Id = "mis-sintomas", Label = "Mis síntomas", IconPath = "icons/mis-sintomas.svg", Route = "mis-sintomas" },
        new() { Id = "cartas-avales", Label = "Cartas avales", IconPath = "icons/cartas-avales.svg", Route = "cartas-avales" },
        new() { Id = "farmacias", Label = "Red de farmacias", IconPath = "icons/farmacias.svg", Route = "farmacias" },
        new() { Id = "contacto", Label = "Contacto de emergencia", IconPath = "icons/contacto.svg", Route = "contacto-emergencia" },
        new() { Id = "odontologia", Label = "Odontología", IconPath = "icons/odontologia.svg", Route = "odontologia" },
        new() { Id = "notificaciones", Label = "Notificaciones", IconPath = "icons/notificaciones.svg", Route = "notificaciones" },
    ];

    /// <summary>Módulos del Home para afiliados LIS (Internacional de Seguros).</summary>
    public static IReadOnlyList<AppModule> ForLis { get; } =
    [
        new() { Id = "coberturas", Label = "Coberturas", IconPath = "icons/coberturas.svg", Route = "beneficios" },
        new() { Id = "proveedores", Label = "Red de proveedores", IconPath = "icons/proveedores.svg", Route = "proveedores" },
        new() { Id = "farmacias", Label = "Red de farmacias", IconPath = "icons/farmacias.svg", Route = "farmacias" },
        new() { Id = "reembolso", Label = "Medicamentos", IconPath = "icons/reembolso.svg", Route = "reembolso" },
        new() { Id = "mis-solicitudes", Label = "Mis solicitudes", IconPath = "icons/reembolso.svg", Route = "mis-solicitudes" },
    ];

    private static readonly HashSet<string> LisAllowedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "home",
        "beneficios",
        "proveedores",
        "farmacias",
        "reembolso",
        "mis-solicitudes",
        "tratamiento",
        "validar",
        "mi-celular",
        // Acceso / sesión
        "",
        "login",
        "enrolar-celular",
        "configurar-acceso",
        "desbloquear",
        "terminos",
    };

    public static IReadOnlyList<AppModule> VisibleFor(bool esLis) =>
        esLis ? ForLis : All;

    /// <summary>Rutas de servicio permitidas para LIS (además de Home/perfil).</summary>
    public static bool IsRouteAllowedForLis(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return true;

        var slug = relativePath.Trim().Trim('/').Split('?', '#')[0];
        if (string.IsNullOrEmpty(slug))
            return true;

        // Rutas con parámetro: tratamiento/123 → tratamiento
        var firstSegment = slug.Split('/')[0];
        return LisAllowedSlugs.Contains(slug) || LisAllowedSlugs.Contains(firstSegment);
    }

    public static AppModule? FindByRoute(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var slug = relativePath.Trim().Trim('/');
        return All.FirstOrDefault(m =>
            string.Equals(m.Route, slug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(m.Id, slug, StringComparison.OrdinalIgnoreCase))
            ?? ForLis.FirstOrDefault(m =>
                string.Equals(m.Route, slug, StringComparison.OrdinalIgnoreCase)
                || string.Equals(m.Id, slug, StringComparison.OrdinalIgnoreCase));
    }
}
