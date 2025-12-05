using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Products;

public class UpdateProductEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
            .WithName("Products: Update")
            .WithSummary("Atualiza um produto")
            .WithDescription("Atualiza os dados de um produto existente")
            .WithOrder(2)
            .Produces<Response<Product?>>();

    private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        long id, // Vem da URL: v1/products/50
        [FromBody] UpdateProductRequest request) // Vem do Corpo (JSON)
    {
        // Garante que o ID do objeto request seja o mesmo da URL
        request.Id = id;

        // Opcional: Se você tiver UserId no request para auditar quem alterou
        // request.UserId = user.Identity?.Name ?? string.Empty;

        var result = await handler.UpdateAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}