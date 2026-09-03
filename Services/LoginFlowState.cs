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
    public string? PendingToken { get; private set; }
    public DateTimeOffset? PendingExpires { get; private set; }

    public async Task CaptureAsync(
        int step,
        long? cedula,
        string telefono,
        string codigoSms,
        bool aceptoTerminos,
        string afiliadoSeleccionado,
        List<BeneficiarioDto> afiliados,
        BeneficiarioDto? afiliadoDetalle,
        string? pendingToken = null,
        DateTimeOffset? pendingExpires = null)
    {
        Step = step;
        Cedula = cedula;
        Telefono = telefono ?? string.Empty;
        CodigoSms = string.Empty;
        AceptoTerminos = aceptoTerminos;
        AfiliadoSeleccionado = afiliadoSeleccionado ?? string.Empty;
        Afiliados = StripFotos(afiliados ?? []);
        AfiliadoDetalle = StripFoto(afiliadoDetalle);
        PendingToken = string.IsNullOrWhiteSpace(pendingToken) ? null : pendingToken.Trim();
        PendingExpires = pendingExpires;
        HasSnapshot = true;
        await PersistAsync();
        _ = codigoSms;
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
            CodigoSms = string.Empty;
            Afiliados = StripFotos(Afiliados);
            AfiliadoDetalle = StripFoto(AfiliadoDetalle);
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
        PendingToken = null;
        PendingExpires = null;
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
        CodigoSms = string.Empty,
        AceptoTerminos = AceptoTerminos,
        AfiliadoSeleccionado = AfiliadoSeleccionado,
        Afiliados = Afiliados,
        AfiliadoDetalle = AfiliadoDetalle,
        PendingToken = PendingToken,
        PendingExpires = PendingExpires
    };

    private void Apply(SnapshotDto dto)
    {
        Step = dto.Step;
        Cedula = dto.Cedula;
        Telefono = dto.Telefono ?? string.Empty;
        CodigoSms = string.Empty;
        AceptoTerminos = dto.AceptoTerminos;
        AfiliadoSeleccionado = dto.AfiliadoSeleccionado ?? string.Empty;
        Afiliados = dto.Afiliados ?? [];
        AfiliadoDetalle = dto.AfiliadoDetalle;
        PendingToken = string.IsNullOrWhiteSpace(dto.PendingToken) ? null : dto.PendingToken.Trim();
        PendingExpires = dto.PendingExpires;
    }

    private static BeneficiarioDto? StripFoto(BeneficiarioDto? dto)
    {
        if (dto is not null)
            dto.FotoBase64 = null;
        return dto;
    }

    private static List<BeneficiarioDto> StripFotos(List<BeneficiarioDto> list)
    {
        foreach (var a in list)
            a.FotoBase64 = null;
        return list;
    }

    private static void StripFotos(SnapshotDto dto)
    {
        dto.AfiliadoDetalle = StripFoto(dto.AfiliadoDetalle);
        dto.Afiliados = StripFotos(dto.Afiliados ?? []);
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
        public string? PendingToken { get; set; }
        public DateTimeOffset? PendingExpires { get; set; }
    }
}
