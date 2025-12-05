    using LuShop.Core.Handlers;
    using LuShop.Core.Models;
    using LuShop.Core.Requests.Products;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    namespace LuShop.Web.Pages;

    public partial class Home : ComponentBase
    {
        // Injeção de Dependências
        [Inject] public IProductHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        // Variáveis de Estado
        public bool IsBusy { get; set; } = true;
        public List<Product> Products { get; set; } = new();
        public int TotalPages { get; set; } = 1;

        // Configuração da Requisição (Paginação Inicial)
        public GetAllProductsRequest Request { get; set; } = new()
        {
            PageNumber = 1,
            PageSize = 8 // Mostra 8 produtos por página (fica bom em grids de 4 colunas)
        };

        // Método executado ao iniciar a página
        protected override async Task OnInitializedAsync()
        {
            await LoadProductsAsync();
        }

        // Função que chama a API
        private async Task LoadProductsAsync()
        {
            IsBusy = true;
            try
            {
                var result = await Handler.GetAllAsync(Request);

                if (result.IsSuccess)
                {
                    Products = result.Data ?? new List<Product>();
                    
                    // Cálculo de páginas: Teto(Total / TamanhoPagina)
                    // O PagedResponse geralmente retorna o TotalCount. 
                    // Assumindo que seu PagedResponse tenha a propriedade TotalCount:
                    var totalCount = result.TotalCount; 
                    TotalPages = (int)Math.Ceiling((double)totalCount / Request.PageSize);
                }
                else
                {
                    // Mostra erro discreto se falhar
                    Snackbar.Add(result.Message ?? "Erro ao carregar produtos", Severity.Error);
                }
            }
            catch
            {
                Snackbar.Add("Falha de conexão com o servidor.", Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Evento disparado quando o usuário clica na paginação
        private async Task OnPageChanged(int page)
        {
            Request.PageNumber = page;
            await LoadProductsAsync();
            
            // Rola a tela para o topo suavemente (Opcional, mas boa UX)
            // Precisaria injetar IJSRuntime para isso, mas o padrão já funciona bem.
        }
        
        
    }