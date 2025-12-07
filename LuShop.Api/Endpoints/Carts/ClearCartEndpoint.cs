using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Carts;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Carts;

public class ClearCartEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/", HandleAsync)
            .WithName("Carts: Clear")
            .WithSummary("Limpa o carrinho")
            .WithDescription("Remove todos os itens do carrinho do usuário")
            .WithOrder(5)
            .Produces<Response<Cart?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        ICartHandler handler)
    {
        var request = new ClearCartRequest 
        { 
            UserId = user.Identity?.Name ?? string.Empty 
        };

        var result = await handler.ClearAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}