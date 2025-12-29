using FCG.Catalog.Application.Events;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Domain.Common.Ports;
using FCG.Catalog.Domain.GamePurchases.Enum;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Consumers;

public class PaymentProcessedConsumer : IConsumer
{
    private readonly IGamePurchaseQueryRepository _gamePurchaseQueryRepository;
    private readonly IGamePurchaseCommandRepository _gamePurchaseCommandRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<PaymentProcessedEvent> _logger;

    public PaymentProcessedConsumer(
        IGamePurchaseQueryRepository gamePurchaseQueryRepository,
        IGamePurchaseCommandRepository gamePurchaseCommandRepository,
        IUserContext userContext,
        ILogger<PaymentProcessedEvent> logger)
    {
        _gamePurchaseQueryRepository = gamePurchaseQueryRepository;
        _gamePurchaseCommandRepository = gamePurchaseCommandRepository;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context, CancellationToken cancellationToken)
    {
        var payment = context.Message;

        _logger.LogInformation(
            "PaymentProcessedEvent recebido - OrderId: {OrderId}, Status: {Status}",
            payment.OrderId, payment.Status);

        var userId = _userContext.GetCurrentUserId();
        var gamePurchase = await _gamePurchaseQueryRepository.GetByUserGamePurchasesAsync(userId, context.Message.GameId, cancellationToken);


        if (gamePurchase == null)
        {
            _logger.LogError(
                "GamePurchase não encontrada para OrderId: {OrderId}",
                payment.OrderId);
            return;
        }

        if (gamePurchase.StatusPurchase != EStatusPurchase.InProgress)
        {
            _logger.LogWarning(
                "GamePurchase {OrderId} já processada. Status atual: {Status}",
                payment.OrderId, gamePurchase.StatusPurchase);
            return;
        }

        Enum.TryParse<EStatusPurchase>(payment.Status, out var statusEnum);

        gamePurchase.UpdateStatus(statusEnum);
        await _gamePurchaseCommandRepository
            .UpdateAsync(gamePurchase, context.CancellationToken);

        if (statusEnum == EStatusPurchase.Approved)
        {
            _logger.LogInformation(
                "Compra aprovada - OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                payment.OrderId, payment.UserId, payment.GameId);
        }
        else if (statusEnum == EStatusPurchase.Rejected)
        {
            _logger.LogInformation(
                "Compra rejeitada - OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                payment.OrderId, payment.UserId, payment.GameId);
        }
    }
}