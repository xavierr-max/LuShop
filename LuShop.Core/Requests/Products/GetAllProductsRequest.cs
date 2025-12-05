using LuShop.Core.Responses;

namespace LuShop.Core.Requests.Products;

public class GetAllProductsRequest : PublicPagedRequest
{
    public string? Title { get; set; }
}