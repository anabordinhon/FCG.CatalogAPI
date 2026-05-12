using System.Text.Json;
using FCG.Catalog.Application.GamePurchases.Outputs;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Application.Games.Mappers;
using FCG.Catalog.Application.Games.Ports;
using FCG.Catalog.Domain.GamePurchases.Enum;
using FCG.Catalog.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace FCG.Catalog.Infrastructure.Cache;

public class TopSellingGamesCacheService : ITopSellingGamesCacheService
{
    private const string TopSellingGamesKey = "games:top-sellers";
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _dbContext;
    private readonly IDistributedCache _distributedCache;
    private readonly IGameCacheService _gameCacheService;
    private readonly RedisCacheSettings _settings;

    public TopSellingGamesCacheService(
        AppDbContext dbContext,
        IDistributedCache distributedCache,
        IGameCacheService gameCacheService,
        IOptions<RedisCacheSettings> settings)
    {
        _dbContext = dbContext;
        _distributedCache = distributedCache;
        _gameCacheService = gameCacheService;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyCollection<TopSellingGameOutput>> RefreshTopSellingGamesAsync(CancellationToken cancellationToken)
    {
        var topSellingGames = await _dbContext.GamePurchases
            .AsNoTracking()
            .Where(gamePurchase => gamePurchase.StatusPurchase == EStatusPurchase.Approved)
            .GroupBy(gamePurchase => gamePurchase.GameId)
            .Select(group => new
            {
                GameId = group.Key,
                TotalSales = group.Count()
            })
            .OrderByDescending(group => group.TotalSales)
            .Take(20)
            .Join(
                _dbContext.Games.AsNoTracking(),
                purchase => purchase.GameId,
                game => game.Id,
                (purchase, game) => new TopSellingGameOutput(game.ToOutput(), purchase.TotalSales))
            .ToListAsync(cancellationToken);

        foreach (var topSellingGame in topSellingGames)
        {
            await _gameCacheService.SetAsync(topSellingGame.Game, cancellationToken);
        }

        var payload = JsonSerializer.Serialize(topSellingGames, JsonSerializerOptions);

        await _distributedCache.SetStringAsync(
            TopSellingGamesKey,
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.TopSellingCacheTtlMinutes)
            },
            cancellationToken);

        return topSellingGames;
    }
}
