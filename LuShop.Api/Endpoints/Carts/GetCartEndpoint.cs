using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Carts;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Carts;

public class GetCartEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Carts: Get")
            .WithSummary("Obtém o carrinho do usuário")
            .WithDescription("Retorna o carrinho de compras atual do usuário autenticado")
            .WithOrder(1)
            .Produces<Response<Cart?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        ICartHandler handler)
    {
        var request = new GetCartRequest 
        { 
            UserId = user.Identity?.Name ?? string.Empty 
        };

        var result = await handler.GetByUserAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}