using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Promotions.Mappers;
using FCG.Catalog.Application.Promotions.Outputs;
using FCG.Catalog.Domain.Promotions.Ports;

namespace FCG.Catalog.Application.Promotions.UseCases.Queries.GetPromotionById;

public class GetPromotionByIdQueryHandler : IGetPromotionByIdQueryHandler
{
    private readonly IPromotionQueryRepository _promotionQueryRepository;
    public GetPromotionByIdQueryHandler(IPromotionQueryRepository promotionQueryRepository)
    {
        _promotionQueryRepository = promotionQueryRepository;
    }
    public async Task<ResultData<PromotionOutput>> Handle(GetPromotionByIdQuery query, CancellationToken cancellationToken)
    {
        var promotion = await _promotionQueryRepository.GetByIdAsync(query.PublicId, cancellationToken);

        if (promotion is null)
        {
            return ResultData<PromotionOutput>.Error("Promoção não encontrada.");
        }

        var promotionOutput = promotion.ToOutput();

        return ResultData<PromotionOutput>.Success(promotionOutput);

    }
}