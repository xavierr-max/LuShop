using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Orders;

public class PayOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{number}/pay", HandleAsync)
            .WithName("Orders: Pay")
            .WithSummary("Realiza o pagamento de um pedido")
            .WithDescription("Marca um pedido como pago e registra o ID da transação do Gateway")
            .WithOrder(3)
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IOrderHandler handler,
        string number, // Vem da URL: v1/orders/ORD-1234/pay
        [FromBody] PayOrderRequest request) // Vem do Corpo (JSON): { "externalReference": "txn_999" }
    {
        // Força o OrderNumber ser o mesmo da URL para garantir segurança
        // (Evita que o usuário mande URL do pedido A e JSON do pedido B)
        request.OrderNumber = number;
        
        // Opcional: Se quiser registrar quem pagou
        // request.UserId = user.Identity?.Name ?? string.Empty;

        var result = await handler.PayAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}