using Microsoft.AspNetCore.Components;
using Cuidanet.Models;

namespace Cuidanet.Services;

/// <summary>Redirige a Home si el afiliado LIS intenta abrir un módulo no permitido.</summary>
public sealed class LisRouteGuard(
    AfiliadoPerfilService perfilService,
    NavigationManager navigation)
{
    public async Task<bool> EnsureAllowedAsync()
    {
        var perfil = await perfilService.GetAsync();
        if (!perfil.EsLis)
            return true;

        var path = navigation.ToBaseRelativePath(navigation.Uri);
        if (AppModules.IsRouteAllowedForLis(path))
            return true;

        navigation.NavigateTo("Home");
        return false;
    }
}
