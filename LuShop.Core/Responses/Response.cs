using System.Text.Json.Serialization;

namespace LuShop.Core.Responses;

public class Response<TData> 
{
    public TData? Data { get; set; }
    public string? Message { get; set; }
    
    // ✅ MUDANÇA: Tornar público para serializar
    public int Code { get; set; }

    [JsonConstructor]
    public Response() 
        => Code = Configuration.DefaultStatusCode;
    
    public Response(TData? data, int code = Configuration.DefaultStatusCode, string? message = null)
    {
        Data = data;
        Code = code;
        Message = message;
    }
    
    [JsonIgnore]
    public bool IsSuccess 
        => Code is >= 200 and <= 299;
}