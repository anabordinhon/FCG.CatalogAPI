using FCG.Catalog.Application.GamePurchases.Outputs;
using FCG.Catalog.Domain.GamePurchases.Entities;
using FCG.Catalog.Domain.Games.Entities;

namespace FCG.Catalog.Application.GamePurchases.Mappers;

public static class GamePurchaseOutputMapper
{
    public static GamePurchaseOutput ToOutput(this GamePurchase gamePurchase, Game game)
    {
        return new GamePurchaseOutput(
            gamePurchase.PublicId,
            game.PublicId,
            gamePurchase.DataGamePurchase,
            gamePurchase.FinalPrice.Value,
            gamePurchase.PromotionValue?.Value
        );
    }

    public static List<GamePurchaseOutput> ToOutput(this IEnumerable<GamePurchase> purchases)
    {
        return purchases.Select(purchase => purchase.ToOutput(purchase.Game)).ToList();
    }
}