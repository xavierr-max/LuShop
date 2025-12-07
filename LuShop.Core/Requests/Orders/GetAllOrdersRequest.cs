using System.Text.Json.Serialization;

namespace LuShop.Core.Requests.Orders;

public class GetAllOrdersRequest : PublicPagedRequest
{
    // O UserId não deve vir da query string, será preenchido pelo endpoint
    [JsonIgnore] // Ignora na serialização
    public string UserId { get; set; } = string.Empty;
}