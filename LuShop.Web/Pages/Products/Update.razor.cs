using LuShop.Core.Handlers;
using LuShop.Core.Requests.Products;
using LuShop.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace LuShop.Web.Pages.Products
{
    public partial class UpdatePage : ComponentBase
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


        // 💡 ESTADO DA PÁGINA (MODIFICADOS PARA PROTECTED)
        // O Razor agora pode acessar estes campos herdados.
        protected readonly UpdateProductRequest _request = new();
        protected MudForm _form = null!; 
        protected bool _success;
        protected bool _isBusy;
        protected bool _isProductLoading = true; 
        protected string? _imageBase64Preview;


        // 💡 MÉTODOS DE CICLO DE VIDA
        
        protected override async Task OnParametersSetAsync()
        {
            await GetProductAsync();
        }

        // 💡 LÓGICA DE ATUALIZAÇÃO

        // Mantido como private, pois é chamado apenas internamente pelo OnParametersSetAsync
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
                
                // Correção de construtor
                var request = new GetProductBySlugRequest { Slug = Slug };
                var response = await ProductHandler.GetBySlugAsync(request); 

                if (response.IsSuccess && response.Data is not null)
                {
                    var product = response.Data;
                    
                    _request.Id = product.Id;
                    _request.Title = product.Title;
                    _request.Description = product.Description;
                    _request.Price = product.Price;
                    _request.CategoryId = product.CategoryId;
                    _request.IsActive = product.IsActive;
                    
                    // Usa ImageUrl do Product (e deixa Base64Image do Request vazio/null por padrão)
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        _imageBase64Preview = product.ImageUrl;
                    }
                    
                    Snackbar.Add("Produto carregado com sucesso!", Severity.Info);
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao carregar produto.", Severity.Error);
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

        protected async Task UploadImage(IBrowserFile? file)
        {
            if (file is null) return;

            long maxFileSize = 1024 * 1024 * 5; 
            
            try 
            {
                var format = "image/jpeg";
                var resizedImage = await file.RequestImageFileAsync(format, 600, 400);

                var buffer = new byte[resizedImage.Size];
                
                await resizedImage.OpenReadStream(maxFileSize).ReadExactlyAsync(buffer);
                
                var base64Data = Convert.ToBase64String(buffer);
                _imageBase64Preview = $"data:{format};base64,{base64Data}";
                
                // Atribui Base64String ao request para envio ao backend
                _request.Base64Image = _imageBase64Preview;
                
                Snackbar.Add("Imagem carregada com sucesso!", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar imagem: {ex.Message}", Severity.Error);
            }
        }

        protected async Task OnSubmitAsync()
        {
            if (_isBusy || _isProductLoading) return;
            
            await _form.Validate();
            if (!_form.IsValid) return;

            try
            {
                _isBusy = true;
                
                var response = await ProductHandler.UpdateAsync(_request);

                if (response.IsSuccess)
                {
                    Snackbar.Add(response.Message ?? "Produto atualizado!", Severity.Success);
                    
                    var productSlug = response.Data?.Slug ?? Slug;
                    Navigation.NavigateTo($"/produto/{productSlug}");
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao atualizar produto.", Severity.Error);
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