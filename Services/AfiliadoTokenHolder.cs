namespace Cuidanet.Services;

/// <summary>JWT de afiliado en memoria para el handler HTTP (sin ciclar con CuidanetApiClient).</summary>
public sealed class AfiliadoTokenHolder
{
    public string? Token { get; set; }
    public string? Cedula { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    public void Set(string? token, string? cedula, DateTimeOffset? expiresAt)
    {
        Token = token;
        Cedula = cedula;
        ExpiresAt = expiresAt;
    }

    public void Clear() => Set(null, null, null);
}
