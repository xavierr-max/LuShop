using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages;

public partial class Home : ComponentBase
{
    #region Injections
    [Inject] 
    public IProductHandler Handler { get; set; } = null!;

    [Inject] 
    public ICartHandler CartHandler { get; set; } = null!;

    [Inject] 
    public ISnackbar Snackbar { get; set; } = null!;
    #endregion

    #region Properties
    public bool IsBusy { get; set; } = true;
    public List<Product> Products { get; set; } = new();
    public int TotalPages { get; set; } = 1;

    public GetAllProductsRequest Request { get; set; } = new()
    {
        PageNumber = 1,
        PageSize = 8 
    };
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }
    #endregion

    #region Methods
    private async Task LoadProductsAsync()
    {
        IsBusy = true;
        try
        {
            var result = await Handler.GetAllAsync(Request);

            if (result.IsSuccess)
            {
                Products = result.Data ?? new List<Product>();
                
                var totalCount = result.TotalCount; 
                TotalPages = (int)Math.Ceiling((double)totalCount / Request.PageSize);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao carregar produtos", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Falha de conexão com o servidor.", Severity.Error);
            Console.WriteLine($"Erro ao carregar produtos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        Request.PageNumber = page;
        await LoadProductsAsync();
    }

    public async Task AddToCartAsync(Product product)
    {
        try
        {
            var request = new AddCartItemRequest
            {
                ProductId = product.Id,
                Quantity = 1,
                UserId = ""
            };

            var result = await CartHandler.AddItemAsync(request);

            if (result.IsSuccess)
            {
                Snackbar.Add($"{product.Title} adicionado ao carrinho!", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Não foi possível adicionar o item", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Erro ao comunicar com o carrinho.", Severity.Error);
            Console.WriteLine($"Erro ao adicionar ao carrinho: {ex.Message}");
        }
    }

    public string GetImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return "/images/no-image.png";
        
        if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return imageUrl;
        
        imageUrl = imageUrl.TrimStart('/');
        
        return $"{Configuration.BackendUrl}/{imageUrl}";
    }
    #endregion
}