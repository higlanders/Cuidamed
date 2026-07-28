using Cuidamed;
using Cuidamed.Handlers;
using Cuidamed.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// appsettings*.json se excluyen del pipeline SWA (.NET 10 fingerprint bug) y se copian
// a mano a wwwroot. CreateDefault no los registra en blazor.boot → hay que cargarlos aquí.
using (var configClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    try
    {
        await using var stream = await configClient.GetStreamAsync("appsettings.json");
        builder.Configuration.AddJsonStream(stream);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[Cuidamed] No se pudo cargar appsettings.json: {ex.Message}");
    }
}

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<CuidanetAuthHandler>();
builder.Services.AddHttpClient<CuidanetApiClient>()
    .AddHttpMessageHandler<CuidanetAuthHandler>();

await builder.Build().RunAsync();
