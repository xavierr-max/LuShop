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
        => app.MapPost("/{orderNumber}/pay", HandleAsync)
            .WithName("Orders: Pay")
            .WithSummary("Inicia o pagamento de um pedido")
            .WithDescription("Cria uma sessão de pagamento no Stripe e retorna o Session ID")
            .WithOrder(2)
            .RequireAuthorization()
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IOrderHandler handler,
        [FromRoute] string orderNumber,
        [FromBody] PayOrderRequest request)
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
                     ?? user.Identity?.Name 
                     ?? string.Empty;
        
        request.UserId = userId;
        request.OrderNumber = orderNumber;
        
        var result = await handler.PayAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}