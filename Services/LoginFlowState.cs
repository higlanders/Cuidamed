using System.Text.Json;
using Cuidanet.Models;
using Microsoft.JSInterop;

namespace Cuidanet.Services;

/// <summary>Conserva el asistente de login al ir a Términos y volver (también tras recarga).</summary>
public sealed class LoginFlowState
{
    private const string StorageKey = "cn_login_flow";
    private readonly IJSRuntime _js;

    public LoginFlowState(IJSRuntime js) => _js = js;

    public bool HasSnapshot { get; private set; }
    public int Step { get; private set; } = 1;
    public long? Cedula { get; private set; }
    public string Telefono { get; private set; } = string.Empty;
    public string CodigoSms { get; private set; } = string.Empty;
    public bool AceptoTerminos { get; private set; }
    public string AfiliadoSeleccionado { get; private set; } = string.Empty;
    public List<BeneficiarioDto> Afiliados { get; private set; } = [];
    public BeneficiarioDto? AfiliadoDetalle { get; private set; }

    public async Task CaptureAsync(
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
        await PersistAsync();
    }

    public async Task HydrateAsync()
    {
        if (HasSnapshot)
            return;

        try
        {
            var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var dto = JsonSerializer.Deserialize<SnapshotDto>(json);
            if (dto is null)
                return;

            Apply(dto);
            HasSnapshot = true;
        }
        catch
        {
            // sessionStorage bloqueado o JSON inválido: el login arranca desde cédula
        }
    }

    public async Task ClearAsync()
    {
        ClearMemory();
        try
        {
            await _js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        }
        catch
        {
            // ignore
        }
    }

    public void Clear() => ClearMemory();

    private void ClearMemory()
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

    private async Task PersistAsync()
    {
        var dto = ToDto();
        try
        {
            await _js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(dto));
        }
        catch
        {
            try
            {
                StripFotos(dto);
                await _js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(dto));
            }
            catch
            {
                // cuota llena: el snapshot en memoria sigue valiendo en la misma sesión SPA
            }
        }
    }

    private SnapshotDto ToDto() => new()
    {
        Step = Step,
        Cedula = Cedula,
        Telefono = Telefono,
        CodigoSms = CodigoSms,
        AceptoTerminos = AceptoTerminos,
        AfiliadoSeleccionado = AfiliadoSeleccionado,
        Afiliados = Afiliados,
        AfiliadoDetalle = AfiliadoDetalle
    };

    private void Apply(SnapshotDto dto)
    {
        Step = dto.Step;
        Cedula = dto.Cedula;
        Telefono = dto.Telefono ?? string.Empty;
        CodigoSms = dto.CodigoSms ?? string.Empty;
        AceptoTerminos = dto.AceptoTerminos;
        AfiliadoSeleccionado = dto.AfiliadoSeleccionado ?? string.Empty;
        Afiliados = dto.Afiliados ?? [];
        AfiliadoDetalle = dto.AfiliadoDetalle;
    }

    private static void StripFotos(SnapshotDto dto)
    {
        if (dto.AfiliadoDetalle is not null)
            dto.AfiliadoDetalle.FotoBase64 = null;
        foreach (var a in dto.Afiliados)
            a.FotoBase64 = null;
    }

    private sealed class SnapshotDto
    {
        public int Step { get; set; }
        public long? Cedula { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string CodigoSms { get; set; } = string.Empty;
        public bool AceptoTerminos { get; set; }
        public string AfiliadoSeleccionado { get; set; } = string.Empty;
        public List<BeneficiarioDto> Afiliados { get; set; } = [];
        public BeneficiarioDto? AfiliadoDetalle { get; set; }
    }
}
