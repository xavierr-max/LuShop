using LuShop.Api.Data;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.CartItems;
using LuShop.Core.Requests.Carts;
using LuShop.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Handlers;

public class CartHandler(AppDbContext context) : ICartHandler
{
    // 1. OBTER CARRINHO (ou criar se não existir)
    public async Task<Response<Cart?>> GetByUserAsync(GetCartRequest request)
    {
        try
        {
            var cart = await GetCartWithItemsAsync(request.UserId);

            if (cart is null)
            {
                // Lazy Creation: Se não existe, cria um vazio agora
                cart = new Cart { UserId = request.UserId };
                await context.Carts.AddAsync(cart);
                await context.SaveChangesAsync();
            }

            return new Response<Cart?>(cart, 200, "Carrinho obtido com sucesso");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Falha ao obter carrinho");
        }
    }

    // 2. ADICIONAR ITEM
    public async Task<Response<Cart?>> AddItemAsync(AddCartItemRequest request)
    {
        try
        {
            // Verifica se o produto existe
            var product = await context.Products.FindAsync(request.ProductId);
            if (product is null)
                return new Response<Cart?>(null, 404, "Produto não encontrado");

            // Obtém o carrinho (ou cria)
            var cart = await GetCartWithItemsAsync(request.UserId);
            if (cart is null)
            {
                cart = new Cart { UserId = request.UserId };
                await context.Carts.AddAsync(cart);
                await context.SaveChangesAsync(); // Salva para gerar o ID do carrinho
            }

            // Verifica se o item já está no carrinho
            var item = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (item is null)
            {
                // Item novo
                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = request.Quantity
                };
                await context.CartItems.AddAsync(item);
            }
            else
            {
                // Item já existe, apenas soma a quantidade
                item.Quantity += request.Quantity;
                context.CartItems.Update(item);
            }

            await context.SaveChangesAsync();
            
            // Recarrega o carrinho completo para garantir totais atualizados
            // (Ou podemos apenas retornar 'cart' se não precisarmos recarregar dados do banco)
            return new Response<Cart?>(cart, 201, "Item adicionado ao carrinho");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Falha ao adicionar item ao carrinho");
        }
    }

    // 3. ATUALIZAR ITEM (Quantidade)
    public async Task<Response<Cart?>> UpdateItemAsync(UpdateCartItemRequest request)
    {
        try
        {
            var cart = await GetCartWithItemsAsync(request.UserId);
            if (cart is null)
                return new Response<Cart?>(null, 404, "Carrinho não encontrado");

            var item = cart.Items.FirstOrDefault(x => x.Id == request.CartItemId);
            if (item is null)
                return new Response<Cart?>(null, 404, "Item não encontrado no carrinho");

            // Atualiza quantidade
            item.Quantity = request.Quantity;

            // Se quantidade for zero ou menor, removemos o item?
            // Geralmente sim, mas aqui vou apenas atualizar. 
            // Se quiser remover, adicione: if (item.Quantity <= 0) context.CartItems.Remove(item);
            
            context.CartItems.Update(item);
            await context.SaveChangesAsync();

            return new Response<Cart?>(cart, 200, "Carrinho atualizado");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Falha ao atualizar item");
        }
    }

    // 4. REMOVER ITEM
    public async Task<Response<Cart?>> RemoveItemAsync(RemoveCartItemRequest request)
    {
        try
        {
            var cart = await GetCartWithItemsAsync(request.UserId);
            if (cart is null)
                return new Response<Cart?>(null, 404, "Carrinho não encontrado");

            var item = cart.Items.FirstOrDefault(x => x.Id == request.CartItemId);
            if (item is null)
                return new Response<Cart?>(null, 404, "Item não encontrado");

            context.CartItems.Remove(item);
            await context.SaveChangesAsync();

            // Remove da lista em memória para o retorno ficar correto sem precisar refazer a query
            cart.Items.Remove(item); 

            return new Response<Cart?>(cart, 200, "Item removido do carrinho");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Falha ao remover item");
        }
    }

    // 5. LIMPAR CARRINHO
    public async Task<Response<Cart?>> ClearAsync(ClearCartRequest request)
    {
        try
        {
            var cart = await GetCartWithItemsAsync(request.UserId);
            if (cart is null)
                return new Response<Cart?>(null, 404, "Carrinho não encontrado");

            // Remove todos os itens
            context.CartItems.RemoveRange(cart.Items);
            
            // Remove o Voucher também ao limpar
            cart.VoucherId = null; 
            cart.Voucher = null;

            context.Carts.Update(cart);
            await context.SaveChangesAsync();

            // Limpa lista em memória para retorno
            cart.Items.Clear();

            return new Response<Cart?>(cart, 200, "Carrinho limpo com sucesso");
        }
        catch
        {
            return new Response<Cart?>(null, 500, "Falha ao limpar carrinho");
        }
    }

    // MÉTODO AUXILIAR PRIVADO
    // Centraliza a lógica de "Include" para trazer Itens e Produtos
    private async Task<Cart?> GetCartWithItemsAsync(string userId)
    {
        return await context.Carts
            .Include(c => c.Items)          // Traz os itens do carrinho
            .ThenInclude(i => i.Product)    // Traz os dados do Produto (para saber Título, Preço, Imagem)
            .Include(c => c.Voucher)        // Traz o Voucher se houver
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}