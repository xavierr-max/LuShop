using LuShop.Core.Handlers;
using LuShop.Core.Requests.Stripe;
using LuShop.Core.Responses.Stripe;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Orders.Admin;

public partial class TransactionsByOrderPage : ComponentBase
{
    #region Parameters

    [Parameter]
    public string OrderNumber { get; set; } = string.Empty;

    #endregion

    #region Services

    [Inject]
    public IStripeHandler StripeHandler { get; set; } = null!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Properties

    protected bool _loading = true;
    protected List<StripeTransactionResponse> _transactions = new();

    #endregion

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        await LoadTransactionsAsync();
    }

    #endregion

    #region Methods

    private async Task LoadTransactionsAsync()
    {
        try
        {
            _loading = true;

            // 👇 ADICIONE ESTE LOG
            Console.WriteLine($"[CLIENT] OrderNumber recebido: '{OrderNumber}'");

            var request = new GetTransactionByOrderNumberRequest 
            { 
                OrderNumber = OrderNumber 
            };

            // 👇 ADICIONE ESTE LOG
            Console.WriteLine($"[CLIENT] Request.OrderNumber: '{request.OrderNumber}'");

            var result = await StripeHandler.GetTransactionsByOrderNumberAsync(request);

            // 👇 ADICIONE ESTE LOG
            Console.WriteLine($"[CLIENT] Result.IsSuccess: {result.IsSuccess}");
            Console.WriteLine($"[CLIENT] Result.Message: {result.Message}");

            if (result.IsSuccess && result.Data != null)
            {
                _transactions = result.Data;
            
                if (!_transactions.Any())
                {
                    Snackbar.Add("Nenhuma transação encontrada para este pedido.", Severity.Info);
                }
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao buscar transações.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            // 👇 MODIFIQUE ESTE LOG
            Console.WriteLine($"[CLIENT] Exception: {ex}");
            Snackbar.Add($"Erro ao conectar com o servidor: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }
    }

    #endregion