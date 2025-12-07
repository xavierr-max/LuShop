using LuShop.Core.Handlers;
using LuShop.Core.Requests.Stripe;
using LuShop.Core.Responses;
using LuShop.Core.Responses.Stripe;
using Stripe;
using Stripe.Checkout;

namespace LuShop.Api.Handlers;

public class StripeHandler : IStripeHandler
{
    public async Task<Response<string?>> CreateSessionAsync(CreateSessionRequest request)
    {
        var options = new SessionCreateOptions
        {
            CustomerEmail = request.CustomerEmail,
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = request.Amount,
                        Currency = request.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.ProductTitle,
                            Description = request.ProductDescription
                        },
                    },
                    Quantity = 1,
                }
            ],
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            ClientReferenceId = request.OrderNumber,
            Metadata = new Dictionary<string, string>
            {
                { "order_number", request.OrderNumber }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return new Response<string?>(session.Id, 201, "Sessão criada");
    }

    public async Task<Response<List<StripeTransactionResponse>>> GetTransactionsByOrderNumberAsync(
        GetTransactionByOrderNumberRequest request)
    {
        try
        {
            var data = new List<StripeTransactionResponse>();

            // Estratégia 1: Listar todas as Sessions recentes e filtrar por ClientReferenceId
            var sessionService = new SessionService();
            var sessionListOptions = new SessionListOptions
            {
                Limit = 100
            };

            var sessions = await sessionService.ListAsync(sessionListOptions);
            var matchingSessions = sessions.Data
                .Where(s => s.ClientReferenceId == request.OrderNumber)
                .ToList();

            if (matchingSessions.Count > 0)
            {
                foreach (var session in matchingSessions)
                {
                    if (!string.IsNullOrEmpty(session.PaymentIntentId))
                    {
                        var paymentIntentService = new PaymentIntentService();
                        var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

                        if (paymentIntent.LatestChargeId != null)
                        {
                            var chargeService = new ChargeService();
                            var charge = await chargeService.GetAsync(paymentIntent.LatestChargeId);

                            data.Add(new StripeTransactionResponse
                            {
                                Id = charge.Id,
                                Email = charge.BillingDetails?.Email ?? session.CustomerEmail ?? "N/A",
                                Amount = charge.Amount,
                                AmountCaptured = charge.AmountCaptured,
                                Status = charge.Status,
                                Paid = charge.Paid,
                                Refunded = charge.Refunded,
                            });
                        }
                        else
                        {
                            data.Add(new StripeTransactionResponse
                            {
                                Id = paymentIntent.Id,
                                Email = session.CustomerEmail ?? "N/A",
                                Amount = paymentIntent.Amount,
                                AmountCaptured = paymentIntent.AmountCapturable,
                                Status = paymentIntent.Status,
                                Paid = paymentIntent.Status == "succeeded",
                                Refunded = false,
                            });
                        }
                    }
                }
            }

            // Estratégia 2: Buscar diretamente por Charges usando metadata
            if (data.Count == 0)
            {
                var chargeService = new ChargeService();
                var chargeListOptions = new ChargeListOptions
                {
                    Limit = 100
                };

                var charges = await chargeService.ListAsync(chargeListOptions);
                var matchingCharges = charges.Data
                    .Where(c => c.Metadata.ContainsKey("order_number") && 
                                c.Metadata["order_number"] == request.OrderNumber)
                    .ToList();

                foreach (var charge in matchingCharges)
                {
                    data.Add(new StripeTransactionResponse
                    {
                        Id = charge.Id,
                        Email = charge.BillingDetails?.Email ?? "N/A",
                        Amount = charge.Amount,
                        AmountCaptured = charge.AmountCaptured,
                        Status = charge.Status,
                        Paid = charge.Paid,
                        Refunded = charge.Refunded,
                    });
                }
            }

            if (data.Count == 0)
            {
                return new Response<List<StripeTransactionResponse>>(null, 404, "Transação não encontrada no Stripe. O pagamento pode ainda não ter sido processado.");
            }

            return new Response<List<StripeTransactionResponse>>(data, 200, "Transações encontradas");
        }
        catch (StripeException ex)
        {
            return new Response<List<StripeTransactionResponse>>(null, 500, $"Erro do Stripe: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new Response<List<StripeTransactionResponse>>(null, 500, $"Erro ao buscar transações: {ex.Message}");
        }
    }
}