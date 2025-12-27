using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Promotions.Mappers;
using FCG.Catalog.Application.Promotions.Outputs;
using FCG.Catalog.Domain.Common.Ports;
using FCG.Catalog.Domain.Games.Ports;
using FCG.Catalog.Domain.Promotions.Entities;
using FCG.Catalog.Domain.Promotions.Ports;

namespace FCG.Catalog.Application.Promotions.UseCases.Commands.AddPromotion;

public class AddOrUpdatePromotionCommandHandler : IAddOrUpdatePromotionCommandHandler
{
    private readonly IPromotionCommandRepository _promotionCommandRepository;
    private readonly IGameQueryRepository _gameQueryRepository;
    private readonly IUserContext _userContext;

    public AddOrUpdatePromotionCommandHandler(
        IPromotionCommandRepository promotionCommandRepository,
        IGameQueryRepository gameQueryRepository,
        IUserContext userContext)
    {
        _promotionCommandRepository = promotionCommandRepository;
        _gameQueryRepository = gameQueryRepository;
        _userContext = userContext;
    }

    public async Task<ResultData<PromotionOutput>> Handle(AddOrUpdatePromotionCommand command, CancellationToken cancellationToken)
    {
        var promotionExists = command.PublicId is not null
            && await _promotionCommandRepository.PromotionExistsAsync(command.PublicId, cancellationToken);

        var promotion = Promotion.Create(command.Description, command.Period, command.DiscountRule);

        promotion.CheckVigency(DateTime.UtcNow);

        foreach (var gamePublicId in command.GamePublicIds.Distinct())
        {
            var game = await _gameQueryRepository.GetByIdAsync(gamePublicId, cancellationToken);
            if (game is not null)
            {
                promotion.Games.Add(game);
            }
        }

        foreach (var userPublicId in command.UserPublicIds.Distinct())
        {
            var userId = _userContext.GetCurrentUserId();
            promotion.UserId = userId;
        }

        if (promotionExists)
            await _promotionCommandRepository.UpdateAsync(promotion, cancellationToken);
        else
            await _promotionCommandRepository.AddAsync(promotion, cancellationToken);

        var promotionOutput = promotion.ToOutput();

        return ResultData<PromotionOutput>.Success(promotionOutput);
    }
}