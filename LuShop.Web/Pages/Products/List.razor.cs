using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Products;

public partial class ListProductPage : ComponentBase
{
    #region Properties

    public bool IsBusy { get; set; } = false;
    public List<Product> Products { get; set; } = [];
    public string SearchTerm { get; set; } = string.Empty;

    #endregion

    #region Services

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public IProductHandler Handler { get; set; } = null!;

    #endregion

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        IsBusy = true;
        try
        {
            var request = new GetAllProductsRequest();
            var result = await Handler.GetAllAsync(request);
            
            if (result.IsSuccess)
                Products = result.Data ?? [];
            else
                Snackbar.Add(result.Message!, Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Methods

    // Filtro para buscar na tabela pelo Título
    public Func<Product, bool> Filter => product =>
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return true;

        if (product.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    };

    #endregion
}