using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.Products;

public class CreateProductRequest
{
    [Required(ErrorMessage = "O título do produto é obrigatório")]
    [MaxLength(80, ErrorMessage = "O título deve ter no máximo 80 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A categoria é obrigatória")]
    public long CategoryId { get; set; } 

    [Required(ErrorMessage = "O preço é obrigatório")]
    [DataType(DataType.Currency)]
    [Range(0.1, 999999, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal Price { get; set; }
    
    // O Slug é o identificador na URL: "meu-produto-incrivel"
    public string Slug { get; set; } = string.Empty; 
    
    // O Frontend manda a imagem codificada aqui
    public string Base64Image { get; set; } = string.Empty; 
    
    // Opcional: Para saber a extensão (.jpg, .png)
    public string FileName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}