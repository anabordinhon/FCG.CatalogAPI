using FCG.Catalog.Application.GamePurchases.Outputs;

namespace FCG.Catalog.Application.GamePurchases.Ports;

public interface ITopSellingGamesCacheService
{
    Task<IReadOnlyCollection<TopSellingGameOutput>> RefreshTopSellingGamesAsync(CancellationToken cancellationToken);
}
