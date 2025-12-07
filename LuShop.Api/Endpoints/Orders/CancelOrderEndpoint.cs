using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Orders;

public class CancelOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id}/cancel", HandleAsync)
            .WithName("Orders: Cancel")
            .WithSummary("Cancela um pedido")
            .WithDescription("Cancela um pedido existente baseado no ID")
            .WithOrder(3)
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

        var request = new CancelOrderRequest 
        { 
            Id = id,
            UserId = userId // 👈 Preenche o UserId
        };

        var result = await handler.CancelAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}