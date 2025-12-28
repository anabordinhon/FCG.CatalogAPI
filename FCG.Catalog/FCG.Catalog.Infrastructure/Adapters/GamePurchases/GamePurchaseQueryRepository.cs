using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Domain.GamePurchases.Entities;
using FCG.Catalog.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infraestructure.Adapters.GamePurchases;

public class GamePurchaseQueryRepository : IGamePurchaseQueryRepository
{
    private readonly AppDbContext _dbContext;
    public GamePurchaseQueryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<GamePurchase>> GetByUserGamePurchasesPagedAsync(int page, int pageSize, int userId, CancellationToken cancellationToken)
    {
        var totalCount = await _dbContext.GamePurchases.AsNoTracking().CountAsync(cancellationToken);

        var gamePurchase = await _dbContext.GamePurchases.AsNoTracking().Where(gp => gp.UserId == userId && gp.StatusPurchase == Domain.GamePurchases.Enum.EStatusPurchase.Approved).Include(gp => gp.Game).OrderByDescending(gp => gp.DataGamePurchase).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<GamePurchase>
        {
            Items = gamePurchase,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

    }

    public async Task<bool> AnyByUserGamePurchasesAsync(int userId, Guid gameId, CancellationToken cancellationToken)
    {
        return await _dbContext.GamePurchases
            .AsNoTracking()
            .AnyAsync(gp => gp.PublicId == gameId && gp.UserId == userId, cancellationToken);
    }
}