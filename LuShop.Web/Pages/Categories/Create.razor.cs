using LuShop.Core.Handlers;
using LuShop.Core.Requests.Categories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Categories
{
    public partial class AdicionarCategoryBase : ComponentBase
    {
        // 💡 INJEÇÕES DE DEPENDÊNCIA
        [Inject]
        public ICategoryHandler CategoryHandler { get; set; } = default!; // Handler de Categoria

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = default!;


        // 💡 ESTADO DA PÁGINA
        protected readonly CreateCategoryRequest _request = new(); // Request de Categoria
        protected MudForm _form = null!; 
        protected bool _success;
        protected bool _isBusy; 


        // 💡 MÉTODOS DE LÓGICA

        protected async Task OnSubmitAsync()
        {
            if (_isBusy) return;
            
            await _form.Validate();
            if (!_form.IsValid) return;

            try
            {
                _isBusy = true;
                var response = await CategoryHandler.CreateAsync(_request);

                if (response.IsSuccess)
                {
                    Snackbar.Add(response.Message ?? "Categoria criada!", Severity.Success);
                    
                    // Retorna para a lista após criar
                    Navigation.NavigateTo("/categorias");
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao criar categoria.", Severity.Error);
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