using FCG.Catalog.Application.Common;
using FCG.Catalog.Domain.Promotions.Entities;
using FCG.Catalog.Domain.Promotions.Ports;
using FCG.Catalog.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infraestructure.Adapters.Promotions.Repositories;

public class PromotionQueryRepository : IPromotionQueryRepository
{
    private readonly AppDbContext _dbContext;
    public PromotionQueryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Promotion> GetByIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Promotions
            .AsNoTracking()
            .FirstAsync(g => g.PublicId == publicId, cancellationToken);
    }

    public async Task<PagedResult<Promotion>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await _dbContext.Promotions.AsNoTracking().CountAsync(cancellationToken);
        var promotions = await _dbContext.Promotions
            .AsNoTracking()
            .OrderBy(g => g.Description)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Promotion>
        {
            Items = promotions,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<Promotion>> GetPromotionByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
    {
        return await _dbContext.Promotions
            .AsNoTracking()
            .Where(p => p.GameId == gameId)
            .ToListAsync(cancellationToken);
    }
    public async Task<IEnumerable<Promotion>> GetPromotionByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Promotions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}