using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Events;
using FCG.Catalog.Application.GamePurchases.Mappers;
using FCG.Catalog.Application.GamePurchases.Outputs;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Domain.Common.Ports;
using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.GamePurchases.Entities;
using FCG.Catalog.Domain.GamePurchases.Enum;
using FCG.Catalog.Domain.Games.Entities;
using FCG.Catalog.Domain.Games.Ports;
using FCG.Catalog.Domain.Promotions.Ports;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.GamePurchases.UseCases.Commands.AddGamePurchase;

public class AddGamePurchasesCommandHandler : IAddGamePurchasesCommandHandler
{
    private readonly IGamePurchaseCommandRepository _gamePurchaseCommandRepository;
    private readonly IGameQueryRepository _gameQueryRepository;
    private readonly IPromotionService _promotionService;
    private readonly IUserContext _userContext;
    private readonly IGamePurchaseQueryRepository _gamePurchaseQueryRepository;
    private readonly ILogger<AddGamePurchasesCommandHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public AddGamePurchasesCommandHandler(
        IGamePurchaseCommandRepository gamePurchaseCommandRepository,
        IGamePurchaseQueryRepository gamePurchaseQueryRepository,
        IGameQueryRepository gameQueryRepository,
        IPromotionService promotionService,
        IUserContext userContext,
        ILogger<AddGamePurchasesCommandHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _gamePurchaseCommandRepository = gamePurchaseCommandRepository;
        _gamePurchaseQueryRepository = gamePurchaseQueryRepository;
        _gameQueryRepository = gameQueryRepository;
        _promotionService = promotionService;
        _userContext = userContext;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }
    public async Task<ResultData<GamePurchaseOutput>> Handle(AddGamePurchasesComand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        var game = await _gameQueryRepository.GetByIdAsync(command.GameId, cancellationToken);

        if (game == null)
        {
            _logger.LogWarning("Jogo com ID {GameId} não encontrado para o usuário {UserId}.", command.GameId, userId);
            return ResultData<GamePurchaseOutput>.Error("Jogo não encontrado.");
        }

        var alreadyOwns = await _gamePurchaseQueryRepository.AnyByUserGamePurchasesAsync(userId, command.GameId, cancellationToken);

        if (alreadyOwns)
        {
            _logger.LogWarning("Usuário {UserId} já possui o jogo com ID {GameId}.", userId, command.GameId);
            return ResultData<GamePurchaseOutput>.Error("Usuário já possui este jogo.");
        }

        var bestPromotion = await _promotionService.GetBestDiscountAsync(game.Price, command.GameId, userId, cancellationToken);

        var finalPrice = game.Price.Value - bestPromotion.DiscountValue.Value;

        var gamePurchase = GamePurchase.Create(userId, game.Id, Price.Create(finalPrice), Price.Create(bestPromotion.DiscountValue.Value), EStatusPurchase.InProgress, bestPromotion.PromotionId);

        await _gamePurchaseCommandRepository.AddAsync(gamePurchase, cancellationToken);

        var gamePurchaseOutput = gamePurchase.ToOutput(game);

        var orderPlacedEvent = new OrderPlacedEvent
        {
            OrderId = gamePurchase.PublicId,
            UserId = userId,
            GameId = command.GameId,
            Price = finalPrice,
            CreatedAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(orderPlacedEvent);

        _logger.LogInformation(
            "✅ OrderPlacedEvent publicado - OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Price: {Price}, Status: {Status}",
            orderPlacedEvent.OrderId, gamePurchase.UserId, gamePurchase.GameId, gamePurchase.FinalPrice, gamePurchase.StatusPurchase);

        return ResultData<GamePurchaseOutput>.Success(gamePurchaseOutput);
    }
}