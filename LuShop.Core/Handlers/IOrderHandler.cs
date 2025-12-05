using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Responses;

namespace LuShop.Core.Handlers;

public interface IOrderHandler
{
    Task<Response<Order?>> CreateAsync(CreateOrderRequest request);
    Task<Response<Order?>> PayAsync(PayOrderRequest request);
    Task<Response<Order?>> CancelAsync(CancelOrderRequest request);
    Task<Response<Order?>> RefundAsync(RefundOrderRequest request);

    // Leitura (Queries)
    Task<Response<Order?>> GetByNumberAsync(GetOrderByNumberRequest request);
    
    // Note o PagedResponse para listas paginadas
    Task<PagedResponse<List<Order>?>> GetAllAsync(GetAllOrdersRequest request);
}