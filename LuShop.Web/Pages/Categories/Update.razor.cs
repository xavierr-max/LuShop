using LuShop.Core.Handlers;
using LuShop.Core.Requests.Categories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Categories
{
    public partial class UpdateCategoryBase : ComponentBase
    {
        // 💡 PARÂMETRO DE ROTA: Usando long Id em vez de string Slug
        [Parameter]
        public long Id { get; set; }


        // 💡 INJEÇÕES DE DEPENDÊNCIA
        [Inject]
        public ICategoryHandler CategoryHandler { get; set; } = null!; // Handler de Categoria

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;


        // 💡 ESTADO DA PÁGINA
        protected readonly UpdateCategoryRequest _request = new(); // Request de Categoria
        protected MudForm _form = null!; 
        protected bool _success;
        protected bool _isBusy;
        protected bool _isCategoryLoading = true; 


        // 💡 MÉTODOS DE CICLO DE VIDA
        
        protected override async Task OnParametersSetAsync()
        {
            // O ID é populado antes de OnParametersSetAsync, então podemos chamar o método
            await GetCategoryAsync();
        }

        // 💡 LÓGICA DE CARREGAMENTO

        private async Task GetCategoryAsync()
        {
            if (Id <= 0)
            {
                Snackbar.Add("ID da categoria inválido.", Severity.Error);
                return;
            }

            try
            {
                _isCategoryLoading = true;
                
                // Usa o novo Request de GetById
                var request = new GetByIdCategoryRequest { Id = Id };
                var response = await CategoryHandler.GetByIdAsync(request); 

                if (response.IsSuccess && response.Data is not null)
                {
                    var category = response.Data;
                    
                    // Mapeia os dados do modelo para o Request
                    _request.Id = category.Id;
                    _request.Title = category.Title;
                    _request.Description = category.Description;
                    
                    Snackbar.Add("Categoria carregada com sucesso!", Severity.Info);
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao carregar categoria.", Severity.Error);
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

        // 💡 LÓGICA DE ATUALIZAÇÃO

        protected async Task OnSubmitAsync()
        {
            if (_isBusy || _isCategoryLoading) return;
            
            await _form.Validate();
            if (!_form.IsValid) return;

            try
            {
                _isBusy = true;
                
                var response = await CategoryHandler.UpdateAsync(_request); // Chamada ao handler

                if (response.IsSuccess)
                {
                    Snackbar.Add(response.Message ?? "Categoria atualizada!", Severity.Success);
                    
                    // Retorna para a lista
                    Navigation.NavigateTo("/categorias");
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao atualizar categoria.", Severity.Error);
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