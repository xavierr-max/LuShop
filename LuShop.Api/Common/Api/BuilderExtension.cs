using LuShop.Api.Data;
using LuShop.Api.Handlers;
using LuShop.Api.Models;
using LuShop.Core;
using LuShop.Core.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Common.Api;

public static class BuilderExtension
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddUserSecrets<Program>(); // Dica: Útil para desenvolvimento local
        
        Configuration.ConnectionString = builder
            .Configuration
            .GetConnectionString("DefaultConnection") ?? string.Empty;
        Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;
        Configuration.FrontendUrl = builder.Configuration.GetValue<string>("FrontendUrl") ?? string.Empty;
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

        // 2. AUTORIZAÇÃO (Aqui estava o erro)
        // Use AddAuthorization (Server), não AddAuthorizationCore (Client)
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => 
                policy.RequireRole("Admin")); // RequireRole resolve o problema da URL longa automaticamente
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
            .AddRoles<IdentityRole<long>>() // Essencial para o RequireRole funcionar
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();
    }
    
    public static void AddCrossOrigin(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(
            options => options.AddPolicy(
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

    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddTransient<IOrderHandler, OrderHandler>();
    }
}