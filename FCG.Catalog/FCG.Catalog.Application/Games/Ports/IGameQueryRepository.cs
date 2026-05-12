using FCG.Catalog.Application.Common;
using FCG.Catalog.Domain.Games.Entities;

namespace FCG.Catalog.Domain.Games.Ports;

public interface IGameQueryRepository
{
    Task<Game?> GetByIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<PagedResult<Game>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Game?> GetByIdWithPromotionsAsync(Guid publicId, CancellationToken cancellationToken);
}
