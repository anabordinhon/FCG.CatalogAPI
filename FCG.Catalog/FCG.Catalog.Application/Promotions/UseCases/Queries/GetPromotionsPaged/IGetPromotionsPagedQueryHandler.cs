using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Promotions.Outputs;


namespace FCG.Catalog.Application.Promotions.UseCases.Queries.GetPromotionsPaged;

public interface IGetPromotionsPagedQueryHandler
{
    Task<ResultData<PagedResult<PromotionOutput>>> Handle(GetPromotionsPagedQuery query, CancellationToken cancellationToken);
}