using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.Games.Enum;
using FCG.Catalog.Domain.Games.ValueObjects;

namespace FCG.Catalog.Application.Games.UseCases.Commands.AddGame;

public record AddOrUpdateGameCommand(
    string Name,
    string Description,
    GameGenreEnum Genre,
    DateTime ReleaseDate,
    string Developer,
    Price Price,
    AgeRating AgeRating,
    Guid? PublicId
);