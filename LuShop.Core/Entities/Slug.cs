using LuShop.Core.ValueObjects;

namespace LuShop.Core.Entities;

public class Slug : ValueObject
{
    public string Value { get;}

    public Slug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Slug inválido");
        
        Value = text.ToLower().Trim().Replace(" ", "-");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}