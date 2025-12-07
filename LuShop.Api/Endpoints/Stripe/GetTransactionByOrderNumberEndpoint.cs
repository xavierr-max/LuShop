using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Requests.Stripe;
using LuShop.Core.Responses.Stripe;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Stripe;

public class GetTransactionByOrderNumberEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/transactions/{orderNumber}", HandleAsync)
            .WithName("Stripe: Get Transactions")
            .WithSummary("Busca transações por número do pedido");
    
    // CORREÇÃO 2: Recebemos 'string' do ASP.NET e criamos o Request manualmente
    private static async Task<IResult> HandleAsync(
        [FromRoute] string orderNumber,
        [FromServices] IStripeHandler handler)
    {
        var request = new GetTransactionByOrderNumberRequest 
        { 
            OrderNumber = orderNumber 
        };

        var result = await handler.GetTransactionsByOrderNumberAsync(request);
        
        // Se encontrar, retorna 200 (OK). 
        // Se não encontrar (Count == 0), o Handler retorna IsSuccess=false, gerando 400 (BadRequest).
        return result.IsSuccess 
            ? TypedResults.Ok(result) 
            : TypedResults.BadRequest(result);
    }
}