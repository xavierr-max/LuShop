namespace LuShop.Core.Requests.Stripe;

public class GetTransactionByOrderNumberRequest : Request
{
    public string OrderNumber { get; set; } = string.Empty;
}