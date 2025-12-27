using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Promotions.Outputs;

namespace FCG.Catalog.Application.Promotions.UseCases.Queries.GetPromotionById;

public interface IGetPromotionByIdQueryHandler
{
    Task<ResultData<PromotionOutput>> Handle(GetPromotionByIdQuery query, CancellationToken cancellationToken);
}