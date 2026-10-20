namespace Cuidanet.Services;

/// <summary>
/// Textos de estado orientados al asegurado.
/// La decisión Aprobada/Rechazada sale de RegistrarTratamientosTemp
/// (Tratamiento / TratamientoDetalle), no de la cola interna de la solicitud.
/// </summary>
public static class ReembolsoUiLabels
{
    public const string Pendiente = "Pendiente";
    public const string EnRevision = "En revisión";
    public const string Aprobada = "Aprobada";
    public const string Rechazada = "Rechazada";
    public const string Correccion = "Requiere corrección";
    public const string Anulada = "Anulada";

    public static string EstadoVisible(
        string? solicitud,
        string? tratamiento = null,
        string? estadoLineas = null,
        IEnumerable<string?>? lineas = null)
    {
        if (Es(solicitud, "Anulado") || Es(tratamiento, "Anulado"))
            return Anulada;

        var decision = DecisionLineas(lineas) ?? NormalizarDecision(estadoLineas);
        if (Es(tratamiento, "Aprobado") || decision == "Aprobado")
            return Aprobada;
        if (Es(tratamiento, "Rechazado") || decision == "Rechazado")
            return Rechazada;
        if (decision == "Mixto")
            return Es(tratamiento, "Aprobado") ? Aprobada : EnRevision;

        if (Es(solicitud, "Error"))
            return Correccion;
        if (Es(solicitud, "Pendiente")
            && !Es(tratamiento, "Listo")
            && !Es(tratamiento, "Procesando"))
            return Pendiente;
        if (Es(solicitud, "Procesando")
            || Es(solicitud, "Listo")
            || Es(tratamiento, "Listo")
            || Es(tratamiento, "Pendiente")
            || Es(tratamiento, "Promovido")
            || Es(tratamiento, "Procesando"))
            return EnRevision;

        return string.IsNullOrWhiteSpace(solicitud) ? "—" : solicitud.Trim();
    }

    public static string EstadoLista(string? status) => EstadoVisible(status);

    public static string EstadoAsegurado(string? status) => EstadoVisible(status);

    public static string EstadoLinea(string? status) => status?.Trim() switch
    {
        "Aprobado" => Aprobada,
        "Rechazado" => Rechazada,
        "Anulado" => Anulada,
        _ => EnRevision
    };

    public static string Mensaje(
        string visible,
        bool hayMedicamentos,
        string? observaciones)
    {
        var texto = visible switch
        {
            Pendiente => "Recibimos tu solicitud. Pasará a revisión en breve.",
            EnRevision when !hayMedicamentos =>
                "Tu solicitud está en revisión. Si acabas de enviar los documentos, vuelve al detalle y actualiza el estado. Un analista completará el registro de medicamentos si hace falta.",
            EnRevision =>
                "Los medicamentos ya están en el registro. Cada uno quedará aprobado o rechazado cuando el analista termine la revisión.",
            Aprobada => "La solicitud fue aprobada.",
            Rechazada => "La solicitud fue rechazada.",
            Correccion => "No se pudo completar con estos documentos. Puedes corregirlos o anular la solicitud.",
            Anulada => "Esta solicitud fue anulada.",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(observaciones))
            return texto;
        if (visible is not (Aprobada or Rechazada or EnRevision))
            return texto;

        var nota = observaciones.Trim();
        return string.IsNullOrEmpty(texto)
            ? $"Observaciones: {nota}"
            : $"{texto} Observaciones: {nota}";
    }

    public static bool MuestraMedicamentos(string visible) =>
        visible is EnRevision or Aprobada or Rechazada;

    public static bool EsEnProceso(string? status) =>
        EstadoVisible(status) is Pendiente or EnRevision;

    public static bool EsListaParaValidar(string? status) =>
        string.Equals(status, "Listo", StringComparison.OrdinalIgnoreCase);

    public static bool EsError(string? status) =>
        string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase);

    public static string Tono(string visible) => visible switch
    {
        Aprobada => "success",
        Rechazada or Correccion => "danger",
        Anulada or EnRevision => "warning",
        _ => "info"
    };

    public static string Icono(string visible) => visible switch
    {
        Aprobada => "bi-check-circle-fill",
        Rechazada or Correccion => "bi-x-circle-fill",
        Anulada => "bi-slash-circle",
        EnRevision => "bi-hourglass-split",
        _ => "bi-info-circle-fill"
    };

    private static string? DecisionLineas(IEnumerable<string?>? lineas)
    {
        if (lineas is null)
            return null;

        var vigentes = lineas
            .Select(s => s?.Trim())
            .Where(s => !string.IsNullOrEmpty(s) && !Es(s, "Anulado"))
            .ToList();
        if (vigentes.Count == 0)
            return null;

        var hayAprobado = vigentes.Any(s => Es(s, "Aprobado"));
        var hayRechazado = vigentes.Any(s => Es(s, "Rechazado"));
        if (hayAprobado && hayRechazado)
            return "Mixto";
        if (hayAprobado && vigentes.All(s => Es(s, "Aprobado")))
            return "Aprobado";
        if (hayRechazado && vigentes.All(s => Es(s, "Rechazado")))
            return "Rechazado";
        return null;
    }

    private static string? NormalizarDecision(string? estadoLineas)
    {
        if (Es(estadoLineas, "Aprobado") || Es(estadoLineas, "Rechazado") || Es(estadoLineas, "Mixto"))
            return estadoLineas!.Trim();
        return null;
    }

    private static bool Es(string? value, string expected) =>
        string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    public static string TipoAnexo(string? tipo) => tipo?.Trim().ToLowerInvariant() switch
    {
        "cedula" or "cédula" => "Cédula",
        "recipe" or "récipe" or "recipe médico" => "Récipe médico",
        "indicaciones" => "Indicaciones",
        "informe" or "informe médico" => "Informe médico",
        _ => string.IsNullOrWhiteSpace(tipo) ? "Documento" : tipo
    };
}
