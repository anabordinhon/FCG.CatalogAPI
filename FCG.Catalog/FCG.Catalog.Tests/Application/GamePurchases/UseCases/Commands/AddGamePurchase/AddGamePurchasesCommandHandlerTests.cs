using Bogus;
using FCG.Catalog.Application.Common.Outputs;
using FCG.Catalog.Application.Common.Ports;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Application.GamePurchases.UseCases.Commands.AddGamePurchase;
using FCG.Catalog.Domain.Common.Ports;
using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.GamePurchases.Entities;
using FCG.Catalog.Domain.Games.Entities;
using FCG.Catalog.Domain.Games.Enum;
using FCG.Catalog.Domain.Games.Ports;
using FCG.Catalog.Domain.Games.ValueObjects;
using FCG.Catalog.Domain.Promotions.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Catalog.Tests.Application.GamePurchases.UseCases.Commands.AddGamePurchase
{
    public class AddGamePurchasesCommandHandlerTests
    {
        private readonly Mock<IGamePurchaseCommandRepository> _gamePurchaseCommandRepositoryMock;
        private readonly Mock<IGamePurchaseQueryRepository> _gamePurchaseQueryRepositoryMock;
        private readonly Mock<IGameQueryRepository> _gameQueryRepositoryMock;
        private readonly Mock<IPromotionService> _promotionServiceMock;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly Mock<ILogger<AddGamePurchasesCommandHandler>> _loggerMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly AddGamePurchasesCommandHandler _handler;
        private readonly Faker _faker;
        private readonly string _name;
        private readonly string _description;
        private readonly string _developer;
        private readonly decimal _priceValue;
        private readonly int _createdBy;
        private readonly int _userId;
        private readonly DateTime _baseDate;

        public AddGamePurchasesCommandHandlerTests()
        {
            _gamePurchaseCommandRepositoryMock = new Mock<IGamePurchaseCommandRepository>();
            _gamePurchaseQueryRepositoryMock = new Mock<IGamePurchaseQueryRepository>();
            _gameQueryRepositoryMock = new Mock<IGameQueryRepository>();
            _promotionServiceMock = new Mock<IPromotionService>();
            _userContextMock = new Mock<IUserContext>();
            _loggerMock = new Mock<ILogger<AddGamePurchasesCommandHandler>>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            _handler = new AddGamePurchasesCommandHandler(
                _gamePurchaseCommandRepositoryMock.Object,
                _gamePurchaseQueryRepositoryMock.Object,
                _gameQueryRepositoryMock.Object,
                _promotionServiceMock.Object,
                _userContextMock.Object,
                _loggerMock.Object,
                _eventPublisherMock.Object
            );

            _faker = new Faker("pt_BR");
            _name = _faker.Commerce.ProductName();
            _description = _faker.Commerce.ProductName();
            _developer = _faker.Company.CompanyName();
            _priceValue = _faker.Random.Decimal(50, 300);
            _createdBy = _faker.Random.Int(1, 10);
            _userId = _faker.Random.Int(1, 10);
            _baseDate = DateTime.UtcNow;
        }

        [Fact]
        public async Task Handle_ShouldCreateGamePurchase_WithCorrectFinalPrice()
        {
            // Cenário: Criar uma compra de jogo com promoção válida
            var userId = _userId;
            var gameId = Guid.NewGuid();
            var game = Game.Create(
                name: _name,
                description: _description,
                genre: GameGenreEnum.RPG,
                releaseDate: _baseDate,
                developer: _developer,
                price: Price.Create(100M),
                ageRating: AgeRating.Create("16+")
            );

            _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _gameQueryRepositoryMock.Setup(x => x.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(game);
            _gamePurchaseQueryRepositoryMock.Setup(x => x.AnyByUserGamePurchasesAsync(userId, gameId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _promotionServiceMock.Setup(x => x.GetBestDiscountAsync(game.Price, gameId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PromotionServiceResult(1, Price.Create(20)));
            _gamePurchaseCommandRepositoryMock.Setup(x => x.AddAsync(It.IsAny<GamePurchase>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GamePurchase gp, CancellationToken ct) => gp);

            var command = new AddGamePurchasesComand(gameId);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.FinalPrice.Should().Be(80); // 100 - 20
            result.Data.PromotionValue!.Value.Should().Be(20);
            _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenGameDoesNotExist()
        {
            var gameId = Guid.NewGuid();

            _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(1);
            _gameQueryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game?)null);

            var command = new AddGamePurchasesComand(gameId);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Jogo não encontrado.");
            _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenUserAlreadyOwnsGame()
        {
            var userId = _userId;
            var gameId = Guid.NewGuid();
            var game = Game.Create(
                name: _name,
                description: _description,
                genre: GameGenreEnum.RPG,
                releaseDate: _baseDate,
                developer: _developer,
                price: Price.Create(100M),
                ageRating: AgeRating.Create("16+")
            );

            _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _gameQueryRepositoryMock.Setup(x => x.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(game);
            _gamePurchaseQueryRepositoryMock.Setup(x => x.AnyByUserGamePurchasesAsync(userId, gameId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var command = new AddGamePurchasesComand(gameId);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Usuário já possui este jogo.");
            _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldApplyPromotionCorrectly()
        {
            var userId = _userId;
            var gameId = Guid.NewGuid();
            var game = Game.Create(
                name: _name,
                description: _description,
                genre: GameGenreEnum.ActionRPG,
                releaseDate: _baseDate,
                developer: _developer,
                price: Price.Create(200M),
                ageRating: AgeRating.Create("16+")
            );

            _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _gameQueryRepositoryMock.Setup(x => x.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(game);
            _gamePurchaseQueryRepositoryMock.Setup(x => x.AnyByUserGamePurchasesAsync(userId, gameId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _promotionServiceMock.Setup(x => x.GetBestDiscountAsync(game.Price, gameId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PromotionServiceResult(5, Price.Create(50)));
            _gamePurchaseCommandRepositoryMock.Setup(x => x.AddAsync(It.IsAny<GamePurchase>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GamePurchase gp, CancellationToken ct) => gp);

            var command = new AddGamePurchasesComand(gameId);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.FinalPrice.Should().Be(150); // 200 - 50
            result.Data.PromotionValue!.Value.Should().Be(50);
            _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateGamePurchase_WhenNoPromotionAvailable()
        {
            // Cenário: Criar uma compra de jogo quando não há promoção aplicada
            var userId = 1;
            var gameId = Guid.NewGuid();
            var game = Game.Create(
                name: _name,
                description: _description,
                genre: GameGenreEnum.RPG,
                releaseDate: _baseDate,
                developer: _developer,
                price: Price.Create(120M),
                ageRating: AgeRating.Create("16+")
            );

            _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _gameQueryRepositoryMock.Setup(x => x.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);
            _gamePurchaseQueryRepositoryMock.Setup(x => x.AnyByUserGamePurchasesAsync(userId, gameId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Retorna promoção zerada
            _promotionServiceMock.Setup(x => x.GetBestDiscountAsync(game.Price, gameId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PromotionServiceResult(null, Price.Create(0m)));

            _gamePurchaseCommandRepositoryMock.Setup(x => x.AddAsync(It.IsAny<GamePurchase>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GamePurchase gp, CancellationToken ct) => gp);

            var command = new AddGamePurchasesComand(gameId);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.FinalPrice.Should().Be(120); // Sem desconto
            result.Data.PromotionValue!.Value.Should().Be(0); // Promoção zerada
            _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

