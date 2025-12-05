using System.ComponentModel.DataAnnotations;
using LuShop.Core.Enums;
using LuShop.Core.Requests.OrderItems;

namespace LuShop.Core.Requests.Orders;

public class CreateOrderRequest : Request
{
    public long? VoucherId { get; set; }
    
    public EPaymentGateway Gateway { get; set; } = EPaymentGateway.Stripe;

    [Required(ErrorMessage = "O pedido deve conter itens")]
    [MinLength(1, ErrorMessage = "Adicione pelo menos um item ao pedido")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}


