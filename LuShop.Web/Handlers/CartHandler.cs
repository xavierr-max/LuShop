using System.Net.Http.Json;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Carts;
using LuShop.Core.Responses;

namespace LuShop.Web.Handlers;

public class CartHandler(IHttpClientFactory httpClientFactory) : ICartHandler
{
    // Cria um cliente HTTP com o nome "API" (configurado no Program.cs)
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<Response<Cart?>> GetByUserAsync(GetCartRequest request)
    {
        // GET: v1/carts
        // Note que não mandamos o "request" no corpo, pois o GET não tem corpo.
        // O Backend vai saber quem é o usuário pelo Token JWT no header.
        try 
        {
            return await _client.GetFromJsonAsync<Response<Cart?>>("v1/carts") 
                   ?? new Response<Cart?>(null, 400, "Falha ao obter carrinho");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Erro de conexão");
        }
    }

    public async Task<Response<Cart?>> AddItemAsync(AddCartItemRequest request)
    {
        // POST: v1/carts/item
        try
        {
            var result = await _client.PostAsJsonAsync("v1/carts/item", request);
            return await result.Content.ReadFromJsonAsync<Response<Cart?>>()
                   ?? new Response<Cart?>(null, 400, "Falha ao adicionar item");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Faça login para adicionar ao carrinho");
        }
    }

    public async Task<Response<Cart?>> UpdateItemAsync(UpdateCartItemRequest request)
    {
        // PUT: v1/carts/item/{id}
        try
        {
            var result = await _client.PutAsJsonAsync($"v1/carts/item/{request.CartItemId}", request);
            return await result.Content.ReadFromJsonAsync<Response<Cart?>>()
                   ?? new Response<Cart?>(null, 400, "Falha ao atualizar item");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Erro ao conectar ao servidor");
        }
    }

    public async Task<Response<Cart?>> RemoveItemAsync(RemoveCartItemRequest request)
    {
        // DELETE: v1/carts/item/{id}
        try
        {
            var result = await _client.DeleteAsync($"v1/carts/item/{request.CartItemId}");
            return await result.Content.ReadFromJsonAsync<Response<Cart?>>()
                   ?? new Response<Cart?>(null, 400, "Falha ao remover item");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Erro ao conectar ao servidor");
        }
    }

    public async Task<Response<Cart?>> ClearAsync(ClearCartRequest request)
    {
        // DELETE: v1/carts
        try
        {
            var result = await _client.DeleteAsync("v1/carts");
            return await result.Content.ReadFromJsonAsync<Response<Cart?>>()
                   ?? new Response<Cart?>(null, 400, "Falha ao limpar carrinho");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Erro ao conectar ao servidor");
        }
    }
}