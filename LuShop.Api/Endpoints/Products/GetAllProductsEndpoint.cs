using LuShop.Api.Common.Api;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Products;
using LuShop.Core.Responses;
using Microsoft.AspNetCore.Mvc; // Para o atributo [FromQuery] se necessário, mas [AsParameters] resolve

namespace LuShop.Api.Endpoints.Products;

public class GetAllProductsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Products: Get All")
            .WithSummary("Recupera todos os produtos")
            .WithDescription("Recupera uma lista paginada de produtos ordenados por título")
            .WithOrder(5)
            .Produces<PagedResponse<List<Product>?>>();

    private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        // 👇 Vamos receber os parâmetros explicitamente para garantir que o binding funcione
        [FromQuery] string? title,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        // Montamos o request manualmente
        var request = new GetAllProductsRequest
        {
            Title = title,
            PageNumber = pageNumber ?? 1, // Se vier nulo, usa 1
            PageSize = pageSize ?? 25     // Se vier nulo, usa 25
        };
        
        var result = await handler.GetAllAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}