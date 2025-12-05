using LuShop.Core.Models;
using LuShop.Core.Requests.Categories;
using LuShop.Core.Responses;

namespace LuShop.Core.Handlers;

public interface ICategoryHandler
{
    Task<Response<Category?>> CreateAsync(CreateCategoryRequest request);
    Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request);
    Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request);
    Task<Response<Category?>> GetByIdAsync(GetByIdCategoryRequest request);
    Task<Response<List<Category>?>> GetAllAsync(GetAllCategoriesRequest request);
}