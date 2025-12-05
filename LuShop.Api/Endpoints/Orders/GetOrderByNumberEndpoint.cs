using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Orders;

public class GetOrderByNumberEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{number}", HandleAsync)
            .WithName("Orders: Get By Number")
            .WithSummary("Recupera um pedido pelo número")
            .WithDescription("Busca os detalhes completos de um pedido (incluindo itens) através do seu código")
            .WithOrder(6)
            .Produces<Response<Order?>>();

    private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        string number) // O Minimal API pega o {number} da URL automaticamente
    {
        var request = new GetOrderByNumberRequest
        {
            Number = number
        };

        var result = await handler.GetByNumberAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(result); 
        // Se der erro, geralmente é 404 (Não encontrado), então retornamos NotFound
    }
}