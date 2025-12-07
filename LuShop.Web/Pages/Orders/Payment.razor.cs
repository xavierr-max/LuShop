using System.Security.Claims;
using LuShop.Core.Handlers;
using LuShop.Core.Requests.Orders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;

namespace LuShop.Web.Pages.Orders;

public class PaymentPage : ComponentBase
{
    #region Parameters
    
    [Parameter] 
    public string Number { get; set; } = string.Empty;

    [SupplyParameterFromQuery(Name = "success")]
    public bool? Success { get; set; }

    #endregion

    #region Properties
    
    protected LuShop.Core.Models.Order? Order { get; private set; }
    protected bool IsBusy { get; private set; }
    
    protected bool PaymentSuccess { get; set; } 

    #endregion

    #region Services
    
    [Inject] public IOrderHandler OrderHandler { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public IConfiguration Configuration { get; set; } = null!; 
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    
    #endregion

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        // Se o Stripe retornou com sucesso (?success=true)
        if (Success == true)
        {
            PaymentSuccess = true;
            
            // NOVO: Chama o backend para efetivar a mudança de status para "Paid"
            await ConfirmPaymentAsync();
        }

        // Carrega o pedido para exibir os dados na tela
        await LoadOrderAsync();
    }

    #endregion

    #region Methods

    // NOVO MÉTODO: Envia o sinal de confirmação para o Backend
    private async Task ConfirmPaymentAsync()
    {
        try
        {
            var request = new PayOrderRequest
            {
                OrderNumber = Number,
                ExternalReference = "confirmed" // A palavra-chave que configuramos no Handler
            };

            // Chama o PayAsync. Como passamos "confirmed", ele vai atualizar o status em vez de criar sessão.
            await OrderHandler.PayAsync(request);
        }
        catch (Exception ex)
        {
            // Apenas logamos ou mostramos erro discreto, pois a tela de sucesso já está visível
            Console.WriteLine($"Erro ao confirmar status: {ex.Message}");
            Snackbar.Add("Erro ao sincronizar status do pagamento.", Severity.Warning);
        }
    }

    private async Task LoadOrderAsync()
    {
        IsBusy = true;
        try
        {
            var request = new GetOrderByNumberRequest { Number = Number };
            var result = await OrderHandler.GetByNumberAsync(request);

            if (result.IsSuccess)
            {
                Order = result.Data;
            }
            else
            {
                Snackbar.Add("Pedido não encontrado.", Severity.Error);
                NavigationManager.NavigateTo("/");
            }
        }
        catch
        {
            Snackbar.Add("Erro ao carregar pedido.", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task PayOrderAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            var userEmail = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value 
                            ?? user.FindFirst(c => c.Type == "email")?.Value 
                            ?? string.Empty;

            var request = new PayOrderRequest
            {
                OrderNumber = Number,
                Email = userEmail
            };

            var result = await OrderHandler.PayAsync(request);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data?.ExternalReference)) 
            {
                var sessionId = result.Data.ExternalReference;
                var publicKey = Configuration["Stripe:PublicKey"];
                await Js.InvokeVoidAsync("checkout", sessionId, publicKey);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro no pagamento", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion
}