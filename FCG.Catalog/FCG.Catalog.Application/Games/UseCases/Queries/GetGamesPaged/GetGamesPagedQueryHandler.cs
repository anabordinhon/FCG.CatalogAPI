using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Mappers;
using FCG.Catalog.Application.Games.Outputs;
using FCG.Catalog.Domain.Games.Ports;

namespace FCG.Catalog.Application.Games.UseCases.Queries.GetGamesPaged;

public class GetGamesPagedQueryHandler : IGetGamesPagedQueryHandler
{
    private readonly IGameQueryRepository _gameQueryRepository;

    public GetGamesPagedQueryHandler(IGameQueryRepository gameQueryRepository)
    {
        _gameQueryRepository = gameQueryRepository;
    }

    public async Task<ResultData<PagedResult<GameOutput>>> Handle(GetGamesPagedQuery query, CancellationToken cancellationToken)
    {
        var pagedResult = await _gameQueryRepository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);

        var items = pagedResult.Items.ToOutput();

        var pagedResultGameOutput = new PagedResult<GameOutput>
        {
            Items = items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };

        return ResultData<PagedResult<GameOutput>>.Success(pagedResultGameOutput);
    }

}