namespace FCG.Catalog.Application.Events;

internal class OrderPlacedEvent
{
    public Guid OrderId { get; init; }
    public int UserId { get; init; }
    public Guid GameId { get; init; }
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
}
