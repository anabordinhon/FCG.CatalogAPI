using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.GamePurchases.Outputs;

namespace FCG.Catalog.Application.GamePurchases.UseCases.Commands.AddGamePurchase;

public interface IAddGamePurchasesCommandHandler
{
    Task<ResultData<GamePurchaseOutput>> Handle(AddGamePurchasesComand command, CancellationToken cancellationToken);
}