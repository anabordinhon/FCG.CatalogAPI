using FCG.Catalog.Application.Common;
using FCG.Catalog.Domain.GamePurchases.Entities;

namespace FCG.Catalog.Application.GamePurchases.Ports;

public interface IGamePurchaseQueryRepository
{
    Task<PagedResult<GamePurchase>> GetByUserGamePurchasesPagedAsync(int page, int pageSize, int userId, CancellationToken cancellationToken);
    Task<bool> AnyByUserGamePurchasesAsync(int userId, Guid gameId, CancellationToken cancellationToken);
}