using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Products;

public class GetProductBySlugEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{slug}", HandleAsync)
            .WithName("Products: Get By Slug")
            .WithSummary("Recupera um produto pelo slug (URL)")
            .WithDescription("Busca os detalhes públicos de um produto ativo usando seu identificador amigável de URL")
            .WithOrder(6)
            .Produces<Response<Product?>>();

    private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        string slug) // O Minimal API pega o {slug} da URL automaticamente
    {
        var request = new GetProductBySlugRequest
        {
            Slug = slug
        };

        var result = await handler.GetBySlugAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(result);
        // Retorna 404 se o produto não existir ou estiver inativo (devido ao filtro do Handler)
    }
}