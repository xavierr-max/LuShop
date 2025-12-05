using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Products;

public class DeleteProductEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id}", HandleAsync)
            .WithName("Products: Delete")
            .WithSummary("Exclui um produto")
            .WithDescription("Exclui um produto do catálogo. Se houver vendas vinculadas, a exclusão será bloqueada.")
            .WithOrder(3)
            .Produces<Response<Product?>>();

    private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        long id) // O Minimal API pega o {id} da URL automaticamente
    {
        var request = new DeleteProductRequest
        {
            Id = id
        };

        var result = await handler.DeleteAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}