using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;


namespace LuShop.Api.Endpoints.Orders;

public class CreateOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", HandleAsync)
            .WithName("Orders: Create")
            .WithSummary("Cria um novo pedido")
            .WithDescription("Cria um novo pedido contendo itens e opcionalmente um voucher")
            .WithOrder(1)
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IOrderHandler handler,
        CreateOrderRequest request)
    {
        // Preenche o ID do usuário automaticamente baseado no Token JWT
        // (Assumindo que sua classe base 'Request' tenha a propriedade UserId)
        request.UserId = user.Identity?.Name ?? string.Empty;

        var result = await handler.CreateAsync(request);

        return result.IsSuccess
            // Retorna 201 Created com o Location Header apontando para o número do pedido
            ? TypedResults.Created($"/{result.Data?.Number}", result)
            : TypedResults.BadRequest(result);
    }
}