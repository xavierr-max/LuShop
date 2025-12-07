using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Orders;

public class RefundOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id}/refund", HandleAsync)
            .WithName("Orders: Refund")
            .WithSummary("Reembolsa um pedido")
            .WithDescription("Realiza o estorno financeiro de um pedido que já foi pago")
            .WithOrder(4)
            .RequireAuthorization() // 👈 Adicione autenticação
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IOrderHandler handler,
        [FromRoute] long id)
    {
        // Extrai o UserId do token
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
                     ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                     ?? user.Identity?.Name 
                     ?? string.Empty;

        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.BadRequest("Usuário não identificado");
        }

        var request = new RefundOrderRequest
        {
            Id = id,
            UserId = userId // 👈 Preenche o UserId
        };

        var result = await handler.RefundAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}