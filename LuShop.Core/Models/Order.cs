using LuShop.Core.Enums;

namespace LuShop.Core.Models;

public class Order
{
    public long Id { get; set; }

    // Inicializa com valor padrão, mas permite alteração
    public string Number { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public string? ExternalReference { get; set; }
    public EPaymentGateway Gateway { get; set; } = EPaymentGateway.Stripe;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public EOrderStatus Status { get; set; } = EOrderStatus.WaitingPayment;

    public long? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    public string UserId { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = new();

    // O Total pode continuar sendo calculado automaticamente para facilitar
    // (Ou você pode criar um "public decimal Total { get; set; }" se quiser salvar fixo no banco)
    public decimal Total 
    { 
        get 
        {
            decimal totalItems = 0;
            // Verifica se a lista não é nula antes de somar
            if (Items != null)
            {
                totalItems = Items.Sum(x => x.Price * x.Quantity);
            }

            return totalItems - (Voucher?.Amount ?? 0);
        }
    }
}