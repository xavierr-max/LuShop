namespace LuShop.Core.Requests.Order;

public class GetOrderByNumberRequest : Request
{
    public string Number { get; set; } = string.Empty;
}