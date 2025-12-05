using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Orders;

public class RefundOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id}/refund", HandleAsync)
            .WithName("Orders: Refund")
            .WithSummary("Reembolsa um pedido")
            .WithDescription("Realiza o estorno financeiro de um pedido que já foi pago")
            .WithOrder(4)
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        long id) // O Minimal API pega o {id} da URL automaticamente
    {
        var request = new RefundOrderRequest
        {
            Id = id
        };

        var result = await handler.RefundAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}