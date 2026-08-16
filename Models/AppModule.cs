namespace Cuidanet.Models;

/// <summary>Módulo visible en el Home (iconos del documento APP Cuidanet).</summary>
public sealed class AppModule
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string IconPath { get; init; }
    public required string Route { get; init; }

    /// <summary>Si es false, el icono no se muestra a afiliados de la empresa LIS.</summary>
    public bool VisibleParaLis { get; init; } = true;
}

/// <summary>Catálogo de funcionalidades. LIS (empresa 10) oculta cartas, reembolso y odontología.</summary>
public static class AppModules
{
    public static IReadOnlyList<AppModule> All { get; } =
    [
        new() { Id = "coberturas", Label = "Coberturas y consumos", IconPath = "icons/coberturas.svg", Route = "beneficios" },
        new() { Id = "proveedores", Label = "Red de proveedores", IconPath = "icons/proveedores.svg", Route = "proveedores" },
        new() { Id = "citas-aps", Label = "Agendar cita APS", IconPath = "icons/citas-aps.svg", Route = "Citas-aps" },
        new() { Id = "telemedicina", Label = "Telemedicina", IconPath = "icons/telemedicina.svg", Route = "telemedicina" },
        new() { Id = "amd", Label = "AMD / Ambulancia", IconPath = "icons/amd.svg", Route = "amd" },
        new() { Id = "reembolso", Label = "Solicitud reembolso", IconPath = "icons/reembolso.svg", Route = "reembolso", VisibleParaLis = false },
        new() { Id = "mis-sintomas", Label = "Mis síntomas", IconPath = "icons/mis-sintomas.svg", Route = "mis-sintomas" },
        new() { Id = "cartas-avales", Label = "Cartas avales", IconPath = "icons/cartas-avales.svg", Route = "cartas-avales", VisibleParaLis = false },
        new() { Id = "farmacias", Label = "Farmacias afiliadas", IconPath = "icons/farmacias.svg", Route = "farmacias" },
        new() { Id = "contacto", Label = "Contacto de emergencia", IconPath = "icons/contacto.svg", Route = "contacto-emergencia" },
        new() { Id = "odontologia", Label = "Odontología", IconPath = "icons/odontologia.svg", Route = "odontologia", VisibleParaLis = false },
        new() { Id = "notificaciones", Label = "Notificaciones", IconPath = "icons/notificaciones.svg", Route = "notificaciones" },
    ];

    public static IReadOnlyList<AppModule> VisibleFor(bool esLis) =>
        esLis ? All.Where(m => m.VisibleParaLis).ToList() : All;

    public static AppModule? FindByRoute(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var slug = relativePath.Trim().Trim('/');
        return All.FirstOrDefault(m =>
            string.Equals(m.Route, slug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(m.Id, slug, StringComparison.OrdinalIgnoreCase));
    }
}
