using LuShop.Api.Data;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Categories;
using LuShop.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Handlers;

public class CategoryHandler(AppDbContext context) : ICategoryHandler
{
    public async Task<Response<Category?>> CreateAsync(CreateCategoryRequest request)
    {
        try
        {
            var category = new Category
            {
                Title = request.Title,
                Description = request.Description
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return new Response<Category?>(category, 201, "Categoria criada com sucesso");
        }
        catch
        {
            return new Response<Category?>(null, 500, "Não foi possível criar a categoria");
        }
    }

    public async Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request)
    {
        try
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            category.Title = request.Title;
            category.Description = request.Description;

            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return new Response<Category?>(category, 200, "Categoria atualizada com sucesso");
        }
        catch
        {
            return new Response<Category?>(null, 500, "Não foi possível atualizar a categoria");
        }
    }

    public async Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request)
    {
        try
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return new Response<Category?>(category, 200, "Categoria excluída com sucesso");
        }
        catch
        {
            return new Response<Category?>(null, 500, "Não foi possível excluir a categoria");
        }
    }

    public async Task<Response<Category?>> GetByIdAsync(GetByIdCategoryRequest request)
    {
        try
        {
            var category = await context.Categories
                .AsNoTracking() // Otimização para leitura
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            return new Response<Category?>(category, 200, "Categoria encontrada");
        }
        catch
        {
            return new Response<Category?>(null, 500, "Não foi possível recuperar a categoria");
        }
    }

    public async Task<Response<List<Category>?>> GetAllAsync(GetAllCategoriesRequest request)
    {
        try
        {
            var categories = await context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Title) // É boa prática ordenar listas (alfabético aqui)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new Response<List<Category>?>(categories, 200, "Lista de categorias obtida com sucesso");
        }
        catch
        {
            return new Response<List<Category>?>(null, 500, "Não foi possível obter as categorias");
        }
    }
}