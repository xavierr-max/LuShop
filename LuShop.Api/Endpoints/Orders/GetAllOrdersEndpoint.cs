using System.Security.Claims;
using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Orders;

public class GetAllOrdersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Orders: Get All")
            .WithSummary("Recupera todos os pedidos")
            .WithDescription("Recupera uma lista paginada de pedidos do usuário logado")
            .WithOrder(5)
            .RequireAuthorization()
            .Produces<PagedResponse<List<Order>?>>();

    private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        ClaimsPrincipal user,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25)
    {
        // Extrai o UserId do token JWT
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
                     ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                     ?? user.Identity?.Name 
                     ?? string.Empty;

        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.BadRequest("Usuário não identificado");
        }

        // Cria o request manualmente
        var request = new GetAllOrdersRequest
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await handler.GetAllAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}