using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products; 
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Products;

public class CreateProductEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", HandleAsync)
            .WithName("Products: Create")
            .WithSummary("Cria um novo produto")
            .WithDescription("Cria um novo produto no catálogo")
            .WithOrder(1)
            .Produces<Response<Product?>>();

    private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        CreateProductRequest request)
    {
        // Aqui chamamos o handler.
        // Lembre-se: O Handler já tem a lógica de gerar o Slug automaticamente
        // baseado no título, então o front não precisa enviar o slug perfeito.
        
        var result = await handler.CreateAsync(request);

        return result.IsSuccess
            // Retorna 201 Created com a URL para acessar o produto pelo Slug
            ? TypedResults.Created($"/{result.Data?.Slug}", result)
            : TypedResults.BadRequest(result);
    }
}