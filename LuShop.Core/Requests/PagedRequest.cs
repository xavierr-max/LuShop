namespace LuShop.Core.Requests;

public abstract class PagedRequest : Request
{
    //número da página atual
    public int PageNumber { get; set; } = Configuration.DefaultPageNumber;
    //número de elementos na página atual
    public int PageSize { get; set; } = Configuration.DefaultPageSize;
}