using FCG.Catalog.Domain.Games.Entities;

namespace FCG.Catalog.Domain.Games.Ports;

public interface IGameCommandRepository
{
    Task<Game> AddAsync(Game game, CancellationToken cancellationToken);
    Task<Game> UpdateAsync(Game game, CancellationToken cancellationToken);
    Task<bool> GameExistsAsync(Guid? publicId, string description, string developer, CancellationToken cancellationToken);
    Task<Game> GetByIdAsync(Guid publicId, CancellationToken cancellationToken);
}