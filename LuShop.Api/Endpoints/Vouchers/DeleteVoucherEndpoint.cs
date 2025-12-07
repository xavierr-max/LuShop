using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Vouchers;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Vouchers;

public class DeleteVoucherEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:long}", HandleAsync)
            .WithName("Vouchers: Delete")
            .WithSummary("Exclui um voucher")
            .WithDescription("Remove um voucher do sistema")
            .WithOrder(4)
            .Produces<Response<Voucher?>>();

    private static async Task<IResult> HandleAsync(
        IVoucherHandler handler,
        long id)
    {
        var request = new DeleteVoucherRequest { Id = id };
        
        var result = await handler.DeleteAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}