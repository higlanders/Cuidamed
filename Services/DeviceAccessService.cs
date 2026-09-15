using System.Text.Json;
using Microsoft.JSInterop;

namespace Cuidanet.Services;

/// <summary>
/// PIN obligatorio y huella/WebAuthn opcional en el dispositivo.
/// El PIN nunca se envía a la API; solo desbloquea la sesión local.
/// </summary>
public sealed class DeviceAccessService
{
    private const string CredKey = "cn_device_cred";
    private const string UnlockKey = "cn_device_unlocked";
    private readonly IJSRuntime _js;
    private bool _unlocked;

    public DeviceAccessService(IJSRuntime js) => _js = js;

    public bool IsUnlocked => _unlocked;

    public async Task EnsureUnlockFlagAsync()
    {
        try
        {
            var flag = await _js.InvokeAsync<string?>("sessionStorage.getItem", UnlockKey);
            _unlocked = flag == "1";
        }
        catch (InvalidOperationException)
        {
            // JS no listo
        }
    }

    public async Task MarkUnlockedAsync()
    {
        _unlocked = true;
        try
        {
            await _js.InvokeVoidAsync("sessionStorage.setItem", UnlockKey, "1");
        }
        catch (InvalidOperationException) { }
    }

    public async Task MarkLockedAsync()
    {
        _unlocked = false;
        try
        {
            await _js.InvokeVoidAsync("sessionStorage.removeItem", UnlockKey);
        }
        catch (InvalidOperationException) { }
    }

    public async Task<DeviceCredential?> ReadCredentialAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>("localStorage.getItem", CredKey);
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            return JsonSerializer.Deserialize<DeviceCredential>(raw);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearCredentialAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", CredKey);
            await MarkLockedAsync();
        }
        catch (InvalidOperationException) { }
    }

    public async Task<bool> HasPinForCedulaAsync(string cedula)
    {
        var cred = await ReadCredentialAsync();
        return cred is not null
            && string.Equals(cred.Cedula, DigitsOnly(cedula), StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(cred.PinHash)
            && !string.IsNullOrWhiteSpace(cred.PinSalt);
    }

    public async Task SetupPinAsync(string cedula, string pin, DateTimeOffset identityExpiresAt)
    {
        var digits = DigitsOnly(cedula);
        if (pin.Length is < 4 or > 8 || pin.Any(c => !char.IsDigit(c)))
            throw new ArgumentException("El PIN debe tener entre 4 y 8 dígitos.");

        var salt = await _js.InvokeAsync<string>("CnDeviceAccess.randomSalt");
        var hash = await _js.InvokeAsync<string>("CnDeviceAccess.hashPin", pin, salt);
        var cred = new DeviceCredential
        {
            Cedula = digits,
            PinSalt = salt,
            PinHash = hash,
            IdentityExpiresAt = identityExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await WriteCredentialAsync(cred);
        await MarkUnlockedAsync();
    }

    public async Task<bool> VerifyPinAsync(string pin)
    {
        var cred = await ReadCredentialAsync();
        if (cred is null || string.IsNullOrWhiteSpace(cred.PinSalt) || string.IsNullOrWhiteSpace(cred.PinHash))
            return false;

        if (IsIdentityExpired(cred))
            return false;

        var hash = await _js.InvokeAsync<string>("CnDeviceAccess.hashPin", pin, cred.PinSalt);
        if (!string.Equals(hash, cred.PinHash, StringComparison.Ordinal))
            return false;

        await MarkUnlockedAsync();
        return true;
    }

    public async Task<bool> IsWebAuthnAvailableAsync()
    {
        try
        {
            return await _js.InvokeAsync<bool>("CnDeviceAccess.webauthnAvailable");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EnableBiometricAsync(string cedula, string displayName)
    {
        var cred = await ReadCredentialAsync();
        if (cred is null)
            return false;

        var id = await _js.InvokeAsync<string>("CnDeviceAccess.createCredential", DigitsOnly(cedula), displayName);
        if (string.IsNullOrWhiteSpace(id))
            return false;

        cred.WebAuthnCredentialId = id;
        await WriteCredentialAsync(cred);
        return true;
    }

    public async Task<bool> TryBiometricUnlockAsync()
    {
        var cred = await ReadCredentialAsync();
        if (cred is null || string.IsNullOrWhiteSpace(cred.WebAuthnCredentialId))
            return false;
        if (IsIdentityExpired(cred))
            return false;

        try
        {
            var ok = await _js.InvokeAsync<bool>("CnDeviceAccess.assertCredential", cred.WebAuthnCredentialId);
            if (ok)
                await MarkUnlockedAsync();
            return ok;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsIdentityExpired(DeviceCredential cred) =>
        cred.IdentityExpiresAt != default
        && DateTimeOffset.UtcNow > cred.IdentityExpiresAt;

    private async Task WriteCredentialAsync(DeviceCredential cred)
    {
        var json = JsonSerializer.Serialize(cred);
        await _js.InvokeVoidAsync("localStorage.setItem", CredKey, json);
    }

    private static string DigitsOnly(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());

    public sealed class DeviceCredential
    {
        public string Cedula { get; set; } = string.Empty;
        public string PinSalt { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public string? WebAuthnCredentialId { get; set; }
        public DateTimeOffset IdentityExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
