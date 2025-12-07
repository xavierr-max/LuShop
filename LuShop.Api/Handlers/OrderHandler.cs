using LuShop.Api.Data;
using LuShop.Core.Enums;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Requests.Stripe;
using LuShop.Core.Responses;
using Microsoft.EntityFrameworkCore;
using LuShop.Api.Common.Api;
using LuShop.Core; // Para acessar Configuration.FrontendUrl

namespace LuShop.Api.Handlers;

public class OrderHandler(
    AppDbContext context, 
    IStripeHandler stripeHandler) : IOrderHandler
{
    public async Task<Response<Order?>> CreateAsync(CreateOrderRequest request)
    {
        Voucher? voucher = null;
        if (request.VoucherId.HasValue)
        {
            voucher = await context.Vouchers
                .FirstOrDefaultAsync(x => x.Id == request.VoucherId.Value && x.IsActive);

            if (voucher is null)
                return new Response<Order?>(null, 400, "Voucher inválido ou inativo.");
        }

        var order = new Order
        {
            UserId = request.UserId,
            Gateway = request.Gateway,
            Voucher = voucher,
            CreatedAt = DateTime.Now,
            Status = EOrderStatus.WaitingPayment
        };

        foreach (var itemRequest in request.Items)
        {
            var product = await context.Products.FindAsync(itemRequest.ProductId);
            if (product is null) continue;

            var item = new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Price = product.Price,
                Quantity = itemRequest.Quantity
            };
            order.Items.Add(item);
        }

        try
        {
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();
            return new Response<Order?>(order, 201, "Pedido criado com sucesso!");
        }
        catch
        {
            return new Response<Order?>(null, 500, "Falha interna ao criar o pedido.");
        }
    }

    public async Task<Response<Order?>> PayAsync(PayOrderRequest request)
    {
        try
        {
            var order = await context.Orders
                .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                .Include(x => x.Voucher)
                .FirstOrDefaultAsync(x => x.Number == request.OrderNumber);

            if (order is null)
                return new Response<Order?>(null, 404, "Pedido não encontrado.");

            // =======================================================================
            // NOVA LÓGICA: INTERCEPTA A CONFIRMAÇÃO
            // =======================================================================
            if (request.ExternalReference == "confirmed")
            {
                // Se o pedido já estiver pago, retorna sucesso mas não faz nada
                if (order.Status == EOrderStatus.Paid)
                    return new Response<Order?>(order, 200, "Pedido já pago.");

                // MUDANÇA DE STATUS NO BANCO
                order.Status = EOrderStatus.Paid; 
                order.ExternalReference = $"CONFIRMADO_{DateTime.Now.Ticks}"; // Registro simples
                order.UpdatedAt = DateTime.Now;

                context.Orders.Update(order);
                await context.SaveChangesAsync();

                return new Response<Order?>(order, 200, "Pagamento confirmado!");
            }
            // =======================================================================

            // Validação padrão para CRIAÇÃO DE SESSÃO (Fluxo normal)
            switch (order.Status)
            {
                case EOrderStatus.Paid:
                    return new Response<Order?>(order, 400, "Este pedido já está pago.");
                case EOrderStatus.Canceled:
                    return new Response<Order?>(order, 400, "Pedidos cancelados não podem ser pagos.");
                case EOrderStatus.Refunded:
                    return new Response<Order?>(order, 400, "Não é possível pagar um pedido reembolsado.");
            }

            // Criação da Sessão no Stripe
            var frontendUrl = Configuration.FrontendUrl; 
            var customerEmail = request.Email ?? string.Empty;

            var stripeRequest = new CreateSessionRequest
            {
                OrderNumber = order.Number,
                Amount = (long)(order.Total * 100),
                Currency = "brl",
                ProductTitle = $"Pedido #{order.Number}",
                ProductDescription = $"Compra LuShop - {order.Items.Count} itens",
                CustomerEmail = customerEmail,
                SuccessUrl = $"{frontendUrl}/pedidos/{order.Number}/pagamento?success=true",
                CancelUrl = $"{frontendUrl}/pedidos/{order.Number}/pagamento?canceled=true"
            };

            var stripeResult = await stripeHandler.CreateSessionAsync(stripeRequest);

            if (!stripeResult.IsSuccess)
                return new Response<Order?>(null, 500, stripeResult.Message);

            order.ExternalReference = stripeResult.Data;
            return new Response<Order?>(order, 200, "Sessão criada.");
        }
        catch (Exception ex)
        {
            return new Response<Order?>(null, 500, $"Erro: {ex.Message}");
        }
    }

    public async Task<Response<Order?>> CancelAsync(CancelOrderRequest request) 
    {
        var order = await context.Orders.FindAsync(request.Id);
        if (order is null) 
            return new Response<Order?>(null, 404, "Pedido não encontrado.");
    
        // 🔒 Validação: Usuário só pode cancelar seus próprios pedidos
        if (order.UserId != request.UserId)
            return new Response<Order?>(null, 403, "Você não tem permissão para cancelar este pedido.");
    
        // 🔒 Validação: Só pode cancelar se estiver aguardando pagamento
        if (order.Status != EOrderStatus.WaitingPayment)
            return new Response<Order?>(null, 400, "Apenas pedidos pendentes podem ser cancelados.");
    
        order.Status = EOrderStatus.Canceled;
        order.UpdatedAt = DateTime.Now;
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    
        return new Response<Order?>(order, 200, "Pedido cancelado com sucesso.");
    }

    public async Task<Response<Order?>> RefundAsync(RefundOrderRequest request) 
    {
        var order = await context.Orders.FindAsync(request.Id);
        if (order is null) 
            return new Response<Order?>(null, 404, "Pedido não encontrado.");

        // 🔒 Validação: Usuário só pode estornar seus próprios pedidos
        if (order.UserId != request.UserId)
            return new Response<Order?>(null, 403, "Você não tem permissão para estornar este pedido.");
    
        // 🔒 Validação: Só pode estornar se estiver pago
        if (order.Status != EOrderStatus.Paid)
            return new Response<Order?>(null, 400, "Apenas pedidos pagos podem ser estornados.");

        order.Status = EOrderStatus.Refunded;
        order.UpdatedAt = DateTime.Now;
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    
        return new Response<Order?>(order, 200, "Pedido estornado com sucesso.");
    }

    public async Task<Response<Order?>> GetByNumberAsync(GetOrderByNumberRequest request)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .Include(x => x.Voucher)
            .FirstOrDefaultAsync(x => x.Number == request.Number);

        return order is null 
            ? new Response<Order?>(null, 404, "Não encontrado") 
            : new Response<Order?>(order, 200, "Sucesso");
    }

    public async Task<PagedResponse<List<Order>?>> GetAllAsync(GetAllOrdersRequest request)
    {
        var query = context.Orders.AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .Include(x => x.Voucher)
            .OrderByDescending(x => x.CreatedAt);

        var count = await query.CountAsync();
        var orders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponse<List<Order>?>(orders, count, request.PageNumber, request.PageSize);
    }
}