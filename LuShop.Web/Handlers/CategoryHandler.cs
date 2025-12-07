using System.Net.Http.Json;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Categories;
using LuShop.Core.Responses;

namespace LuShop.Web.Handlers;

public class CategoryHandler(IHttpClientFactory httpClientFactory) : ICategoryHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<Response<Category?>> CreateAsync(CreateCategoryRequest request)
    {
        var result = await _client.PostAsJsonAsync("v1/categories", request);
        return await result.Content.ReadFromJsonAsync<Response<Category?>>()
               ?? new Response<Category?>(null, 400, "Falha ao criar categoria");
    }

    public async Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request)
    {
        var result = await _client.PutAsJsonAsync($"v1/categories/{request.Id}", request);
        return await result.Content.ReadFromJsonAsync<Response<Category?>>()
               ?? new Response<Category?>(null, 400, "Falha ao atualizar categoria");
    }

    public async Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request)
    {
        var result = await _client.DeleteAsync($"v1/categories/{request.Id}");
        return await result.Content.ReadFromJsonAsync<Response<Category?>>()
               ?? new Response<Category?>(null, 400, "Falha ao excluir categoria");
    }

    public async Task<Response<Category?>> GetByIdAsync(GetByIdCategoryRequest request)
    {
        return await _client.GetFromJsonAsync<Response<Category?>>($"v1/categories/{request.Id}")
               ?? new Response<Category?>(null, 400, "Não foi possível obter a categoria");
    }

    public async Task<Response<List<Category>?>> GetAllAsync(GetAllCategoriesRequest request)
    {
        // GET com Query String para paginação
        var url = $"v1/categories?pageNumber={request.PageNumber}&pageSize={request.PageSize}";
        
        return await _client.GetFromJsonAsync<Response<List<Category>?>>(url)
               ?? new Response<List<Category>?>(null, 400, "Não foi possível obter as categorias");
    }
}