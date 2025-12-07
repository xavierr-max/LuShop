using LuShop.Core.Handlers;
using LuShop.Core.Requests.Stripe;
using LuShop.Core.Responses;
using LuShop.Core.Responses.Stripe;
using System.Net.Http.Json;

namespace LuShop.Web.Handlers;

public class StripeHandler(IHttpClientFactory httpClientFactory) : IStripeHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("lushop");
    private const string BaseUrl = "v1/stripe";

    public async Task<Response<string?>> CreateSessionAsync(CreateSessionRequest request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"{BaseUrl}/session", request);
            var result = await response.Content.ReadFromJsonAsync<Response<string?>>();
            
            return result ?? new Response<string?>(null, (int)response.StatusCode, "Erro ao criar sessão.");
        }
        catch (Exception ex)
        {
            return new Response<string?>(null, 500, $"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<Response<List<StripeTransactionResponse>>> GetTransactionsByOrderNumberAsync(GetTransactionByOrderNumberRequest request)
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrl}/transactions/{request.OrderNumber}");
            var result = await response.Content.ReadFromJsonAsync<Response<List<StripeTransactionResponse>>>();
            
            return result ?? new Response<List<StripeTransactionResponse>>(null, (int)response.StatusCode, "Falha ao buscar transações.");
        }
        catch (Exception ex)
        {
            return new Response<List<StripeTransactionResponse>>(null, 500, $"Erro na requisição: {ex.Message}");
        }
    }
}