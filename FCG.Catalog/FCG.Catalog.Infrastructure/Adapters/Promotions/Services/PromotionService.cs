using FCG.Catalog.Application.Common.Outputs;
using FCG.Catalog.Domain.Common.Ports;
using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.Games.Ports;
using FCG.Catalog.Domain.Promotions.Enum;
using FCG.Catalog.Domain.Promotions.Ports;

namespace FCG.Catalog.Infraestructure.Adapters.Promotions.Services;

public class PromotionService : IPromotionService
{
    public IPromotionQueryRepository _promotionService;

    public PromotionService(
        IPromotionQueryRepository promotionService)
    {
        _promotionService = promotionService;
    }

    public async Task<PromotionServiceResult> GetBestDiscountAsync(Price price, Guid gameId, int userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var promotionsUser = await _promotionService.GetPromotionByUserIdAsync(userId, cancellationToken);
        var promotionsGame = await _promotionService.GetPromotionByGameIdAsync(gameId, cancellationToken);

        var allPromotions = promotionsGame
            .Concat(promotionsUser)
            .Where(p => p.Status == PromotionStatusEnum.Ativo && p.Period.IsActive(now))
            .ToList();

        if (!allPromotions.Any())
            return new PromotionServiceResult(0, Price.Create(0));

        var bestPromotion = allPromotions
               .Select(p => new
               {
                   Promotion = p,
                   DiscountValue = p.DiscountRule.CalculateDiscount(price.Value)
               })
               .OrderByDescending(x => x.DiscountValue)
               .First();

        return new PromotionServiceResult(bestPromotion.Promotion.Id, new Price(bestPromotion.DiscountValue));
    }
}