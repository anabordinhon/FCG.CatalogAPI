using FCG.Catalog.Domain.GamePurchases.Entities;

namespace FCG.Catalog.Application.GamePurchases.Ports;

public interface IGamePurchaseCommandRepository
{
    Task<GamePurchase> AddAsync(GamePurchase gamePurchase, CancellationToken cancellationToken);
    Task<GamePurchase> UpdateAsync(GamePurchase gamePurchase, CancellationToken cancellationToken);

}