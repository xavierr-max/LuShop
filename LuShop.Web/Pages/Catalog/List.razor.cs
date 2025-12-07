using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Categories;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Catalog;

public partial class CatalogPage : ComponentBase
{
    [Inject] protected IProductHandler ProductHandler { get; set; } = null!;
    [Inject] protected ICategoryHandler CategoryHandler { get; set; } = null!;
    [Inject] protected ICartHandler CartHandler { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected bool IsBusy { get; set; } = true;
    protected List<Product> Products { get; set; } = new();

    // Nome mudado para evitar conflito de tipo
    private Dictionary<long, LuShop.Core.Models.Category> CategoriesById { get; set; } = new();

    private GetAllProductsRequest Request { get; set; } = new()
    {
        PageNumber = 1,
        PageSize = 500
    };

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadCategoriesAsync(), LoadProductsAsync());
        await ApplyCategoryFilterIfNeeded();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await CategoryHandler.GetAllAsync(new GetAllCategoriesRequest());
            if (result.IsSuccess && result.Data != null)
            {
                CategoriesById = result.Data.ToDictionary(c => c.Id, c => c);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CatalogPage] Erro ao carregar categorias: {ex}");
        }
    }

    private async Task LoadProductsAsync()
    {
        IsBusy = true;
        StateHasChanged();

        try
        {
            var result = await ProductHandler.GetAllAsync(Request);

            Products = result is { IsSuccess: true, Data: not null }
                ? result.Data.Where(p => p.IsActive).ToList()
                : new List<Product>();
        }
        catch (Exception ex)
        {
            Snackbar.Add("Erro ao carregar produtos.", Severity.Error);
            Console.WriteLine($"[CatalogPage] Erro ao carregar produtos: {ex}");
            Products = new List<Product>();
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }

    private async Task ApplyCategoryFilterIfNeeded()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var categoriaParam = query["categoria"]?.ToLower().Trim();

        if (string.IsNullOrWhiteSpace(categoriaParam))
            return;

        var targetProduct = Products.FirstOrDefault(p =>
        {
            var category = GetCategoryForProduct(p);
            if (category == null) return false;

            var slug = category.Title.ToLower()
                .Replace(" ", "-")
                .Replace("&", "e")
                .Replace("/", "-")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("+", "");

            return slug == categoriaParam;
        });

        if (targetProduct != null)
        {
            Request.CategoryId = targetProduct.CategoryId;
            await LoadProductsAsync();
        }
    }

    // Agora retorna com tipo completo para evitar conflito
    protected LuShop.Core.Models.Category? GetCategoryForProduct(Product product)
    {
        return product.Category
            ?? CategoriesById.GetValueOrDefault(product.CategoryId)
            ?? new LuShop.Core.Models.Category { Id = 0, Title = "Sem Categoria" };
    }

    protected async Task AddToCartAsync(Product product)
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
                Snackbar.Add($"{product.Title} adicionado ao carrinho!", Severity.Success);
            else
                Snackbar.Add(result.Message ?? "Falha ao adicionar", Severity.Warning);
        }
        catch (Exception ex)
        {
            Snackbar.Add("Erro de conexão.", Severity.Error);
            Console.WriteLine(ex);
        }
    }

    protected string GetImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return "/images/no-image.png";

        return imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? imageUrl
            : $"{Configuration.BackendUrl}/{imageUrl.TrimStart('/')}";
    }
}