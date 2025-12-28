using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Outputs;

namespace FCG.Catalog.Application.Games.UseCases.Commands.AddGame;

public interface IAddOrUpdateGameCommandHandler
{
    Task<ResultData<GameOutput>> Handle(AddOrUpdateGameCommand command, CancellationToken cancellationToken);
}