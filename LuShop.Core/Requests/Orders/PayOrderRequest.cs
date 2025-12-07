namespace LuShop.Core.Requests.Orders;

public class PayOrderRequest : Request
{
    public string OrderNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ExternalReference { get; set; }
}