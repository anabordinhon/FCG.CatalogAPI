namespace FCG.Catalog.Application.Promotions.Outputs;

public record PromotionOutput(
    Guid PublicId,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    decimal DiscountValue,
    string DiscountType,
    IList<Guid> GamePublicIds
);