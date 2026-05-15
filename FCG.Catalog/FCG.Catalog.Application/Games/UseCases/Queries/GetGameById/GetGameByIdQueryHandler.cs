using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Mappers;
using FCG.Catalog.Application.Games.Outputs;
using FCG.Catalog.Application.Games.Ports;
using FCG.Catalog.Domain.Games.Ports;

namespace FCG.Catalog.Application.Games.UseCases.Queries.GetGameById;

public class GetGameByIdQueryHandler : IGetGameByIdQueryHandler
{
    private readonly IGameQueryRepository _gameQueryRepository;
    private readonly IGameCacheService _gameCacheService;

    public GetGameByIdQueryHandler(
        IGameQueryRepository gameQueryRepository,
        IGameCacheService gameCacheService)
    {
        _gameQueryRepository = gameQueryRepository;
        _gameCacheService = gameCacheService;
    }

    public async Task<ResultData<GameOutput>> Handle(GetGameByIdQuery query, CancellationToken cancellationToken)
    {
        var cachedGame = await _gameCacheService.GetByPublicIdAsync(query.PublicId, cancellationToken);

        if (cachedGame is not null)
        {
            return ResultData<GameOutput>.Success(cachedGame);
        }

        var game = await _gameQueryRepository.GetByIdAsync(query.PublicId, cancellationToken);

        if (game is null)
        {
            return ResultData<GameOutput>.Error("Jogo não encontrado.");
        }

        var gameOutput = game.ToOutput();
        await _gameCacheService.SetAsync(gameOutput, cancellationToken);

        return ResultData<GameOutput>.Success(gameOutput);
    }
}
