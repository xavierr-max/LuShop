using Microsoft.AspNetCore.Components;

namespace LuShop.Web.Pages.Orders.Admin;

public class RequestsPage : ComponentBase
{
    #region Services

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Properties

    protected string SearchOrderNumber = string.Empty;

    #endregion

    #region Methods

    protected void SearchTransactions()
    {
        if (!string.IsNullOrWhiteSpace(SearchOrderNumber))
        {
            NavigationManager.NavigateTo($"/pedidos/{SearchOrderNumber.Trim()}/transacoes");
        }
    }

    #endregion
}