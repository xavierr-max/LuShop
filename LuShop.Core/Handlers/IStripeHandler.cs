using LuShop.Core.Requests.Stripe;
using LuShop.Core.Responses;
using LuShop.Core.Responses.Stripe;

namespace LuShop.Core.Handlers;

public interface IStripeHandler
{
    Task<Response<string?>> CreateSessionAsync(CreateSessionRequest request);
    Task<Response<List<StripeTransactionResponse>>> GetTransactionsByOrderNumberAsync(GetTransactionByOrderNumberRequest request);
}