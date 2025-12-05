using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;

// Ajuste para o seu namespace de IEndpoint

// Necessário para [FromRoute] se quiser ser explícito, ou deixe o Minimal API inferir

namespace LuShop.Api.Endpoints.Orders;

public class CancelOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id}/cancel", HandleAsync)
            .WithName("Orders: Cancel")
            .WithSummary("Cancela um pedido")
            .WithDescription("Cancela um pedido existente baseado no ID")
            .WithOrder(1)
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        long id) // O Minimal API pega o {id} da URL automaticamente
    {
        // Montamos o Request com o ID que veio da Rota
        var request = new CancelOrderRequest 
        { 
            Id = id,
        };

        var result = await handler.CancelAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}