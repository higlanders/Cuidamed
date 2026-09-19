namespace Cuidanet.Services;

/// <summary>
/// Textos de estado orientados al asegurado (sin mencionar IA ni procesos internos).
/// </summary>
public static class ReembolsoUiLabels
{
    public static string EstadoAsegurado(string? status) => status switch
    {
        "Pendiente" => "En proceso",
        "Procesando" => "En proceso",
        "Listo" => "Lista para validar",
        "Error" => "Requiere corrección",
        "Anulado" => "Anulada",
        _ => string.IsNullOrWhiteSpace(status) ? "—" : status
    };

    public static string EstadoLista(string? status) => status switch
    {
        "Pendiente" or "Procesando" => "En proceso",
        "Listo" => "Lista para validar",
        "Error" => "Requiere corrección",
        "Anulado" => "Anulada",
        _ => status ?? "—"
    };

    public static bool EsEnProceso(string? status) =>
        string.Equals(status, "Pendiente", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Procesando", StringComparison.OrdinalIgnoreCase);

    public static bool EsListaParaValidar(string? status) =>
        string.Equals(status, "Listo", StringComparison.OrdinalIgnoreCase);

    public static bool EsError(string? status) =>
        string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase);

    public static string TipoAnexo(string? tipo) => tipo?.Trim().ToLowerInvariant() switch
    {
        "cedula" or "cédula" => "Cédula",
        "recipe" or "récipe" or "recipe médico" => "Récipe médico",
        "indicaciones" => "Indicaciones",
        "informe" or "informe médico" => "Informe médico",
        _ => string.IsNullOrWhiteSpace(tipo) ? "Documento" : tipo
    };
}
