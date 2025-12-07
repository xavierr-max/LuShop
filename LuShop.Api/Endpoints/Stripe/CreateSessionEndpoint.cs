using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Requests.Stripe;
using Microsoft.AspNetCore.Mvc;

namespace LuShop.Api.Endpoints.Stripe;

public class CreateSessionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/v1/stripe/session", HandleAsync)
            .WithName("Stripe: Create Session")
            .WithSummary("Cria uma sessão de checkout no Stripe")
            .WithDescription("Cria uma sessão e retorna o ID para redirecionamento");

    private static async Task<IResult> HandleAsync(
        [FromBody] CreateSessionRequest request,
        [FromServices] IStripeHandler handler)
    {
        var result = await handler.CreateSessionAsync(request);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}