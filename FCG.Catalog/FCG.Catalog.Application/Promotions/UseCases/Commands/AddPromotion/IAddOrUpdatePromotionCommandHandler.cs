using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Promotions.Outputs;

namespace FCG.Catalog.Application.Promotions.UseCases.Commands.AddPromotion;

public interface IAddOrUpdatePromotionCommandHandler
{
    Task<ResultData<PromotionOutput>> Handle(AddOrUpdatePromotionCommand command, CancellationToken cancellationToken);
}