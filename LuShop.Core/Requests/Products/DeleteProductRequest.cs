using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.Products;

public class DeleteProductRequest
{
    [Required(ErrorMessage = "O ID do produto é obrigatório")]
    public long Id { get; set; }
}