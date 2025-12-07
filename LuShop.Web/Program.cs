using System.Globalization;
using LuShop.Core.Handlers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LuShop.Web;
using LuShop.Web.Handlers;
using LuShop.Web.Security;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuração da URL do Backend
Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;

// Configuração de Cultura (R$ e Datas)
var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- SEGURANÇA ---

// 1. Handler de Cookies (Interceptador HTTP)
builder.Services.AddTransient<CookieHandler>();

// 2. Autorização (Policies)
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
});

// 3. Autenticação (State Provider)
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(x => 
    x.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddScoped<ICookieAuthenticationStateProvider>(x => 
    x.GetRequiredService<CookieAuthenticationStateProvider>());

// --- UI ---
builder.Services.AddMudServices();

// --- HTTP CLIENT & HANDLERS ---

// 1. Configura o "Cliente Nomeado" (Factory)
// Todos os handlers que pedirem httpClientFactory.CreateClient("lushop") vão receber
// um cliente configurado com essa URL Base e com o CookieHandler anexado.
builder.Services
    .AddHttpClient(Configuration.HttpClientName, opt => 
    { 
        opt.BaseAddress = new Uri(Configuration.BackendUrl); 
    })
    .AddHttpMessageHandler<CookieHandler>(); 

// 2. Registra os Handlers da Aplicação
// Como eles usam IHttpClientFactory internamente, basta registrá-los como Transient.
builder.Services.AddTransient<IAccountHandler, AccountHandler>();
builder.Services.AddTransient<IOrderHandler, OrderHandler>();  
builder.Services.AddTransient<IProductHandler, ProductHandler>(); 
builder.Services.AddTransient<ICartHandler, CartHandler>();
builder.Services.AddTransient<ICategoryHandler, CategoryHandler>();
builder.Services.AddTransient<IVoucherHandler, VoucherHandler>();
builder.Services.AddTransient<IStripeHandler, StripeHandler>();

await builder.Build().RunAsync();