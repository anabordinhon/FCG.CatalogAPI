using FCG.Catalog.Domain.Common.Entities;
using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.GamePurchases.Enum;
using FCG.Catalog.Domain.Games.Entities;

namespace FCG.Catalog.Domain.GamePurchases.Entities;

public class GamePurchase : BaseEntity
{
    private GamePurchase(int userId, int gameId, DateTime dataGamePurchase, Price finalPrice, Price promotionValue, EStatusPurchase statusPurchase, int? promotionId)
    {
        UserId = userId;
        GameId = gameId;
        DataGamePurchase = dataGamePurchase;
        FinalPrice = finalPrice;
        PromotionValue = promotionValue;
        PromotionId = promotionId;
        StatusPurchase = statusPurchase;
    }
    private GamePurchase() { }

    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public int UserId { get; private set; }
    public int GameId { get; private set; }
    public DateTime DataGamePurchase { get; private set; }
    public Price FinalPrice { get; private set; } = default!;
    public Price? PromotionValue { get; private set; }
    public EStatusPurchase StatusPurchase { get; set; }
    public Game Game { get; private set; } = default!;
    public int? PromotionId { get; private set; }

    public static GamePurchase Create(int userId, int gameId, Price finalPrice, Price promotionValue, EStatusPurchase statusPurchase, int? promotionId)
    {
        GamePurchase gamePurcharse = new GamePurchase(userId, gameId, DateTime.UtcNow, finalPrice, promotionValue, statusPurchase, promotionId);
        return gamePurcharse;
    }

}