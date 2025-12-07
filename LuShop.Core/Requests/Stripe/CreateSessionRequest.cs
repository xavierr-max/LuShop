// LuShop.Core/Requests/Stripe/CreateSessionRequest.cs
namespace LuShop.Core.Requests.Stripe;

public class CreateSessionRequest : Request
{
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = "brl";
    
    public string CustomerEmail { get; set; } = string.Empty;

    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}