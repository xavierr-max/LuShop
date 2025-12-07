using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.CartItem;

public class AddItemEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/item", HandleAsync) // MUDANÇA: Adicionado "/item" para casar com o frontend
            .WithName("Carts: Add Item")
            .WithSummary("Adiciona um item ao carrinho")
            .WithDescription("Adiciona um novo produto ou incrementa a quantidade se já existir")
            .WithOrder(2)
            .Produces<Response<Cart?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        [FromServices] ICartHandler handler,
        AddCartItemRequest request)
    {
        request.UserId = user.Identity?.Name ?? string.Empty;

        var result = await handler.AddItemAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}