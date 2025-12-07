using LuShop.Core.Handlers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Categories
{
    public class DeleteCategoryPage : ComponentBase
    {
        [Parameter]
        public long Id { get; set; }

        [Inject]
        public ICategoryHandler CategoryHandler { get; set; } = null!;

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;


        // 💡 CORRIGIDO: CAMPOS MUDADOS PARA 'protected' para acesso no arquivo .razor
        // O tipo é qualificado pelo namespace completo.
        protected LuShop.Core.Models.Category? _category;
        protected bool _isBusy;
        protected bool _isCategoryLoading = true;


        protected override async Task OnParametersSetAsync()
        {
            await GetCategoryAsync();
        }

        private async Task GetCategoryAsync()
        {
            if (Id <= 0)
            {
                Snackbar.Add("ID da categoria não encontrado.", Severity.Error);
                return;
            }

            try
            {
                _isCategoryLoading = true;

                // 💡 CORRIGIDO: Tipo de Request qualificado
                var request = new LuShop.Core.Requests.Categories.GetByIdCategoryRequest { Id = Id };
                var response = await CategoryHandler.GetByIdAsync(request);

                if (response.IsSuccess && response.Data is not null)
                {
                    _category = response.Data;
                    Snackbar.Add("Categoria carregada para confirmação.", Severity.Info);
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Categoria não encontrada.", Severity.Error);
                    Navigation.NavigateTo("/categorias");
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro crítico ao carregar: {ex.Message}", Severity.Error);
                Navigation.NavigateTo("/categorias");
            }
            finally
            {
                _isCategoryLoading = false;
            }
        }

        public async Task OnDeleteAsync()
        {
            if (_isBusy || _isCategoryLoading || _category is null) return;

            try
            {
                _isBusy = true;

                // 💡 CORRIGIDO: Tipo de Request qualificado
                var request = new LuShop.Core.Requests.Categories.DeleteCategoryRequest { Id = _category.Id };

                var response = await CategoryHandler.DeleteAsync(request);

                if (response.IsSuccess)
                {
                    Snackbar.Add(response.Message ?? $"Categoria '{_category.Title}' excluída com sucesso!", Severity.Success);

                    Navigation.NavigateTo("/categorias");
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao excluir categoria.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro crítico: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}