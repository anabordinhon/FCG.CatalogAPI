using FCG.Catalog.Application.Common;
using FCG.Catalog.Domain.Promotions.Entities;

namespace FCG.Catalog.Domain.Promotions.Ports;

public interface IPromotionQueryRepository
{
    Task<Promotion> GetByIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<PagedResult<Promotion>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<Promotion>> GetPromotionByGameIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<IEnumerable<Promotion>> GetPromotionByUserIdAsync(int userId, CancellationToken cancellationToken);
}