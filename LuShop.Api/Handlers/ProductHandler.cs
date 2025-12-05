using LuShop.Api.Data;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Handlers;

public class ProductHandler(AppDbContext context) : IProductHandler
{
    public async Task<Response<Product?>> CreateAsync(CreateProductRequest request)
    {
        try
        {
            // 1. Gera o Slug simples (caixa baixa e espaços viram traços)
            var slug = request.Title.ToLower().Replace(" ", "-");

            // 2. Validação: Verifica se esse Slug já existe no banco
            // É importante verificar isso para não ter dois produtos com a mesma URL
            var slugExists = await context.Products
                .AnyAsync(x => x.Slug == slug);

            if (slugExists)
                return new Response<Product?>(null, 400, "Já existe um produto com este nome/link.");

            var imageUrl = "https://placehold.co/600x400"; // Imagem padrão caso não enviem nada

            if (!string.IsNullOrEmpty(request.Base64Image))
            {
                // Gera um nome único para não sobrescrever imagens
                var fileName = $"{Guid.NewGuid()}.jpg"; 
            
                // Remove o cabeçalho do base64 se vier (ex: "data:image/jpeg;base64,")
                var data = new System.Text.RegularExpressions.Regex(@"^data:image\/[a-z]+;base64,").Replace(request.Base64Image, "");
            
                var imageBytes = Convert.FromBase64String(data);

                // Define onde salvar (pasta wwwroot/images)
                // OBS: Você precisa criar essa pasta no seu projeto API
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                // Salva o arquivo no disco
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Salva o caminho relativo para o banco
                imageUrl = $"images/{fileName}";
            }

            // 2. Cria a entidade Produto
            var product = new Product
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Slug = slug,
                IsActive = request.IsActive,
                CategoryId = request.CategoryId,
                ImageUrl = imageUrl // <--- AQUI
            };

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            return new Response<Product?>(product, 201, "Produto criado com sucesso!");
        }
        catch (Exception ex)
        {
            // RETORNE ex.Message TEMPORARIAMENTE PARA DEBUGAR
            return new Response<Product?>(null, 500, $"Falha ao criar o produto. Erro: {ex.Message} - {ex.InnerException?.Message}");
        }
    }

    public async Task<Response<Product?>> UpdateAsync(UpdateProductRequest request)
    {
        try
        {
            // 1. Tenta encontrar o produto no banco
            var product = await context.Products.FindAsync(request.Id);

            if (product is null)
                return new Response<Product?>(null, 404, "Produto não encontrado.");

            // 2. Lógica de atualização do Slug
            // O Slug deve ser gerado a partir do Título
            var newSlug = request.Title.ToLower().Replace(" ", "-");

            // Verifica se o slug mudou em relação ao que já estava no banco
            if (product.Slug != newSlug)
            {
                // Se mudou, verifica se o novo slug já existe EM OUTRO produto
                var slugExists = await context.Products
                    .AnyAsync(x => x.Slug == newSlug && x.Id != request.Id);

                if (slugExists)
                    return new Response<Product?>(null, 400, "Já existe outro produto com este título/link.");
            
                // Se passou na validação, atualiza o slug
                product.Slug = newSlug;
            }

            // 3. Lógica de Atualização da Imagem
            if (!string.IsNullOrEmpty(request.Base64Image))
            {
                // --- Reutilizando a lógica de upload do CreateAsync ---
                
                // Gera um nome único para não sobrescrever imagens
                var fileName = $"{Guid.NewGuid()}.jpg"; 
            
                // Remove o cabeçalho do base64 se vier (ex: "data:image/jpeg;base64,")
                var data = new System.Text.RegularExpressions.Regex(@"^data:image\/[a-z]+;base64,").Replace(request.Base64Image, "");
            
                var imageBytes = Convert.FromBase64String(data);

                // Define onde salvar (pasta wwwroot/images)
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                // Salva o arquivo no disco (Substituindo o antigo)
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Salva o NOVO caminho relativo para o banco
                product.ImageUrl = $"images/{fileName}";
            }
            // OBS: Se request.Base64Image for nulo ou vazio, a Imagem URL existente é mantida.

            // 4. Atualiza os outros campos
            product.Title = request.Title;
            product.Description = request.Description;
            product.Price = request.Price;
            product.IsActive = request.IsActive;
            product.CategoryId = request.CategoryId;

            // 5. Salva no banco
            context.Products.Update(product);
            await context.SaveChangesAsync();

            return new Response<Product?>(product, 200, "Produto atualizado com sucesso!");
        }
        catch (Exception ex)
        {
            // Retorno detalhado para debug, semelhante ao CreateAsync
            return new Response<Product?>(null, 500, $"Falha ao atualizar o produto. Erro: {ex.Message} - {ex.InnerException?.Message}");
        }
    }

    public async Task<Response<Product?>> DeleteAsync(DeleteProductRequest request)
    {
        try
        {
            // 1. Busca o produto
            var product = await context.Products.FindAsync(request.Id);

            if (product is null)
                return new Response<Product?>(null, 404, "Produto não encontrado.");

            // 2. Hard Delete (Remove do DbSet)
            context.Products.Remove(product);
        
            // 3. Tenta salvar no banco
            await context.SaveChangesAsync();

            return new Response<Product?>(product, 200, "Produto excluído permanentemente.");
        }
        catch (DbUpdateException)
        {
            // 4. Captura erro de Foreign Key (Chave Estrangeira)
            // Isso acontece se você tentar apagar um produto que já tem pedidos vinculados
            return new Response<Product?>(null, 400, "Não é possível apagar este produto pois ele já possui pedidos vinculados.");
        }
        catch
        {
            return new Response<Product?>(null, 500, "Falha ao excluir o produto.");
        }
    }

    public async Task<Response<Product?>> GetBySlugAsync(GetProductBySlugRequest request)
    {
        try
        {
            var product = await context.Products
                .AsNoTracking() // Otimização: Não rastreia mudanças (leitura mais rápida)
                .FirstOrDefaultAsync(x => 
                        x.Slug == request.Slug && 
                        x.IsActive == true // Segurança: Só traz se estiver Ativo
                );

            if (product is null)
                return new Response<Product?>(null, 404, "Produto não encontrado.");

            return new Response<Product?>(product, 200, "Produto recuperado com sucesso.");
        }
        catch
        {
            return new Response<Product?>(null, 500, "Falha ao recuperar o produto.");
        }
    }

    public async Task<PagedResponse<List<Product>?>> GetAllAsync(GetAllProductsRequest request)
    {
        try
        {
            // 1. Query Base
            var query = context.Products
                .AsNoTracking();

            // ✅ A CORREÇÃO É ESTE BLOCO ABAIXO:
            // Verifica se veio algum texto na busca e aplica o filtro
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                // Filtra onde o título contém o texto (Case Insensitive forçado com ToLower)
                query = query.Where(x => x.Title.ToLower().Contains(request.Title.ToLower()));
            }

            // 2. Ordenação
            // Aplicamos o OrderBy depois do filtro
            query = query.OrderBy(x => x.Title);

            // 3. Contagem (Banco Hit #1)
            // O Count agora vai contar apenas os itens filtrados, e não o banco todo
            var count = await query.CountAsync();

            // 4. Paginação e Execução (Banco Hit #2)
            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // 5. Retorno
            return new PagedResponse<List<Product>?>(
                products,
                count,
                request.PageNumber,
                request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Product>?>(null, 500, "Não foi possível consultar os produtos.");
        }
    }
}