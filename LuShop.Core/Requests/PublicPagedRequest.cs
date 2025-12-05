namespace LuShop.Core.Requests;

public class PublicPagedRequest
{
    //número da página atual
    public int PageNumber { get; set; } = Configuration.DefaultPageNumber;
    //número de elementos na página atual
    public int PageSize { get; set; } = Configuration.DefaultPageSize;
}