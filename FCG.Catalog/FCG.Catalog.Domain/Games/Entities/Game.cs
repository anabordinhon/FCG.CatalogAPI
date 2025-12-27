using FCG.Catalog.Domain.Common.Entities;
using FCG.Catalog.Domain.Common.ValueObjects;
using FCG.Catalog.Domain.Games.Enum;
using FCG.Catalog.Domain.Games.ValueObjects;
using FCG.Catalog.Domain.Promotions.Entities;

namespace FCG.Catalog.Domain.Games.Entities;

public class Game : BaseEntity
{

    private Game(string name, string description, GameGenreEnum genre, DateTime releaseDate, string developer, Price price, AgeRating ageRating)
    {
        Name = name;
        Description = description;
        Genre = genre;
        ReleaseDate = releaseDate;
        Developer = developer;
        Price = price;
        AgeRating = ageRating;
    }

    private Game() { }
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public GameGenreEnum Genre { get; private set; } = default!;
    public DateTime ReleaseDate { get; private set; }
    public string Developer { get; private set; } = default!;
    public Price Price { get; private set; } = default!;
    public AgeRating AgeRating { get; private set; } = default!;
    public ICollection<Promotion> Promotions { get; set; } = [];

    public static Game Create(string name, string description, GameGenreEnum genre, DateTime releaseDate, string developer, Price price, AgeRating ageRating)
    {
        Game game = new Game(name, description, genre, releaseDate, developer, price, ageRating);
        return game;
    }

    public void UpdateDetails(string name, string description, GameGenreEnum genre, DateTime releaseDate, string developer, Price price, AgeRating ageRating)
    {
        Name = name;
        Description = description;
        Genre = genre;
        ReleaseDate = releaseDate;
        Developer = developer;
        Price = price;
        AgeRating = ageRating;
    }

}