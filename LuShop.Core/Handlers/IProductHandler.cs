using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;

namespace LuShop.Core.Handlers;

public interface IProductHandler
{
    // Escrita (Admin)
    Task<Response<Product?>> CreateAsync(CreateProductRequest request);
    Task<Response<Product?>> UpdateAsync(UpdateProductRequest request);
    Task<Response<Product?>> DeleteAsync(DeleteProductRequest request);
    // Muito útil para SEO e URLs amigáveis (ex: lushop.com/produto/iphone-15-pro)
    Task<Response<Product?>> GetBySlugAsync(GetProductBySlugRequest request); 
    // Catálogo da loja (Paginado)
    Task<PagedResponse<List<Product>?>> GetAllAsync(GetAllProductsRequest request);
}
