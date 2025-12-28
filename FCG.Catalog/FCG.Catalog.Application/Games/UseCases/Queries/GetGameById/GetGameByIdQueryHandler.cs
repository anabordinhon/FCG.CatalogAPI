using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Mappers;
using FCG.Catalog.Application.Games.Outputs;
using FCG.Catalog.Domain.Games.Ports;

namespace FCG.Catalog.Application.Games.UseCases.Queries.GetGameById;

public class GetGameByIdQueryHandler : IGetGameByIdQueryHandler
{
    private readonly IGameQueryRepository _gameQueryRepository;
    public GetGameByIdQueryHandler(IGameQueryRepository gameQueryRepository)
    {
        _gameQueryRepository = gameQueryRepository;
    }

    public async Task<ResultData<GameOutput>> Handle(GetGameByIdQuery query, CancellationToken cancellationToken)
    {
        var game = await _gameQueryRepository.GetByIdAsync(query.PublicId, cancellationToken);

        if (game is null)
        {
            return ResultData<GameOutput>.Error("Jogo não encontrado.");
        }

        var gameOutput = game.ToOutput();

        return ResultData<GameOutput>.Success(gameOutput);
    }
}