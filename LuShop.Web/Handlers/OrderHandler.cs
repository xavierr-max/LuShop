using System.Net.Http.Json;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;

namespace LuShop.Web.Handlers;

public class OrderHandler(IHttpClientFactory httpClientFactory) : IOrderHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);
    private const string BaseUrl = "v1/orders";

    public async Task<Response<Order?>> CreateAsync(CreateOrderRequest request)
    {
        var result = await _client.PostAsJsonAsync(BaseUrl, request);
        return await result.Content.ReadFromJsonAsync<Response<Order?>>()
               ?? new Response<Order?>(null, 400, "Falha ao criar o pedido.");
    }

    public async Task<Response<Order?>> PayAsync(PayOrderRequest request)
    {
        var result = await _client.PostAsJsonAsync($"{BaseUrl}/{request.OrderNumber}/pay", request);
        return await result.Content.ReadFromJsonAsync<Response<Order?>>()
               ?? new Response<Order?>(null, 400, "Falha ao processar o pagamento.");
    }

    public async Task<Response<Order?>> CancelAsync(CancelOrderRequest request)
    {
        var result = await _client.PostAsJsonAsync($"{BaseUrl}/{request.Id}/cancel", request);
        return await result.Content.ReadFromJsonAsync<Response<Order?>>()
               ?? new Response<Order?>(null, 400, "Falha ao cancelar o pedido.");
    }

    public async Task<Response<Order?>> RefundAsync(RefundOrderRequest request)
    {
        var result = await _client.PostAsJsonAsync($"{BaseUrl}/{request.Id}/refund", request);
        return await result.Content.ReadFromJsonAsync<Response<Order?>>()
               ?? new Response<Order?>(null, 400, "Falha ao solicitar reembolso.");
    }

    public async Task<Response<Order?>> GetByNumberAsync(GetOrderByNumberRequest request)
    {
        return await _client.GetFromJsonAsync<Response<Order?>>($"{BaseUrl}/{request.Number}")
               ?? new Response<Order?>(null, 404, "Pedido não encontrado.");
    }

    public async Task<PagedResponse<List<Order>?>> GetAllAsync(GetAllOrdersRequest request)
    {
        var url = $"{BaseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}";
        return await _client.GetFromJsonAsync<PagedResponse<List<Order>?>>(url)
               ?? new PagedResponse<List<Order>?>(null, 400, "Não foi possível buscar os pedidos.");
    }
}