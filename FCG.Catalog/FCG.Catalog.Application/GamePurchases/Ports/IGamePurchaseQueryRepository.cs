using FCG.Catalog.Application.Common;
using FCG.Catalog.Domain.GamePurchases.Entities;

namespace FCG.Catalog.Application.GamePurchases.Ports;

public interface IGamePurchaseQueryRepository
{
    Task<PagedResult<GamePurchase>> GetByUserGamePurchasesAsync(int page, int pageSize, int userId, CancellationToken cancellationToken);
}