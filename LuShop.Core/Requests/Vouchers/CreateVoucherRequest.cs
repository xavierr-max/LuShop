namespace LuShop.Core.Requests.Vouchers;
using System.ComponentModel.DataAnnotations;

public class CreateVoucherRequest
{
    [Required(ErrorMessage = "O título é obrigatório")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O número é obrigatório")]
    public string Number { get; set; } = string.Empty; // Código do voucher
    
    public decimal Amount { get; set; } // Valor de desconto ou preço
}