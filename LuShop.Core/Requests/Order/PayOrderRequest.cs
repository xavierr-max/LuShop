namespace LuShop.Core.Requests.Order;

public class PayOrderRequest : Request
{
    public string OrderNumber { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
}