using System.Text.Json.Serialization;

namespace LuShop.Core.Responses;

//modelo de reposta paginada dada a uma requisicao
public class PagedResponse<TData> : Response<TData>
{
    //página atual
    public int CurrentPage { get; set; } 
    
    //total de páginas, sendo o total de querys(elementos) pela quantidade de querys por página
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); 
    
    //querys por página
    public int PageSize { get; set; } = Configuration.DefaultPageSize; 
    
    //total de querys
    public int TotalCount { get; set; } 

    [JsonConstructor]
    public PagedResponse(
        TData? data, //query
        int totalCount, //contagem de querys
        int currentPage = 1, //página atual
        int pageSize = Configuration.DefaultPageSize) //quantidade de querys por página (25)
        :base(data)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
    
    public PagedResponse(
        TData? data,
        int code = Configuration.DefaultStatusCode,
        string? message = null)
        : base(data, code, message)
    {
    }

    
}