using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders; // Namespace corrigido (Plural)
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Orders;

public class GetAllOrdersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Orders: Get All")
            .WithSummary("Recupera todos os pedidos")
            .WithDescription("Recupera uma lista paginada de pedidos ordenados por data")
            .WithOrder(5)
            .Produces<PagedResponse<List<Order>?>>();

    private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        [AsParameters] GetAllOrdersRequest request)
    {
        // [AsParameters] é vital aqui!
        // Ele diz ao ASP.NET: "Pegue os valores de PageNumber e PageSize da URL 
        // (ex: ?pageNumber=2&pageSize=10) e preencha este objeto request".
        
        var result = await handler.GetAllAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}