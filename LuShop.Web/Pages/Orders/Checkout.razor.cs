using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.OrderItems;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;
using MudBlazor;
// Adicionado para resolver CreateOrderItemRequest

namespace LuShop.Web.Pages.Orders;

public class CheckoutPage : ComponentBase
{
    #region Parameters

    [Parameter] public string ProductSlug { get; set; } = string.Empty;

    [SupplyParameterFromQuery(Name = "voucher")]
    public string? VoucherCode { get; set; }

    #endregion

    #region Properties

    // REMOVIDO: A propriedade 'Mask' foi deletada daqui.

    public bool IsBusy { get; set; } = false;
    public bool IsValid { get; set; } = false;
    
    public Product? Product { get; set; }
    public Voucher? Voucher { get; set; } 
    public decimal Total { get; set; }

    #endregion

    #region Services

    [Inject] public IProductHandler ProductHandler { get; set; } = null!;
    [Inject] public IOrderHandler OrderHandler { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await ProductHandler.GetBySlugAsync(new GetProductBySlugRequest { Slug = ProductSlug });
            
            if (result.IsSuccess == false || result.Data is null)
            {
                Snackbar.Add("Não foi possível obter o produto", Severity.Error);
                IsValid = false;
                return;
            }

            Product = result.Data;

            if (!string.IsNullOrEmpty(VoucherCode))
            {
                Snackbar.Add("Sistema de voucher temporariamente indisponível", Severity.Info);
            }

            IsValid = true;
            CalculateTotal();
        }
        catch
        {
            Snackbar.Add("Erro ao inicializar checkout", Severity.Error);
            IsValid = false;
        }
    }

    private void CalculateTotal()
    {
        if (Product is null) return;
        Total = Product.Price - (Voucher?.Amount ?? 0);
        if (Total < 0) Total = 0;
    }

    public async Task OnValidSubmitAsync()
    {
        IsBusy = true;

        try
        {
            // CORREÇÃO: CreateOrderRequest espera uma lista de Itens, não ProductId direto.
            var request = new CreateOrderRequest
            {
                VoucherId = Voucher?.Id,
                Items = new List<CreateOrderItemRequest>
                {
                    new()
                    {
                        ProductId = Product!.Id,
                        Quantity = 1
                    }
                }
            };

            var result = await OrderHandler.CreateAsync(request);
            
            if (result.IsSuccess && result.Data != null)
            {
                NavigationManager.NavigateTo($"/pedidos/{result.Data.Number}/pagamento");
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao criar pedido", Severity.Error);
            }
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

    private static char AllUpperCase(char c) => c.ToString().ToUpperInvariant()[0];

    #endregion
}