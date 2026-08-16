using Microsoft.AspNetCore.Components.Authorization;
using Cuidanet.Models;

namespace Cuidanet.Services;

/// <summary>Perfil de empresa/plan del afiliado autenticado.</summary>
public sealed class AfiliadoPerfil
{
    public int? ClienteId { get; init; }
    public string? NombreCliente { get; init; }
    public string? NombrePlan { get; init; }
    public bool EsLis { get; init; }
}

/// <summary>Resuelve si el afiliado logueado pertenece a la empresa LIS.</summary>
public sealed class AfiliadoPerfilService(
    CuidanetApiClient api,
    AuthenticationStateProvider authStateProvider,
    CuidanetAppSettings appSettings)
{
    private AfiliadoPerfil? _cache;

    public async Task<AfiliadoPerfil> GetAsync()
    {
        if (_cache is not null)
            return _cache;

        var auth = await authStateProvider.GetAuthenticationStateAsync();
        var cedula = auth.User.Identity?.Name;

        CoberturaPlanDto? plan = null;
        if (!string.IsNullOrWhiteSpace(cedula))
        {
            try
            {
                var planes = await api.GetCoberturaPlanAsync(cedula);
                plan = planes?.FirstOrDefault();
            }
            catch (Exception)
            {
                // Sin plan la app sigue; se trata como no-LIS.
            }
        }

        _cache = new AfiliadoPerfil
        {
            ClienteId = plan?.ClienteId,
            NombreCliente = plan?.NombreCliente,
            NombrePlan = plan?.NombrePlan,
            EsLis = plan?.ClienteId is int id && id == appSettings.LisClienteId
        };

        return _cache;
    }
}
