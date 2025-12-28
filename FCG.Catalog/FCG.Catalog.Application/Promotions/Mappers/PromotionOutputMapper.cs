using FCG.Catalog.Application.Promotions.Outputs;
using FCG.Catalog.Domain.Promotions.Entities;
using FCG.Catalog.Domain.Promotions.Enum;
using System.Linq;

namespace FCG.Catalog.Application.Promotions.Mappers;

public static class PromotionOutputMapper
{
    public static PromotionOutput ToOutput(this Promotion promotion)
    {
        var discountValue = promotion.DiscountRule.Type == DiscountTypeEnum.Percentual
            ? promotion.DiscountRule.Percentage
            : promotion.DiscountRule.FixedAmount;

        var gameIds = promotion.Games?.Select(g => g.PublicId).ToList() ?? new List<Guid>();

        return new PromotionOutput(
            promotion.PublicId,
            promotion.Description,
            promotion.Period.StartDate,
            promotion.Period.EndDate,
            promotion.Status.ToString(),
            discountValue,
            promotion.DiscountRule.Type.ToString(),
            gameIds
        );
    }

    public static List<PromotionOutput> ToOutput(this IEnumerable<Promotion> promotion)
    {
        return promotion.Select(ToOutput).ToList();
    }
}