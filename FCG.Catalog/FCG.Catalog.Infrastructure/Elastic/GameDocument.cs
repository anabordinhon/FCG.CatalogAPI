namespace FCG.Catalog.Infrastructure.Elastic;
public class GameDocument
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public string Developer { get; init; } = default!;
    public string Genre { get; init; } = default!;
    public decimal Price { get; init; }
    public DateTime IndexedAt { get; init; }
}
