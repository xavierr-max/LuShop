using System.ComponentModel.DataAnnotations;

namespace LuShop.Core.Requests.Vouchers;

public class UpdateVoucherRequest
{
    public long Id { get; set; } // Necessário ID para saber qual atualizar
    
    [Required(ErrorMessage = "O título é obrigatório")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O número é obrigatório")]
    public string Number { get; set; } = string.Empty; // Código do voucher
    
    public decimal Amount { get; set; } // Valor de desconto ou preço
}