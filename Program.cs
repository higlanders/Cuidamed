using Cuidanet;
using Cuidanet.Handlers;
using Cuidanet.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// appsettings*.json se excluyen del pipeline SWA (.NET 10 fingerprint bug) y se copian
// a mano a wwwroot. CreateDefault no los registra en blazor.boot → hay que cargarlos aquí.
// Usamos MemoryStream: AddJsonStream no debe depender de un stream HTTP ya dispuesto.
try
{
    using var configClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    // cache-bust para evitar appsettings viejo del service worker
    var json = await configClient.GetStringAsync($"appsettings.json?v={DateTime.UtcNow.Ticks}");
    if (string.IsNullOrWhiteSpace(json) || !json.Contains("CuidanetServices", StringComparison.Ordinal))
        throw new InvalidOperationException("appsettings.json vacío o inválido.");

    builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));

    var user = builder.Configuration["CuidanetServices:user"];
    if (string.IsNullOrWhiteSpace(user))
        Console.Error.WriteLine("[Cuidamed] appsettings cargó pero CuidanetServices:user está vacío.");
    else
        Console.WriteLine($"[Cuidamed] Config API OK (user len={user.Length}).");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[Cuidamed] No se pudo cargar appsettings.json: {ex.Message}");
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

