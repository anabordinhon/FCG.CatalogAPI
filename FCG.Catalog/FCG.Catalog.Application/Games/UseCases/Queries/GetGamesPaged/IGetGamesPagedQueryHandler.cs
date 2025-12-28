using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Outputs;

namespace FCG.Catalog.Application.Games.UseCases.Queries.GetGamesPaged;

public interface IGetGamesPagedQueryHandler
{
    Task<ResultData<PagedResult<GameOutput>>> Handle(GetGamesPagedQuery query, CancellationToken cancellationToken);
}