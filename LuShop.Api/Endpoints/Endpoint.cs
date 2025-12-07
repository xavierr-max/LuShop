using LuShop.Api.Common.Api;
using LuShop.Api.Endpoints.CartItem;
using LuShop.Api.Endpoints.Carts;
using LuShop.Api.Endpoints.Categories;
using LuShop.Api.Endpoints.Identity;
using LuShop.Api.Endpoints.Orders;
using LuShop.Api.Endpoints.Products;
using LuShop.Api.Endpoints.Stripe; // 👈 ADICIONE ESTA LINHA
using LuShop.Api.Endpoints.Vouchers;
using LuShop.Api.Models;

namespace LuShop.Api.Endpoints;

public static class Endpoint
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("");
        
        // --- HEALTH CHECK ---
        endpoints.MapGroup("/")
            .WithTags("Health Check")
            .MapGet("/", () => "OK");
        
        // --- IDENTITY (AUTENTICAÇÃO) ---
        endpoints.MapGroup("v1/identity")
            .WithTags("Identity")
            .MapIdentityApi<User>();
            
        endpoints.MapGroup("v1/identity")
            .WithTags("Identity")
            .MapEndpoint<LogoutEndpoint>()
            .MapEndpoint<GetRolesEndpoint>();

        // --- CARTS (CARRINHO) ---
        // Requer usuário logado
        endpoints.MapGroup("v1/carts")
            .WithTags("Carts")
            .RequireAuthorization()
            .MapEndpoint<GetCartEndpoint>()
            .MapEndpoint<AddItemEndpoint>()
            .MapEndpoint<UpdateItemEndpoint>()
            .MapEndpoint<RemoveItemEndpoint>()
            .MapEndpoint<ClearCartEndpoint>();
        
        // --- CATEGORIES (CATEGORIAS) ---
        // Público: Listar e Detalhes
        var categoriesPublic = endpoints.MapGroup("v1/categories")
            .WithTags("Categories");
        categoriesPublic.MapEndpoint<GetAllCategoriesEndpoint>();
        categoriesPublic.MapEndpoint<GetCategoryByIdEndpoint>();

        // Admin: Criar, Editar, Excluir
        var categoriesAdmin = endpoints.MapGroup("v1/categories")
            .WithTags("Categories")
            .RequireAuthorization("Admin");
        categoriesAdmin.MapEndpoint<CreateCategoryEndpoint>();
        categoriesAdmin.MapEndpoint<UpdateCategoryEndpoint>();
        categoriesAdmin.MapEndpoint<DeleteCategoryEndpoint>();

        // --- VOUCHERS (CUPONS) ---
        // Público: Apenas validar código específico
        var vouchersPublic = endpoints.MapGroup("v1/vouchers")
            .WithTags("Vouchers");
        vouchersPublic.MapEndpoint<GetVoucherByNumberEndpoint>();

        // Admin: Gestão completa e listagem
        var vouchersAdmin = endpoints.MapGroup("v1/vouchers")
            .WithTags("Vouchers")
            .RequireAuthorization("Admin");
        vouchersAdmin.MapEndpoint<CreateVoucherEndpoint>();
        vouchersAdmin.MapEndpoint<GetAllVouchersEndpoint>();
        vouchersAdmin.MapEndpoint<UpdateVoucherEndpoint>();
        vouchersAdmin.MapEndpoint<DeleteVoucherEndpoint>();

        // --- ORDERS (PEDIDOS) ---
        // Requer usuário logado
        endpoints.MapGroup("v1/orders")
            .WithTags("Orders")
            .RequireAuthorization() 
            .MapEndpoint<CreateOrderEndpoint>()
            .MapEndpoint<PayOrderEndpoint>()
            .MapEndpoint<RefundOrderEndpoint>()
            .MapEndpoint<CancelOrderEndpoint>()
            .MapEndpoint<GetAllOrdersEndpoint>()
            .MapEndpoint<GetOrderByNumberEndpoint>();

        // --- PRODUCTS (PRODUTOS) ---
        // Público: Vitrine
        var productsPublic = endpoints.MapGroup("v1/products")
            .WithTags("Products");
        productsPublic.MapEndpoint<GetAllProductsEndpoint>();
        productsPublic.MapEndpoint<GetProductBySlugEndpoint>();

        // Admin: Gestão
        var productsAdmin = endpoints.MapGroup("v1/products")
            .WithTags("Products")
            .RequireAuthorization("Admin"); 
        productsAdmin.MapEndpoint<CreateProductEndpoint>();
        productsAdmin.MapEndpoint<UpdateProductEndpoint>();
        productsAdmin.MapEndpoint<DeleteProductEndpoint>();

        // --- STRIPE (PAGAMENTOS) ---
        endpoints.MapGroup("v1/stripe")
            .WithTags("Stripe")
            .MapEndpoint<CreateSessionEndpoint>()       
            .MapEndpoint<GetTransactionByOrderNumberEndpoint>()  
            .MapEndpoint<StripeWebhookEndpoint>();       
    }
    
    // Método de extensão que mapeia os endpoints com a interface IEndpoint
    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}