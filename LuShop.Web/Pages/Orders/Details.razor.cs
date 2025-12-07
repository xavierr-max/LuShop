using System.Security.Claims;
using LuShop.Core.Enums;
using LuShop.Core.Handlers;
using LuShop.Core.Requests.Orders; // Certifique-se de ter os Requests de Cancel/Refund aqui ou crie classes simples
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace LuShop.Web.Pages.Orders;

public class DetailsPage : ComponentBase
{
    #region Services
    [Inject] public IOrderHandler OrderHandler { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] public IDialogService DialogService { get; set; } = null!; // Injeção do Dialog
    #endregion

    #region Properties
    public List<LuShop.Core.Models.Order> Orders { get; private set; } = new();
    public bool IsBusy { get; private set; } = true;
    
    public string CurrentUserEmail { get; set; } = "Carregando...";
    public string CurrentUserId { get; set; } = "---";
    #endregion

    #region Overrides
    protected override async Task OnInitializedAsync()
    {
        IsBusy = true;
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                CurrentUserEmail = user.Identity.Name ?? "Sem Email";
                CurrentUserId = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
                                ?? user.FindFirst("sub")?.Value 
                                ?? "ID não encontrado";

                await LoadOrdersAsync();
            }
            else
            {
                Snackbar.Add("Você precisa estar logado.", Severity.Warning);
                NavigationManager.NavigateTo("/login");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Methods

    // --- CARREGAR PEDIDOS ---
    private async Task LoadOrdersAsync()
    {
        try
        {
            var request = new GetAllOrdersRequest { PageNumber = 1, PageSize = 25 };
            var result = await OrderHandler.GetAllAsync(request);

            if (result.IsSuccess)
                Orders = result.Data ?? new List<LuShop.Core.Models.Order>();
            else
                Snackbar.Add($"Erro API: {result.Message}", Severity.Error);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Snackbar.Add("Falha de conexão ao buscar pedidos.", Severity.Error);
        }
    }

    // --- AÇÃO: CANCELAR PEDIDO (Aguardando Pagamento) ---
    public async Task OnCancelOrderAsync(LuShop.Core.Models.Order order)
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Cancelar Pedido", 
            $"Tem certeza que deseja cancelar o pedido #{order.Number}?", 
            yesText: "Sim, Cancelar", cancelText: "Não");

        if (confirm == true)
        {
            IsBusy = true;
            try
            {
                var request = new CancelOrderRequest { Id = order.Id };
                var result = await OrderHandler.CancelAsync(request);
            
                if (result.IsSuccess)
                {
                    Snackbar.Add($"Pedido #{order.Number} cancelado com sucesso!", Severity.Success);
                    await LoadOrdersAsync(); 
                }
                else
                {
                    Snackbar.Add($"Erro: {result.Message}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao cancelar: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    // --- AÇÃO: ESTORNAR PEDIDO (Pago) ---
    public async Task OnRefundOrderAsync(LuShop.Core.Models.Order order)
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Solicitar Estorno", 
            $"Deseja solicitar o estorno do valor de {order.Total:C} referente ao pedido #{order.Number}?", 
            yesText: "Sim, Estornar", cancelText: "Voltar");

        if (confirm == true)
        {
            IsBusy = true;
            try
            {
                var request = new RefundOrderRequest { Id = order.Id };
                var result = await OrderHandler.RefundAsync(request);

                if (result.IsSuccess)
                {
                    Snackbar.Add($"Estorno do pedido #{order.Number} solicitado!", Severity.Info);
                    await LoadOrdersAsync();
                }
                else
                {
                    Snackbar.Add($"Erro: {result.Message}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao estornar: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public void GoToPayment(LuShop.Core.Models.Order order)
    {
        // Se estiver pago ou cancelado, talvez queira ir para detalhes em vez de pagamento
        if(order.Status == EOrderStatus.WaitingPayment)
            NavigationManager.NavigateTo($"/pedidos/{order.Number}/pagamento");
        else
            // Exemplo: criar uma página de detalhes completa ou apenas avisar
            Snackbar.Add($"Detalhes do pedido #{order.Number}", Severity.Normal);
    }
    
    public Color GetStatusColor(EOrderStatus status) => status switch
    {
        EOrderStatus.Paid => Color.Success,
        EOrderStatus.WaitingPayment => Color.Warning,
        EOrderStatus.Canceled => Color.Error,
        EOrderStatus.Refunded => Color.Info,
        _ => Color.Default
    };

    public string GetStatusText(EOrderStatus status) => status switch
    {
        EOrderStatus.Paid => "Pago",
        EOrderStatus.WaitingPayment => "Pendente",
        EOrderStatus.Canceled => "Cancelado",
        EOrderStatus.Refunded => "Estornado",
        _ => status.ToString()
    };
    #endregion
}