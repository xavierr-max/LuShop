using System.Net.Http.Json;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;

namespace LuShop.Web.Handlers;

public class ProductHandler(IHttpClientFactory httpClientFactory) : IProductHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);
    private const string BaseUrl = "v1/products";

    public async Task<Response<Product?>> CreateAsync(CreateProductRequest request)
    {
        try
        {
            var result = await _client.PostAsJsonAsync(BaseUrl, request);
            return await result.Content.ReadFromJsonAsync<Response<Product?>>()
                   ?? new Response<Product?>(null, 400, "Falha ao criar o produto.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar produto: {ex.Message}");
            return new Response<Product?>(null, 500, "Erro ao conectar ao servidor");
        }
    }

    public async Task<Response<Product?>> UpdateAsync(UpdateProductRequest request)
    {
        try
        {
            var result = await _client.PutAsJsonAsync($"{BaseUrl}/{request.Id}", request);
            return await result.Content.ReadFromJsonAsync<Response<Product?>>()
                   ?? new Response<Product?>(null, 400, "Falha ao atualizar o produto.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar produto: {ex.Message}");
            return new Response<Product?>(null, 500, "Erro ao conectar ao servidor");
        }
    }

    public async Task<Response<Product?>> DeleteAsync(DeleteProductRequest request)
    {
        try
        {
            var result = await _client.DeleteAsync($"{BaseUrl}/{request.Id}");
            return await result.Content.ReadFromJsonAsync<Response<Product?>>()
                   ?? new Response<Product?>(null, 400, "Falha ao excluir o produto.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar produto: {ex.Message}");
            return new Response<Product?>(null, 500, "Erro ao conectar ao servidor");
        }
    }

    public async Task<Response<Product?>> GetBySlugAsync(GetProductBySlugRequest request)
    {
        // ✅ VALIDAÇÃO CRÍTICA: Não faz requisição se o slug estiver vazio ou inválido
        if (string.IsNullOrWhiteSpace(request.Slug) || 
            request.Slug == "{Slug}" || 
            request.Slug.Contains('{') || 
            request.Slug.Contains('}'))
        {
            Console.WriteLine($"⚠️ Slug inválido detectado: '{request.Slug}' - requisição bloqueada");
            return new Response<Product?>(null, 400, "Slug inválido");
        }

        try
        {
            // Garante que o slug está encodado corretamente para URL
            var encodedSlug = Uri.EscapeDataString(request.Slug);
            var url = $"{BaseUrl}/{encodedSlug}";
            
            Console.WriteLine($"🔍 Buscando produto: {url}");
            
            return await _client.GetFromJsonAsync<Response<Product?>>(url)
                   ?? new Response<Product?>(null, 404, "Produto não encontrado.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine($"❌ Produto não encontrado: {request.Slug}");
            return new Response<Product?>(null, 404, "Produto não encontrado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao buscar produto '{request.Slug}': {ex.Message}");
            return new Response<Product?>(null, 500, "Erro ao conectar com o servidor.");
        }
    }

    public async Task<PagedResponse<List<Product>?>> GetAllAsync(GetAllProductsRequest request)
    {
        // Inicia a URL apenas com paginação
        var url = $"v1/products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

        // CORREÇÃO: Adiciona o CategoryId apenas se ele tiver um valor (para o filtro funcionar)
        if (request.CategoryId.HasValue)
        {
            url += $"&categoryId={request.CategoryId.Value}";
        }
        
        return await _client.GetFromJsonAsync<PagedResponse<List<Product>?>>(url)
               ?? new PagedResponse<List<Product>?>(null, 400, "Não foi possível obter os produtos");
    }
}