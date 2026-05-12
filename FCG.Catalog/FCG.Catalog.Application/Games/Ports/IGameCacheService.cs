using FCG.Catalog.Application.Games.Outputs;

namespace FCG.Catalog.Application.Games.Ports;

public interface IGameCacheService
{
    Task<GameOutput?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task SetAsync(GameOutput game, CancellationToken cancellationToken);
}
