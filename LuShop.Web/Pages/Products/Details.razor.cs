using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Components;

namespace LuShop.Web.Pages.Products;

public partial class DetailsPage : ComponentBase
{
    [Inject]
    public IProductHandler ProductHandler { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public string Slug { get; set; } = string.Empty;

    public Product? Product { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsLoading { get; set; } = true;

    // ✅ MUDANÇA PRINCIPAL: Usamos OnParametersSetAsync em vez de OnInitializedAsync
    // Este método roda toda vez que o parâmetro 'Slug' muda na URL.
    protected override async Task OnParametersSetAsync()
    {
        // 1. Reseta o estado para mostrar o "loading" ao trocar de produto
        IsLoading = true;
        Product = null; 

        // Validação básica
        if (string.IsNullOrWhiteSpace(Slug))
        {
            IsLoading = false;
            return;
        }

        try
        {
            var request = new GetProductBySlugRequest { Slug = Slug };
            var response = await ProductHandler.GetBySlugAsync(request);

            if (response?.IsSuccess == true && response.Data != null)
            {
                Product = response.Data;

                // Lógica da Imagem (Mantida conforme seu envio)
                // Dica: Futuramente, tente pegar a URL base (localhost:5047) do appsettings.json
                ImageUrl = string.IsNullOrWhiteSpace(Product.ImageUrl)
                    ? "https://placehold.co/800x600?text=Sem+Imagem"
                    : $"http://localhost:5047/{Product.ImageUrl}"; 
            }
        }
        catch (Exception)
        {
            // Opcional: Adicionar log ou Snackbar de erro aqui
        }
        finally
        {
            // Garante que o loading suma, tendo sucesso ou erro
            IsLoading = false;
            StateHasChanged(); // Força a atualização da tela
        }
    }

    public void GoBack() => Navigation.NavigateTo("/");
}