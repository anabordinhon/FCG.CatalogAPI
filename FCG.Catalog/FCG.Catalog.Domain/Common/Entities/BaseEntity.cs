namespace FCG.Catalog.Domain.Common.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
}