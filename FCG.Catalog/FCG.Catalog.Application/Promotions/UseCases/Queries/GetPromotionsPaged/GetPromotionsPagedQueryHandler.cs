using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Promotions.Mappers;
using FCG.Catalog.Application.Promotions.Outputs;
using FCG.Catalog.Domain.Promotions.Ports;

namespace FCG.Catalog.Application.Promotions.UseCases.Queries.GetPromotionsPaged;

public class GetPromotionsPagedQueryHandler : IGetPromotionsPagedQueryHandler
{
    private readonly IPromotionQueryRepository _promotionQueryRepository;
    public GetPromotionsPagedQueryHandler(IPromotionQueryRepository promotionQueryRepository)
    {
        _promotionQueryRepository = promotionQueryRepository;
    }
    public async Task<ResultData<PagedResult<PromotionOutput>>> Handle(GetPromotionsPagedQuery query, CancellationToken cancellationToken)
    {
        var pagedResult = await _promotionQueryRepository.GetPagedAsync(
                query.Page,
                query.PageSize,
                cancellationToken);

        var items = pagedResult.Items.ToOutput();

        var pagedResultOutput = new PagedResult<PromotionOutput>
        {
            Items = items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };

        return ResultData<PagedResult<PromotionOutput>>.Success(pagedResultOutput);

    }
}