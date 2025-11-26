using LuShop.Core.Enums;

namespace LuShop.Core.Models;

public class Piece
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Stock { get; set; }
    public EAvailabilityType Type { get; set; } = EAvailabilityType.On; 
    public decimal Price { get; set; }
    public long CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    //usado para representar um usuário do Identity
    public string UserId { get; set; } = string.Empty; 
    
}