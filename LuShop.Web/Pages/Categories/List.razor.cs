using LuShop.Core.Handlers;
using LuShop.Core.Models;                    // Para Product, Category, etc
using LuShop.Core.Requests.Categories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Categories;

public partial class ListCategoryPage : ComponentBase
{
    #region Properties

    protected bool IsBusy { get; set; } = true;

    // Usa o tipo completo para evitar conflito
    protected List<LuShop.Core.Models.Category> Categories { get; set; } = new();

    protected string SearchTerm { get; set; } = string.Empty;

    #endregion

    #region Services

    [Inject]
    protected ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    protected ICategoryHandler Handler { get; set; } = null!;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        IsBusy = true;
        StateHasChanged();

        try
        {
            var request = new GetAllCategoriesRequest();
            var result = await Handler.GetAllAsync(request);

            if (result.IsSuccess && result.Data != null)
            {
                Categories = result.Data;
            }
            else
            {
                Categories = new();
                Snackbar.Add(result.Message ?? "Erro ao carregar categorias.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Categories = new();
            Snackbar.Add("Erro ao conectar com o servidor.", Severity.Error);
            Console.WriteLine($"[ListCategoryPage] Erro: {ex}");
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Filter

    protected Func<LuShop.Core.Models.Category, bool> Filter => category =>
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return true;

        if (category.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrWhiteSpace(category.Description) &&
            category.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    };

    #endregion
}