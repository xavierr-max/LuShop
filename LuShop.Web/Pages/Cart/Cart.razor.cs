using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Carts;
using LuShop.Core.Requests.OrderItems;
using LuShop.Core.Requests.Orders;
using LuShop.Core.Requests.Vouchers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Cart;

public partial class CartPage : ComponentBase
{
    #region Injections
    [Inject] public ICartHandler CartHandler { get; set; } = null!;
    [Inject] public IVoucherHandler VoucherHandler { get; set; } = null!;
    [Inject] public IOrderHandler OrderHandler { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public NavigationManager Navigation { get; set; } = null!;
    [Inject] public IDialogService DialogService { get; set; } = null!;
    [Inject] public IConfiguration Configuration { get; set; } = null!;
    #endregion

    #region Properties
    public Core.Models.Cart? CartModel { get; set; }
    public bool IsBusy { get; set; } = true; 
    public bool IsProcessingCheckout { get; set; } = false; 
    public string VoucherCode { get; set; } = string.Empty;
    #endregion

    #region Overrides
    protected override async Task OnInitializedAsync()
    {
        await LoadCartAsync();
    }
    #endregion

    #region Cart Loading & Management
    private async Task LoadCartAsync()
    {
        IsBusy = true;
        try
        {
            var request = new GetCartRequest();
            var result = await CartHandler.GetByUserAsync(request);

            if (result.IsSuccess)
            {
                CartModel = result.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar carrinho: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        await UpdateCartItemAsync(item);
    }

    public async Task DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            await UpdateCartItemAsync(item);
        }
    }

    private async Task UpdateCartItemAsync(CartItem item)
    {
        try
        {
            var request = new UpdateCartItemRequest
            {
                CartItemId = item.Id,
                Quantity = item.Quantity
            };

            var result = await CartHandler.UpdateItemAsync(request);

            if (result.IsSuccess)
            {
                CartModel = result.Data;
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao atualizar item", Severity.Error);
                await LoadCartAsync();
            }
        }
        catch
        {
            Snackbar.Add("Erro ao atualizar quantidade", Severity.Error);
            await LoadCartAsync();
        }
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        try
        {
            var request = new RemoveCartItemRequest { CartItemId = item.Id };
            var result = await CartHandler.RemoveItemAsync(request);

            if (result.IsSuccess)
            {
                CartModel = result.Data;
                Snackbar.Add("Item removido.", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao remover item", Severity.Error);
            }
        }
        catch
        {
            Snackbar.Add("Erro ao remover item", Severity.Error);
        }
    }

    public async Task ClearCartAsync()
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Limpar Carrinho", 
            "Tem certeza que deseja remover todos os itens?", 
            yesText: "Sim", 
            cancelText: "Cancelar");
            
        if (confirm == true)
        {
            try
            {
                var result = await CartHandler.ClearAsync(new ClearCartRequest());
                if (result.IsSuccess)
                {
                    CartModel = result.Data;
                    Snackbar.Add("Carrinho limpo.", Severity.Success);
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao limpar carrinho", Severity.Error);
                }
            }
            catch
            {
                Snackbar.Add("Erro ao limpar carrinho", Severity.Error);
            }
        }
    }
    #endregion

    #region Voucher & Totals
    public async Task ApplyVoucherAsync()
    {
        if (string.IsNullOrWhiteSpace(VoucherCode))
        {
            Snackbar.Add("Digite um código de cupom", Severity.Warning);
            return;
        }

        try
        {
            var request = new GetVoucherByNumberRequest { Number = VoucherCode };
            var result = await VoucherHandler.GetByNumberAsync(request);

            if (result.IsSuccess && result.Data is not null)
            {
                var voucher = result.Data;

                // ✅ Verifica se o cupom está ativo
                if (!voucher.IsActive)
                {
                    Snackbar.Add("Este cupom já foi utilizado.", Severity.Warning);
                    return;
                }

                if (CartModel is not null)
                {
                    CartModel.Voucher = voucher;
                    CartModel.VoucherId = voucher.Id;
                    Snackbar.Add($"Cupom {voucher.Title} aplicado!", Severity.Success);
                    VoucherCode = string.Empty;
                }
            }
            else
            {
                Snackbar.Add("Cupom inválido ou não encontrado.", Severity.Warning);
            }
        }
        catch
        {
            Snackbar.Add("Erro ao aplicar cupom", Severity.Error);
        }
    }

    public decimal CalculateSubtotal()
    {
        if (CartModel is null || !CartModel.Items.Any()) 
            return 0;
        
        return CartModel.Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);
    }

    public decimal CalculateTotal()
    {
        var subtotal = CalculateSubtotal();
        var discount = CartModel?.Voucher?.Amount ?? 0;
        return Math.Max(0, subtotal - discount);
    }
    #endregion

    #region Checkout Logic
    public async Task CheckoutAsync()
    {
        if (CartModel is null || !CartModel.Items.Any())
        {
            Snackbar.Add("Seu carrinho está vazio", Severity.Warning);
            return;
        }

        if (IsProcessingCheckout) return;
        IsProcessingCheckout = true;

        try
        {
            // ✅ Se houver voucher, desativar antes de criar o pedido
            if (CartModel.VoucherId.HasValue)
            {
                var updateRequest = new UpdateVoucherRequest
                {
                    Id = CartModel.VoucherId.Value,
                    Title = CartModel.Voucher!.Title,
                    Amount = CartModel.Voucher.Amount,
                    IsActive = false // ← Desativa o cupom
                };

                await VoucherHandler.UpdateAsync(updateRequest);
            }

            // Criar o pedido
            var request = new CreateOrderRequest
            {
                VoucherId = CartModel.VoucherId,
                Items = CartModel.Items.Select(item => new CreateOrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            var result = await OrderHandler.CreateAsync(request);

            if (result.IsSuccess && result.Data is not null)
            {
                try 
                {
                    await CartHandler.ClearAsync(new ClearCartRequest());
                }
                catch { }

                Navigation.NavigateTo($"/pedidos/{result.Data.Number}/pagamento");
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao criar pedido", Severity.Error);
                IsProcessingCheckout = false;
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro ao processar: {ex.Message}", Severity.Error);
            IsProcessingCheckout = false;
        }
    }
    #endregion

    #region Helpers
    public string GetImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return "/images/no-image.png";
        
        if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return imageUrl;
        
        var backendUrl = Configuration["BackendUrl"] ?? "";
        return $"{backendUrl.TrimEnd('/')}/{imageUrl.TrimStart('/')}";
    }
    #endregion
}