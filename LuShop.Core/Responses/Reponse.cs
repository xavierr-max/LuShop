using System.Text.Json.Serialization;

namespace LuShop.Core.Responses;

public class Response<TData> 
{
    public TData? Data { get; set; }
    public string? Message { get; set; }
    private readonly int _code;

    [JsonConstructor]
    public Response() 
        => _code = Configuration.DefaultStatusCode;
    
    public Response(TData? data, int code = Configuration.DefaultStatusCode, string? message = null)
    {
        Data = data;
        _code = code;
        Message = message;
    }
    
    [JsonIgnore]
    public bool IsSuccess 
        => _code is >= 200 and <= 299;
}