using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.CartItems;

public class UpdateCartItemRequest
{
    // Usamos o ID do CartItem (e não do produto) para ser mais específico
    [Required]
    public long CartItemId { get; set; }

    [Required(ErrorMessage = "A quantidade deve ser informada")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade inválida")]
    public int Quantity { get; set; }
    
    public string UserId { get; set; } = string.Empty;
}