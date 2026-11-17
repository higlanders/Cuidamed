using Microsoft.AspNetCore.Components.Authorization;
using Cuidanet.Models;

namespace Cuidanet.Services;

/// <summary>Perfil de empresa/plan del afiliado autenticado.</summary>
public sealed class AfiliadoPerfil
{
    public int? ClienteId { get; init; }
    public int? EmpresaId { get; init; }
    public int? BeneficiarioId { get; init; }
    public string? NombreCliente { get; init; }
    public string? NombrePlan { get; init; }
    public string? Nombre { get; init; }
    public string? Apellido { get; init; }
    public bool EsLis { get; init; }

    /// <summary>Nombre y apellido del asegurado según Cobertura/plan.</summary>
    public string NombreCompleto
    {
        get
        {
            var full = $"{Nombre} {Apellido}".Trim();
            return string.IsNullOrWhiteSpace(full) ? string.Empty : full;
        }
    }
}

/// <summary>Resuelve si el afiliado logueado pertenece a la empresa LIS.</summary>
public sealed class AfiliadoPerfilService(
    CuidanetApiClient api,
    AuthenticationStateProvider authStateProvider,
    CuidanetAppSettings appSettings)
{
    private AfiliadoPerfil? _cache;

    /// <summary>Invalida el perfil cacheado (p. ej. tras elegir otro afiliado en login).</summary>
    public void Clear() => _cache = null;

    public async Task<AfiliadoPerfil> GetAsync()
    {
        if (_cache is not null)
            return _cache;

        var auth = await authStateProvider.GetAuthenticationStateAsync();
        // Identity.Name = cédula activa (afiliado elegido). Cobertura debe permitir grupo familiar.
        var cedula = auth.User.Identity?.Name;

        List<CoberturaPlanDto> planes = [];
        if (!string.IsNullOrWhiteSpace(cedula))
        {
            try
            {
                planes = await api.GetCoberturaPlanAsync(cedula) ?? [];
            }
            catch (Exception)
            {
                // Sin plan la app sigue; se trata como no-LIS.
            }
        }

        var plan = planes.FirstOrDefault();
        var filaLis = planes.FirstOrDefault(p => p.EmpresaId == appSettings.LisClienteId);

        _cache = new AfiliadoPerfil
        {
            ClienteId = plan?.ClienteId,
            EmpresaId = filaLis?.EmpresaId ?? plan?.EmpresaId,
            BeneficiarioId = plan?.BeneficiarioId,
            NombreCliente = plan?.NombreCliente,
            NombrePlan = plan?.NombrePlan,
            Nombre = plan?.Nombre,
            Apellido = plan?.Apellido,
            EsLis = filaLis is not null
        };

        return _cache;
    }
}
