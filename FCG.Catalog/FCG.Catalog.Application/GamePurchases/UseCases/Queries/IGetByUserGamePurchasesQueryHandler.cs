using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.GamePurchases.Outputs;

namespace FCG.Catalog.Application.GamePurchases.UseCases.Queries;

public interface IGetByUserGamePurchasesQueryHandler
{
    Task<ResultData<PagedResult<GamePurchaseOutput>>> Handle(GetByUserGamePurchaseQuery query, CancellationToken cancellationToken);
}