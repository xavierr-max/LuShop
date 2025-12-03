namespace LuShop.Core.Models;

public class OrderItem
{
    public long Id { get; set; }

    // Relacionamento com Order
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Relacionamento com Product
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Dados abertos para leitura e escrita
    public int Quantity { get; set; }
    public decimal Price { get; set; } // Preço unitário
}