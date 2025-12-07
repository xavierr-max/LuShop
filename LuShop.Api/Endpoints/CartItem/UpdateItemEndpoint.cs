using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.CartItem;

public class UpdateItemEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/item/{id:long}", HandleAsync) // MUDANÇA: Adicionado "/item" para consistência
            .WithName("Carts: Update Item")
            .WithSummary("Atualiza um item do carrinho")
            .WithDescription("Atualiza a quantidade de um item específico no carrinho")
            .WithOrder(3)
            .Produces<Response<Cart?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        [FromServices] ICartHandler handler,
        long id,
        UpdateCartItemRequest request)
    {
        request.UserId = user.Identity?.Name ?? string.Empty;
        request.CartItemId = id;

        var result = await handler.UpdateItemAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}