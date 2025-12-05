using System.Text.Json.Serialization; // Útil para evitar ciclos se usar API

namespace LuShop.Core.Models;

public class CartItem
{
    public long Id { get; set; }

    // Relacionamento com o Carrinho
    public long CartId { get; set; }
    [JsonIgnore] // Evita loop infinito na serialização
    public Cart Cart { get; set; } = null!;

    // Relacionamento com o Produto
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; } = 1;

    // Diferente do OrderItem, aqui geralmente não salvamos o Price fixo no banco,
    // pois o preço do carrinho deve refletir o preço ATUAL do produto na loja.
    // Mas podemos ter uma propriedade calculada para facilitar:
    public decimal TotalPrice => Product?.Price * Quantity ?? 0;
}