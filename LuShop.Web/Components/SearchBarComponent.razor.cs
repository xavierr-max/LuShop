using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;

namespace LuShop.Web.Components;

public partial class SearchBarComponent : ComponentBase
{
    #region Services

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    public IProductHandler Handler { get; set; } = null!;

    #endregion

    #region Methods

    protected async Task<IEnumerable<Product>> SearchAsync(string? value, CancellationToken token)
    {
        if (token.IsCancellationRequested || string.IsNullOrEmpty(value))
            return new List<Product>();

        try
        {
            var request = new GetAllProductsRequest();
            var result = await Handler.GetAllAsync(request);

            if (result.IsSuccess && result.Data is not null)
            {
                return result.Data.Where(x => x.Title.Contains(value, StringComparison.OrdinalIgnoreCase));
            }
        }
        catch
        {
            return new List<Product>();
        }

        return new List<Product>();
    }

    protected void OnProductSelected(Product product)
    {
        if (product is not null && !string.IsNullOrEmpty(product.Slug))
        {
            NavigationManager.NavigateTo($"/produto/{product.Slug}");
        }
    }

    // ✅ NOVO MÉTODO: Formata a URL para apontar para o Backend
    private string GetImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return "https://placehold.co/100x100?text=No+Img"; // Placeholder pequeno para search
        
        if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imageUrl;
        
        // Garante que a URL aponte para a API (Backend)
        return $"{Configuration.BackendUrl}/{imageUrl.TrimStart('/')}";
    }

    #endregion
}