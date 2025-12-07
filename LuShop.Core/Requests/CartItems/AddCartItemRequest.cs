using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.CartItems;

public class AddCartItemRequest
{
    [Required(ErrorMessage = "O produto é inválido")]
    public long ProductId { get; set; }

    [Required(ErrorMessage = "A quantidade deve ser informada")]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1")]
    public int Quantity { get; set; }

    // O UserId será preenchido automaticamente pela API (Controller)
    public string UserId { get; set; } = string.Empty;
}