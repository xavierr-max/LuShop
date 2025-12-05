// Adicionar.razor.cs

using LuShop.Core.Handlers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using LuShop.Core.Requests.Products;
using Microsoft.AspNetCore.Components.Forms;

// Mude "LuShop.Web.Pages.Products" para o namespace real das suas páginas.
namespace LuShop.Web.Pages.Products
{
    // A classe que o arquivo .razor irá herdar. 
    // Foi renomeada de 'CreateProductPage' para 'AdicionarBase' para clareza.
    public partial class AdicionarBase : ComponentBase
    {
        // 💡 INJEÇÕES DE DEPENDÊNCIA (Da sua página original)
        [Inject]
        public IProductHandler ProductHandler { get; set; } = default!;

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = default!;


        // 💡 ESTADO DA PÁGINA
        protected readonly CreateProductRequest _request = new();
        protected MudForm _form = null!; 
        protected bool _success;
        protected bool _isBusy; 
        protected string? _imageBase64Preview;


        // 💡 MÉTODOS DE LÓGICA (Da sua página original)

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
            if (_isBusy) return;
            
            await _form.Validate();
            if (!_form.IsValid) return;

            try
            {
                _isBusy = true;
                var response = await ProductHandler.CreateAsync(_request);

                if (response.IsSuccess)
                {
                    Snackbar.Add(response.Message ?? "Produto criado!", Severity.Success);
                    var createdProductSlug = response.Data?.Slug; 
                    
                    if (!string.IsNullOrEmpty(createdProductSlug))
                    {
                        Navigation.NavigateTo($"/produto/{createdProductSlug}");
                    }
                    else
                    {
                        Snackbar.Add("Produto criado, mas não foi possível obter o link direto.", Severity.Warning);
                    }
                }
                else
                {
                    Snackbar.Add(response.Message ?? "Erro ao criar produto.", Severity.Error);
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