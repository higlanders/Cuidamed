using Cuidanet.Models;

namespace Cuidanet.Services;

/// <summary>Conserva el asistente de login al ir a Términos y volver.</summary>
public sealed class LoginFlowState
{
    public bool HasSnapshot { get; private set; }
    public int Step { get; private set; } = 1;
    public long? Cedula { get; private set; }
    public string Telefono { get; private set; } = string.Empty;
    public string CodigoSms { get; private set; } = string.Empty;
    public bool AceptoTerminos { get; private set; }
    public string AfiliadoSeleccionado { get; private set; } = string.Empty;
    public List<BeneficiarioDto> Afiliados { get; private set; } = [];
    public BeneficiarioDto? AfiliadoDetalle { get; private set; }

    public void Capture(
        int step,
        long? cedula,
        string telefono,
        string codigoSms,
        bool aceptoTerminos,
        string afiliadoSeleccionado,
        List<BeneficiarioDto> afiliados,
        BeneficiarioDto? afiliadoDetalle)
    {
        Step = step;
        Cedula = cedula;
        Telefono = telefono ?? string.Empty;
        CodigoSms = codigoSms ?? string.Empty;
        AceptoTerminos = aceptoTerminos;
        AfiliadoSeleccionado = afiliadoSeleccionado ?? string.Empty;
        Afiliados = afiliados ?? [];
        AfiliadoDetalle = afiliadoDetalle;
        HasSnapshot = true;
    }

    public void Clear()
    {
        HasSnapshot = false;
        Step = 1;
        Cedula = null;
        Telefono = string.Empty;
        CodigoSms = string.Empty;
        AceptoTerminos = false;
        AfiliadoSeleccionado = string.Empty;
        Afiliados = [];
        AfiliadoDetalle = null;
    }
}
