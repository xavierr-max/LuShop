using LuShop.Core.Handlers;
using LuShop.Core.Requests.Orders;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Orders;

public partial class ConfirmPage : ComponentBase
{
    [Parameter] public string OrderNumber { get; set; } = string.Empty;

    [Inject] public IOrderHandler OrderHandler { get; set; } = null!;
    [Inject] public NavigationManager Navigation { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    public bool IsLoading { get; set; } = true;
    public bool IsSuccess { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        // Ao carregar a página de sucesso, confirmamos o pagamento no banco
        await ConfirmPaymentAsync();
    }

    private async Task ConfirmPaymentAsync()
    {
        try
        {
            var request = new PayOrderRequest
            {
                OrderNumber = OrderNumber,
                ExternalReference = "stripe_checkout_confirmed"
            };

            var result = await OrderHandler.PayAsync(request);

            if (result.IsSuccess)
            {
                IsSuccess = true;
                Snackbar.Add("Pagamento confirmado com sucesso!", Severity.Success);
            }
            else
            {
                IsSuccess = false;
                Snackbar.Add("Erro ao confirmar status do pagamento.", Severity.Error);
            }
        }
        catch (Exception)
        {
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}