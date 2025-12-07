using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Vouchers;
using LuShop.Core.Responses;

namespace LuShop.Api.Endpoints.Vouchers;

public class UpdateVoucherEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:long}", HandleAsync)
            .WithName("Vouchers: Update")
            .WithSummary("Atualiza um voucher")
            .WithDescription("Atualiza os dados de um voucher existente")
            .WithOrder(3)
            .Produces<Response<Voucher?>>();

    private static async Task<IResult> HandleAsync(
        IVoucherHandler handler,
        UpdateVoucherRequest request,
        long id)
    {
        request.Id = id;
        
        var result = await handler.UpdateAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}