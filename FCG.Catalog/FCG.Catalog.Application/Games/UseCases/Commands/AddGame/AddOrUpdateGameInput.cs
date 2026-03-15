using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.Games.Enum;
using FCG.Catalog.Domain.Games.ValueObjects;

namespace FCG.Catalog.Application.Games.UseCases.Commands.AddGame;

public class AddOrUpdateGameInput
{
    public Guid? PublicId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Genre { get; set; }
    public required DateTime ReleaseDate { get; set; }
    public required string Developer { get; set; }
    public required decimal PriceValue { get; set; }
    public required string AgeRatingValue { get; set; }

    public AddOrUpdateGameCommand MapToCommand()
    {
        if (!Enum.TryParse(Genre, true, out GameGenreEnum genreEnum))
        {
            throw new ArgumentException("Gênero do jogo inválido.", nameof(Genre));
        }

        return new AddOrUpdateGameCommand(Name, Description, genreEnum, ReleaseDate, Developer, Price.Create(PriceValue), AgeRating.Create(AgeRatingValue), PublicId);
    }
}