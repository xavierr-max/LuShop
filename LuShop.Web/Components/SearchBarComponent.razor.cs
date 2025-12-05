using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components; // Necessário para ComponentBase

namespace LuShop.Web.Components; // Verifique se a pasta é Components mesmo

// CORREÇÃO: Adicionado ': ComponentBase'
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

    #endregion
}