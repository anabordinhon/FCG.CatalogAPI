namespace FCG.Catalog.Application.Games.Outputs;
public record GameIndexDto(
    Guid Id,
    string Name,
    string Description,
    string Developer,
    string Genre,
    decimal Price
);