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
        var result = await _client.PostAsJsonAsync(BaseUrl, request);
        return await result.Content.ReadFromJsonAsync<Response<Product?>>()
               ?? new Response<Product?>(null, 400, "Falha ao criar o produto.");
    }

    public async Task<Response<Product?>> UpdateAsync(UpdateProductRequest request)
    {
        var result = await _client.PutAsJsonAsync($"{BaseUrl}/{request.Id}", request);
        return await result.Content.ReadFromJsonAsync<Response<Product?>>()
               ?? new Response<Product?>(null, 400, "Falha ao atualizar o produto.");
    }

    public async Task<Response<Product?>> DeleteAsync(DeleteProductRequest request)
    {
        var result = await _client.DeleteAsync($"{BaseUrl}/{request.Id}");
        return await result.Content.ReadFromJsonAsync<Response<Product?>>()
               ?? new Response<Product?>(null, 400, "Falha ao excluir o produto.");
    }

    public async Task<Response<Product?>> GetBySlugAsync(GetProductBySlugRequest request)
    {
        try
        {
            // Tenta buscar o produto
            return await _client.GetFromJsonAsync<Response<Product?>>($"{BaseUrl}/{request.Slug}")
                   ?? new Response<Product?>(null, 404, "Produto não encontrado.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Se a API retornar 404, capturamos o erro e retornamos null sem quebrar o site
            return new Response<Product?>(null, 404, "Produto não encontrado.");
        }
        catch (Exception)
        {
            // Qualquer outro erro genérico
            return new Response<Product?>(null, 500, "Erro ao conectar com o servidor.");
        }
    }

    public async Task<PagedResponse<List<Product>?>> GetAllAsync(GetAllProductsRequest request)
    {
        // Monta a URL básica
        var url = $"{BaseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

        // ✅ MELHORIA: Verifica se tem filtro de título e adiciona na URL
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            url += $"&title={request.Title}";
        }

        return await _client.GetFromJsonAsync<PagedResponse<List<Product>?>>(url)
               ?? new PagedResponse<List<Product>?>(null, 400, "Não foi possível buscar os produtos.");
    }
}