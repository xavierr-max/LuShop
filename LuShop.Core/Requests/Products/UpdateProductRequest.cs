using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.Products;

public class UpdateProductRequest
{
    [Required]
    public long Id { get; set; } // Obrigatório para saber quem atualizar

    [Required(ErrorMessage = "O título do produto é obrigatório")]
    [MaxLength(80, ErrorMessage = "O título deve ter no máximo 80 caracteres")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A categoria é obrigatória")]
    public long CategoryId { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço é obrigatório")]
    [DataType(DataType.Currency)]
    [Range(0.1, 999999, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    // NOVOS CAMPOS PARA ATUALIZAÇÃO DA IMAGEM
    public string Base64Image { get; set; } = string.Empty; 
    public string FileName { get; set; } = string.Empty;
}