using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.GamePurchases.Mappers;
using FCG.Catalog.Application.GamePurchases.Outputs;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Domain.Common.Ports;


namespace FCG.Catalog.Application.GamePurchases.UseCases.Queries;

public class GetByUserGamePurchasesQueryHandler : IGetByUserGamePurchasesQueryHandler
{
    private readonly IGamePurchaseQueryRepository _gamePurchaseQueryRepository;
    private readonly IUserContext _userContext;
    public GetByUserGamePurchasesQueryHandler(IUserContext userContext, IGamePurchaseQueryRepository gamePurchaseQueryRepository)
    {
        _gamePurchaseQueryRepository = gamePurchaseQueryRepository;
        _userContext = userContext;
    }
    public async Task<ResultData<PagedResult<GamePurchaseOutput>>> Handle(GetByUserGamePurchaseQuery query, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();

        var pagedResult = await _gamePurchaseQueryRepository.GetByUserGamePurchasesPagedAsync(query.Page, query.PageSize, userId, cancellationToken);

        var items = pagedResult.Items.ToOutput();

        var pagedResultGamePurchaseOutput = new PagedResult<GamePurchaseOutput>
        {
            Items = items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };

        return ResultData<PagedResult<GamePurchaseOutput>>.Success(pagedResultGamePurchaseOutput);
    }
}