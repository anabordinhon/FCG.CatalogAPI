using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Outputs;

namespace FCG.Catalog.Application.Games.UseCases.Queries.GetGameById;

public interface IGetGameByIdQueryHandler
{
    Task<ResultData<GameOutput>> Handle(GetGameByIdQuery query, CancellationToken cancellationToken);
}