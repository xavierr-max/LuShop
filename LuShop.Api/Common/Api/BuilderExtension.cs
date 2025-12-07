using System.Text.Json.Serialization;
using LuShop.Api.Data;
using LuShop.Api.Handlers;
using LuShop.Api.Models;
using LuShop.Core;
using LuShop.Core.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;


namespace LuShop.Api.Common.Api;

public static class BuilderExtension

{
    public static void AddConfiguration(this WebApplicationBuilder builder)

    {
        builder.Configuration.AddUserSecrets<Program>();

        Configuration.ConnectionString = builder
            .Configuration
            .GetConnectionString("DefaultConnection") ?? string.Empty;

        Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;

        Configuration.FrontendUrl = builder.Configuration.GetValue<string>("FrontendUrl") ?? string.Empty;

        ApiConfiguration.StripeApiKey = builder.Configuration.GetValue<string>("StripeApiKey") ?? string.Empty;


        StripeConfiguration.ApiKey = ApiConfiguration.StripeApiKey;
    }


    public static void AddDocumentation(this WebApplicationBuilder builder)

    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(x => { x.CustomSchemaIds(n => n.FullName); });
    }


    public static void AddSecurity(this WebApplicationBuilder builder)

    {
// 1. AUTENTICAÇÃO

        builder.Services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();


// 2. AUTORIZAÇÃO

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireRole("Admin"));
        });
    }


    public static void AddDataContexts(this WebApplicationBuilder builder)

    {
        builder
            .Services
            .AddDbContext<AppDbContext>
                (x => { x.UseSqlServer(Configuration.ConnectionString); });


        builder.Services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();
    }

    public static void AddCrossOrigin(this WebApplicationBuilder builder)

    {
        builder.Services.AddCors(options => options.AddPolicy(
            ApiConfiguration.CorsPolicyName,
            policy => policy
                .WithOrigins([
                    Configuration.BackendUrl,

                    Configuration.FrontendUrl
                ])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
        ));
    }

    public static void AddJsonSerialization(this WebApplicationBuilder builder)

    {
// Configuração para Minimal APIs (TypedResults, IResult)

        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });


// Configuração para Controllers Tradicionais (caso use algum)

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
    }


    public static void AddServices(this WebApplicationBuilder builder)

    {
// Handlers de Pedidos e Produtos (Existentes)

        builder.Services.AddTransient<IOrderHandler, OrderHandler>();

        builder.Services.AddTransient<IProductHandler, ProductHandler>();

        builder.Services.AddTransient<ICartHandler, CartHandler>();

        builder.Services.AddTransient<ICategoryHandler, CategoryHandler>();

        builder.Services.AddTransient<IVoucherHandler, VoucherHandler>();
        
        builder.Services.AddTransient<IStripeHandler, StripeHandler>();
    }
}