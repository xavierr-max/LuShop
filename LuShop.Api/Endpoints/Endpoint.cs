using LuShop.Api.Common.Api;
using LuShop.Api.Endpoints.Identity;
using LuShop.Api.Endpoints.Orders;
using LuShop.Api.Endpoints.Products;
using LuShop.Api.Models;

namespace LuShop.Api.Endpoints;

public static class Endpoint
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app
            .MapGroup("");
        
        endpoints.MapGroup("/")
            .WithTags("Health Check")
            .MapGet("/", () => "OK");
        
        endpoints.MapGroup("v1/identity")
            .WithTags("Identity")
            .MapIdentityApi<User>();
            
        endpoints.MapGroup("v1/identity")
            .WithTags("Identity")
            .MapEndpoint<LogoutEndpoint>()
            .MapEndpoint<GetRolesEndpoint>();
        
        // ORDERS (PEDIDOS)
        // Todos os endpoints abaixo herdam o prefixo "v1/orders"
        endpoints.MapGroup("v1/orders")
            .WithTags("Orders")
            .RequireAuthorization() // Dica: Descomente para proteger todas as rotas de pedido
            .MapEndpoint<CreateOrderEndpoint>()
            .MapEndpoint<PayOrderEndpoint>()
            .MapEndpoint<RefundOrderEndpoint>()
            .MapEndpoint<CancelOrderEndpoint>()
            .MapEndpoint<GetAllOrdersEndpoint>()
            .MapEndpoint<GetOrderByNumberEndpoint>();

        // PRODUCTS (PRODUTOS)
        // AQUI ESTÁ A MUDANÇA: Dividimos em dois grupos

        // GRUPO 1: Público (Vitrine)
        // Qualquer pessoa pode ver a lista e os detalhes
        var productsPublic = endpoints.MapGroup("v1/products")
            .WithTags("Products");
            
        productsPublic.MapEndpoint<GetAllProductsEndpoint>();
        productsPublic.MapEndpoint<GetProductBySlugEndpoint>();

        // GRUPO 2: Admin (Gerenciamento)
        // Só quem tem a Role "Admin" pode criar, editar ou apagar
        var productsAdmin = endpoints.MapGroup("v1/products")
            .WithTags("Products")
            .RequireAuthorization("Admin"); // <--- A MÁGICA ACONTECE AQUI
            
        productsAdmin.MapEndpoint<CreateProductEndpoint>();
        productsAdmin.MapEndpoint<UpdateProductEndpoint>();
        productsAdmin.MapEndpoint<DeleteProductEndpoint>();
    }
    
    //método de extensao que mapeia os endpoints com a interface IEndpoint
    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}