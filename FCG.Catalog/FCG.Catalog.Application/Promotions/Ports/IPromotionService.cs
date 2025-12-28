using FCG.Catalog.Application.Common.Outputs;
using FCG.Catalog.Domain.Common.ValueObjects;

namespace FCG.Catalog.Domain.Promotions.Ports;

public interface IPromotionService
{
    Task<PromotionServiceResult> GetBestDiscountAsync(Price price, Guid gameId, int userId, CancellationToken cancellationToken);
}