namespace LuShop.Core.Entities;
/// <summary>
/// Usado nas classes que possuem uma continuidade de vida, mesmo que seus dados mudem, ela continua sendo a mesma.
/// </summary>
public abstract class Entity
{
    protected Entity(Guid id, DateTime createdAt, DateTime? updateAt)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdateAt = null;
    }

    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdateAt { get; protected set; }

    protected void Touch()
    {
        UpdateAt = DateTime.UtcNow;
    }
}