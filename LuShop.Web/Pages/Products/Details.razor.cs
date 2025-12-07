using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Products;

public partial class DetailsPage : ComponentBase
{
    [Inject]
    public IProductHandler ProductHandler { get; set; } = null!;

    [Inject]
    public ICartHandler CartHandler { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public string Slug { get; set; } = string.Empty;

    public Product? Product { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    
    public bool IsLoading { get; set; } = false;
    public bool IsAddingToCart { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(Slug) || Slug == "{Slug}")
        {
            IsLoading = false;
            Product = null;
            return;
        }

        IsLoading = true;
        Product = null;

        try
        {
            var request = new GetProductBySlugRequest { Slug = Slug };
            var response = await ProductHandler.GetBySlugAsync(request);

            if (response?.IsSuccess == true && response.Data != null)
            {
                Product = response.Data;
                ImageUrl = GetImageUrl(Product.ImageUrl);
            }
            else
            {
                Snackbar.Add(response?.Message ?? "Produto não encontrado", Severity.Warning);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Erro ao carregar produto", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public async Task OnAddToCartAsync()
    {
        if (IsAddingToCart || Product is null) return;

        IsAddingToCart = true;

        try
        {
            var request = new AddCartItemRequest
            {
                ProductId = Product.Id,
                Quantity = 1
            };

            var response = await CartHandler.AddItemAsync(request);

            if (response.IsSuccess)
            {
                Snackbar.Add("Produto adicionado ao carrinho!", Severity.Success);
            }
            else
            {
                Snackbar.Add(response.Message ?? "Não foi possível adicionar ao carrinho", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsAddingToCart = false;
        }
    }

    // ✅ Novo método: Redireciona para o Checkout passando o Slug do produto atual
    public void OnBuyNow()
    {
        if (Product is not null)
        {
            Navigation.NavigateTo($"/checkout/{Product.Slug}");
        }
    }

    public void GoBack() => Navigation.NavigateTo("/");

    private string GetImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return "https://placehold.co/800x600?text=Sem+Imagem";
        
        if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imageUrl;
        
        return $"{Configuration.BackendUrl}/{imageUrl.TrimStart('/')}";
    }
}