using FCG.Catalog.Application.Games.Outputs;

namespace FCG.Catalog.Application.GamePurchases.Outputs;

public record TopSellingGameOutput(GameOutput Game, int TotalSales);
