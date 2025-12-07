using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Vouchers;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Vouchers;

public class GetAllVouchersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Vouchers: Get All")
            .WithSummary("Recupera todos os vouchers")
            .WithDescription("Recupera a lista de vouchers paginada")
            .WithOrder(2)
            .Produces<Response<List<Voucher>?>>();

    private static async Task<IResult> HandleAsync(
        IVoucherHandler handler,
        [AsParameters] GetAllVouchersRequest request)
    {
        var result = await handler.GetAllAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}