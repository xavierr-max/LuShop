using LuShop.Api.Data;
using LuShop.Core.Enums;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Order;
using LuShop.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Handlers;

public class OrderHandler(AppDbContext context) : IOrderHandler
{
    public async Task<Response<Order?>> CreateAsync(CreateOrderRequest request)
    {
        Voucher? voucher = null;
        if (voucher is null)
            return new Response<Order?>(null, 400, "não foi possível criar seu pedido");
        
        voucher = await context.Vouchers
            .FirstOrDefaultAsync(x => x.Id == request.VoucherId && x.IsActive == true);

        var order = new Order
        {
            Gateway = request.Gateway,
            Voucher = voucher,
            CreatedAt = DateTime.Now,
            Status = EOrderStatus.WaitingPayment
        };

        foreach (var itemRequest in request.Items)
        {
            var product = await context.Products
                .FindAsync(itemRequest.ProductId);

            if (product is null)
                return new Response<Order?>(null, 400, "não foi possível encontrar o produto");
    
            // Você cria o objeto OrderItem manualmente aqui
            var item = new OrderItem
            {
                Product = product,
                Price = product.Price, // Você copia o preço manualmente aqui
                Quantity = itemRequest.Quantity
            };

            // E adiciona diretamente na lista pública
            order.Items.Add(item);
        }

        try
        {
            // Adiciona o pedido ao DbSet (o EF rastreia os itens automaticamente)
            await context.Orders.AddAsync(order);
            
            // Comita a transação no banco
            await context.SaveChangesAsync();

            return new Response<Order?>(order, 201, "Pedido criado com sucesso!");
        }
        catch (Exception ex)
        {
            // Logar o erro 'ex' aqui se tiver logger
            return new Response<Order?>(null, 500, "Falha interna ao criar o pedido.");
        }
    }

    public async Task<Response<Order?>> PayAsync(PayOrderRequest request)
    {
        try
        {
            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Number == request.OrderNumber);

            if (order is null)
                return new Response<Order?>(null, 404, "Pedido não encontrado.");
            
            // Verifica o status ATUAL do pedido antes de tentar pagar
            switch (order.Status)
            {
                // Casos impeditivos (Retornam erro imediatamente)
                case EOrderStatus.Paid:
                    return new Response<Order?>(order, 400, "Este pedido já está pago.");

                case EOrderStatus.Canceled:
                    return new Response<Order?>(order, 400, "Pedidos cancelados não podem ser pagos.");

                case EOrderStatus.Refunded:
                    return new Response<Order?>(order, 400, "Não é possível pagar um pedido que já foi reembolsado.");

                // Caso de sucesso (O único status que permite pagamento)
                case EOrderStatus.WaitingPayment:
                    // A execução sai do switch e continua abaixo
                    break;

                // Caso default (segurança para status desconhecidos)
                default:
                    return new Response<Order?>(null, 400, "Status do pedido inválido para pagamento.");
            }

            // Se o código chegou aqui, significa que caiu no 'case EOrderStatus.WaitingPayment'
            // e deu o 'break'. Agora executamos o pagamento.
        
            order.Status = EOrderStatus.Paid;
            order.ExternalReference = request.ExternalReference;
            order.UpdatedAt = DateTime.Now;

            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new Response<Order?>(order, 200, "Pagamento confirmado!");
        }
        catch
        {
            return new Response<Order?>(null, 500, "Falha ao processar pagamento.");
        }
    }

    public async Task<Response<Order?>> CancelAsync(CancelOrderRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response<Order?>> RefundAsync(RefundOrderRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response<Order?>> GetByNumberAsync(GetOrderByNumberRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<List<Order>?>> GetAllAsync(GetAllOrdersRequest request)
    {
        throw new NotImplementedException();
    }
}