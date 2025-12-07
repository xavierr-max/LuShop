using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.CartItem;

public class RemoveItemEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/item/{id:long}", HandleAsync) // MUDANÇA: Adicionado "/item" para consistência
            .WithName("Carts: Remove Item")
            .WithSummary("Remove um item do carrinho")
            .WithDescription("Remove um produto específico do carrinho de compras")
            .WithOrder(4)
            .Produces<Response<Cart?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        [FromServices] ICartHandler handler,
        long id)
    {
        var request = new RemoveCartItemRequest
        {
            UserId = user.Identity?.Name ?? string.Empty,
            CartItemId = id
        };

        var result = await handler.RemoveItemAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}