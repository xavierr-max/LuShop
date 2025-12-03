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
    // o cliente vai passar os produtos que ele deseja e o handler vai buscar no banco para criar um pedido
    public async Task<Response<Order?>> CreateAsync(CreateOrderRequest request)
    {
        Voucher? voucher = null;
        if (request.VoucherId.HasValue)
        {
            voucher = await context.Vouchers
                .FirstOrDefaultAsync(x => x.Id == request.VoucherId.Value && x.IsActive);

            // Se o usuário mandou um ID, mas não achamos (ou está inativo), é erro
            if (voucher is null)
                return new Response<Order?>(null, 400, "Voucher inválido ou inativo.");
        }

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
                ProductId = product.Id,
                Product = product,
                Price = product.Price, // Você copia o preço manualmente aqui
                Quantity = itemRequest.Quantity
            };
            //O EF consegue preencher o Order e OrderId com base nessas relações automaticamente 

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
        catch
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
        try
        {
            // 1. Busca o pedido pelo ID (Primary Key)
            // O FindAsync é otimizado para buscar pela chave primária
            var order = await context.Orders.FindAsync(request.Id);

            if (order is null)
                return new Response<Order?>(null, 404, "Pedido não encontrado.");

            // 2. Valida o status atual
            switch (order.Status)
            {
                // Se já foi cancelado, avisa o usuário (Idempotência)
                case EOrderStatus.Canceled:
                    return new Response<Order?>(order, 200, "Este pedido já foi cancelado anteriormente.");

                // Regra Crítica: Pedido pago não se cancela, se estorna (reembolsa)
                case EOrderStatus.Paid:
                    return new Response<Order?>(order, 400, "Não é possível cancelar um pedido já pago. Solicite o reembolso.");

                // Pedido já devolvido
                case EOrderStatus.Refunded:
                    return new Response<Order?>(order, 400, "Este pedido já foi reembolsado.");

                // Cenário Feliz: Aguardando pagamento -> Pode Cancelar
                case EOrderStatus.WaitingPayment:
                    break;

                default:
                    return new Response<Order?>(null, 400, "O status atual do pedido não permite cancelamento.");
            }

            // 3. Executa o cancelamento
            order.Status = EOrderStatus.Canceled;
            order.UpdatedAt = DateTime.Now;

            // 4. Salva no banco
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new Response<Order?>(order, 200, "Pedido cancelado com sucesso.");
        }
        catch
        {
            return new Response<Order?>(null, 500, "Falha ao cancelar o pedido.");
        }
    }

    public async Task<Response<Order?>> RefundAsync(RefundOrderRequest request)
    {
        try
        {
            // 1. Busca o pedido pelo ID
            var order = await context.Orders.FindAsync(request.Id);

            if (order is null)
                return new Response<Order?>(null, 404, "Pedido não encontrado.");

            // 2. Valida o status
            switch (order.Status)
            {
                // O único caso válido para reembolso é se ele JÁ FOI PAGO
                case EOrderStatus.Paid:
                    break; // Sai do switch e executa o reembolso

                // Se já foi reembolsado, avisa (Idempotência)
                case EOrderStatus.Refunded:
                    return new Response<Order?>(order, 400, "Este pedido já foi reembolsado anteriormente.");

                // Se não foi pago, não tem como devolver dinheiro
                case EOrderStatus.WaitingPayment:
                    return new Response<Order?>(order, 400, "Não é possível reembolsar um pedido que ainda não foi pago.");

                case EOrderStatus.Canceled:
                    return new Response<Order?>(order, 400, "Este pedido está cancelado e não foi pago, impossível reembolsar.");

                default:
                    return new Response<Order?>(null, 400, "Status inválido para reembolso.");
            }

            // 3. Lógica de Gateway (Stripe/PayPal)
            // No mundo real, aqui você chamaria a API do gateway para devolver o dinheiro.
            // ex: await _stripeService.RefundAsync(order.ExternalReference);

            // 4. Atualiza o status local
            order.Status = EOrderStatus.Refunded;
            order.UpdatedAt = DateTime.Now;

            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new Response<Order?>(order, 200, "Pedido reembolsado com sucesso!");
        }
        catch
        {
            return new Response<Order?>(null, 500, "Falha ao processar o reembolso.");
        }
    }

    public async Task<Response<Order?>> GetByNumberAsync(GetOrderByNumberRequest request)
    {
        try
        {
            // 1. Monta a consulta
            var order = await context.Orders
                .AsNoTracking() // IMPORTANTE: Diz ao EF para não gastar memória rastreando mudanças (mais rápido)
                .Include(x => x.Items) // Traz a lista de itens junto
                .ThenInclude(x => x.Product) // Dentro de cada item, traz os dados do Produto (para saber o Title, etc)
                .Include(x => x.Voucher) // Traz os dados do Voucher
                .FirstOrDefaultAsync(x => x.Number == request.Number);

            // 2. Valida se encontrou
            if (order is null)
                return new Response<Order?>(null, 404, "Pedido não encontrado.");

            // 3. Retorna o objeto completo
            return new Response<Order?>(order, 200, "Pedido recuperado com sucesso.");
        }
        catch
        {
            return new Response<Order?>(null, 500, "Falha ao recuperar o pedido.");
        }
    }

    public async Task<PagedResponse<List<Order>?>> GetAllAsync(GetAllOrdersRequest request)
    {
        try
        {
            // 1. Configurar a Consulta Base
            // Criamos um objeto IQueryable. Isso ainda NÃO executa nada no banco.
            var query = context.Orders
                .AsNoTracking() // Otimização de leitura
                .Include(x => x.Items) // Importante trazer os itens para calcular o Total
                .ThenInclude(x => x.Product)
                .Include(x => x.Voucher)
                .OrderByDescending(x => x.CreatedAt); // Sempre ordene pedidos do mais novo para o mais antigo

            // 2. Contar o Total (Banco Hit #1)
            // Precisamos saber quantos existem no total antes de cortar a página
            var count = await query.CountAsync();

            // 3. Buscar os dados da Página (Banco Hit #2)
            // Skip: Pula X registros (ex: Pagina 2, Pula os primeiros 25)
            // Take: Pega Y registros (ex: Pega os próximos 25)
            var orders = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // 4. Retornar resposta paginada
            // O PagedResponse calcula o TotalPages internamente com base no 'count' e 'PageSize'
            return new PagedResponse<List<Order>?>(
                orders, 
                count, 
                request.PageNumber, 
                request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Order>?>(null, 500, "Não foi possível consultar os pedidos.");
        }
    }
}