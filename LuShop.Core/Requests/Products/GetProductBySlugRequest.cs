using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.Products;

public class GetProductBySlugRequest{
    [Required(ErrorMessage = "O Slug é obrigatório")]
    public string Slug { get; set; } = string.Empty;
}