namespace LuShop.Core.Models;

public class Cart
{
    public long Id { get; set; }
    
    // O carrinho pertence a um usuário (logado ou sessão anônima)
    public string UserId { get; set; } = string.Empty;

    // Relacionamento com Voucher (Opcional)
    // O usuário pode aplicar um cupom no carrinho antes de fechar o pedido
    public long? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    // Lista de itens
    public List<CartItem> Items { get; set; } = new();

    // Propriedade calculada do Total (soma itens - desconto)
    public decimal Total 
    { 
        get 
        {
            decimal totalItems = 0;
            if (Items != null)
            {
                // Soma o preço atual dos produtos * quantidade
                totalItems = Items.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);
            }

            // Aplica desconto do voucher, se existir
            // Nota: Cuide para o total não ficar negativo
            decimal discount = Voucher?.Amount ?? 0;
            decimal finalTotal = totalItems - discount;
            
            return finalTotal < 0 ? 0 : finalTotal;
        }
    }
    
    // Propriedade útil para exibir o subtotal sem desconto
    public decimal SubTotal => Items?.Sum(x => (x.Product?.Price ?? 0) * x.Quantity) ?? 0;
}