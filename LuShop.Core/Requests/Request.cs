using System.Text.Json.Serialization;

namespace LuShop.Core.Requests;

public abstract class Request
{
    public string UserId { get; set; } = string.Empty;
}