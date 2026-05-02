namespace FCG.Catalog.Application.Games.Outputs;
public record GameSearchDto(
    Guid Id,
    string Name,
    string Description,
    string Developer,
    string Genre,
    decimal Price,
    double Score
);