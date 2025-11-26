namespace LuShop.Core.Entities;

public class Product : Entity
{
    private readonly List<Sku> _skus = new();
    
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public Slug Slug { get; set; }
    public Material Material { get; set; }
    public Image Image { get; set; }
    public Money Price { get; set; }
    public Dimensions Dimensions { get; set; }
    public IReadOnlyCollection<Sku> Skus => _skus.AsReadOnly();
}