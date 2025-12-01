using LuShop.Core.Handlers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LuShop.Web;
using LuShop.Web.Handlers;
using LuShop.Web.Security;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Security - Handler de Cookies para interceptar requisições
builder.Services.AddScoped<CookieHandler>();

// Security - Autorização
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Admin", policy => 
        // A MUDANÇA MÁGICA: 
        // Use RequireRole em vez de RequireClaim. 
        // O RequireRole verifica automaticamente tanto "Role" quanto a URL longa da Microsoft.
        policy.RequireRole("Admin"));
});

// Security - Autenticação
// Garante que a mesma instância gerencie o estado de autenticação
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(x => 
    x.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddScoped<ICookieAuthenticationStateProvider>(x => 
    x.GetRequiredService<CookieAuthenticationStateProvider>());

// MudBlazor
builder.Services.AddMudServices();

// HttpClient
builder.Services
    .AddHttpClient(Configuration.HttpClientName, opt => { opt.BaseAddress = new Uri(Configuration.BackendUrl); })
    .AddHttpMessageHandler<CookieHandler>(); 

builder.Services.AddTransient<IAccountHandler, AccountHandler>();

await builder.Build().RunAsync();