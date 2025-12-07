namespace LuShop.Core.Requests.CartItems;

public class RemoveCartItemRequest
{
    public long CartItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
}