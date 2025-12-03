using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.OrderItem;

// Classe auxiliar (DTO) apenas para o input
public class CreateOrderItemRequest
{
    [Required] public long ProductId { get; set; }

    [Required]
    [Range(1, 999, ErrorMessage = "A quantidade deve ser maior que zero")]
    public int Quantity { get; set; }
}