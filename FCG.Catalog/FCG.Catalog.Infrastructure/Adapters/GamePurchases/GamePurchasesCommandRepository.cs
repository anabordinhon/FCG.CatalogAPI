using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Domain.GamePurchases.Entities;
using FCG.Catalog.Infraestructure.Persistence;

namespace FCG.Catalog.Infraestructure.Adapters.GamePurchases;

public class GamePurchasesCommandRepository : IGamePurchaseCommandRepository
{
    private readonly AppDbContext _dbContext;
    public GamePurchasesCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GamePurchase> AddAsync(GamePurchase gamePurchase, CancellationToken cancellationToken)
    {
        await _dbContext.GamePurchases.AddAsync(gamePurchase, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return gamePurchase;
    }

    public async Task<GamePurchase> UpdateAsync(GamePurchase gamePurchase, CancellationToken cancellationToken)
    {
        _dbContext.GamePurchases.Update(gamePurchase);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return gamePurchase;
    }
}