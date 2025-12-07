using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Carts;
using LuShop.Core.Responses;

namespace LuShop.Core.Handlers;

public interface ICartHandler
{
    // Obtém o carrinho atual do usuário (ou cria um novo se não existir)
    Task<Response<Cart?>> GetByUserAsync(GetCartRequest request);

    // Adiciona um item e retorna o Carrinho atualizado (com novos totais)
    Task<Response<Cart?>> AddItemAsync(AddCartItemRequest request);

    // Atualiza quantidade (+ ou -) e retorna o Carrinho atualizado
    Task<Response<Cart?>> UpdateItemAsync(UpdateCartItemRequest request);

    // Remove um item e recalcula o total
    Task<Response<Cart?>> RemoveItemAsync(RemoveCartItemRequest request);

    // Limpa o carrinho (ex: após finalizar compra)
    Task<Response<Cart?>> ClearAsync(ClearCartRequest request);
}