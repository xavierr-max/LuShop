using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LuShop.Web.Pages.Products
{
    public partial class DeletePage : ComponentBase
    {
        // 💡 PARÂMETRO DE ROTA
        [Parameter]
        public string Slug { get; set; } = string.Empty;


        // 💡 INJEÇÕES DE DEPENDÊNCIA
        [Inject]
        public IProductHandler ProductHandler { get; set; } = null!;

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;


        // 💡 ESTADO DA PÁGINA
        protected Product? _product; // O modelo do produto para exibição e obtenção do ID
        protected bool _isBusy;
        protected bool _isProductLoading = true;


        // 💡 MÉTODOS DE CICLO DE VIDA
        
        protected override async Task OnParametersSetAsync()
        {
            await GetProductAsync();
        }

        // 💡 LÓGICA DE CARREGAMENTO

        private async Task GetProductAsync()
        {
            if (string.IsNullOrEmpty(Slug))
            {
                Snackbar.Add("Slug do produto não encontrado.", Severity.Error);
                return;
            }

            try
            {
                _isProductLoading = true;
                
                var request = new GetProductBySlugRequest { Slug = Slug };
                var response = await ProductHandler.GetBySlugAsync(request); 

                if (response.IsSuccess && response.Data is not null)
                {
                    _product = response.Data;
                    Snackbar.Add("Produto carregado para confirmação.", Severity.Info);
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Produto não encontrado.", Severity.Error);
                    Navigation.NavigateTo("/produtos"); 
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro crítico ao carregar: {ex.Message}", Severity.Error);
                Navigation.NavigateTo("/produtos"); 
            }
            finally
            {
                _isProductLoading = false;
            }
        }

        // 💡 LÓGICA DE EXCLUSÃO

        public async Task OnDeleteAsync()
        {
            if (_isBusy || _isProductLoading || _product is null) return;
            
            try
            {
                _isBusy = true;

                // ✅ CORRIGIDO: Instanciação usando inicializador de objeto
                var request = new DeleteProductRequest { Id = _product.Id };

                var response = await ProductHandler.DeleteAsync(request);

                if (response.IsSuccess)
                {
                    Snackbar.Add(response.Message ?? $"Produto '{_product.Title}' excluído com sucesso!", Severity.Success);
                    
                    Navigation.NavigateTo("/produtos");
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao excluir produto.", Severity.Error);
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