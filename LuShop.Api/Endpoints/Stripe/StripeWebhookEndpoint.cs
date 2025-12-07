using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Requests.Orders;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace LuShop.Api.Endpoints.Stripe;

public class StripeWebhookEndpoint : IEndpoint
{
    private const string WebhookSecret = "whsec_SUA_CHAVE_AQUI"; 

    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/webhook", HandleAsync)
            .WithName("Stripe: Webhook")
            .WithSummary("Recebe notificações do Stripe")
            .AllowAnonymous()
            .ExcludeFromDescription();

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IOrderHandler orderHandler)
    {
        var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            var stripeSignature = httpContext.Request.Headers["Stripe-Signature"].ToString();
            
            // 🔧 CORREÇÃO 1: Remover o parâmetro booleano (throwOnApiVersionMismatch)
            var stripeEvent = EventUtility.ConstructEvent(
                json, 
                stripeSignature, 
                WebhookSecret
            );

            // 🔧 CORREÇÃO 2: Usar string literal ao invés de Events.CheckoutSessionCompleted
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                
                if (session?.ClientReferenceId != null)
                {
                    Console.WriteLine($"[Webhook] Pagamento confirmado para pedido: {session.ClientReferenceId}");
                    
                    await orderHandler.PayAsync(new PayOrderRequest
                    {
                        OrderNumber = session.ClientReferenceId,
                        ExternalReference = "confirmed",
                        Email = session.CustomerEmail,
                        UserId = string.Empty
                    });
                }
            }

            return TypedResults.Ok();
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"[Webhook] Erro: {ex.Message}");
            return TypedResults.BadRequest(ex.Message);
        }
    }
}