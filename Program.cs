using System.Text;
using Cuidanet;
using Cuidanet.Handlers;
using Cuidanet.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// appsettings.json se publica fuera del pipeline SWA (.NET 10 fingerprint bug).
// CreateDefault puede no cargarlo; lo leemos explícitamente desde el BaseAddress (/Cuidamed/).
try
{
    using var configClient = new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    var json = await configClient.GetStringAsync("appsettings.json");
    if (!string.IsNullOrWhiteSpace(json) && json.Contains("CuidanetServices", StringComparison.Ordinal))
    {
        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
    }
    else
    {
        Console.Error.WriteLine("[Cuidanet] appsettings.json vacío o sin CuidanetServices.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        $"[Cuidanet] No se pudo cargar appsettings.json desde {builder.HostEnvironment.BaseAddress}: {ex.Message}");
}

if (string.IsNullOrWhiteSpace(builder.Configuration["CuidanetServices:BaseUrl"]))
{
    Console.Error.WriteLine("[Cuidanet] CuidanetServices:BaseUrl está vacío tras cargar configuración.");
}

builder.Services.AddSingleton<CuidanetAppSettings>();
builder.Services.AddSingleton<AfiliadoTokenHolder>();
builder.Services.AddScoped<CustomAuthStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<CuidanetAuthHandler>();

builder.Services.AddScoped<AfiliadoPerfilService>();
builder.Services.AddScoped<LoginFlowState>();

builder.Services.AddHttpClient<CuidanetApiClient>()
    .AddHttpMessageHandler<CuidanetAuthHandler>();

await builder.Build().RunAsync();
