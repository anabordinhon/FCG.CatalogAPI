namespace FCG.Catalog.Application.Events;
public class PaymentProcessedEvent
{
    public Guid OrderId { get; init; }
    public int UserId { get; init; }
    public Guid GameId { get; init; }
    public string Status { get; init; }
    public DateTime ProcessedAt { get; init; }
    //TODO devo colocar atualizado em no gamepurchase ver
}
