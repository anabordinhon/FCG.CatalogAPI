using System.Text.Json;
using FCG.Catalog.Application.Games.Outputs;
using FCG.Catalog.Application.Games.Ports;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace FCG.Catalog.Infrastructure.Cache;

public class DistributedGameCacheService : IGameCacheService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _distributedCache;
    private readonly RedisCacheSettings _settings;

    public DistributedGameCacheService(
        IDistributedCache distributedCache,
        IOptions<RedisCacheSettings> settings)
    {
        _distributedCache = distributedCache;
        _settings = settings.Value;
    }

    public async Task<GameOutput?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var payload = await _distributedCache.GetStringAsync(GetGameKey(publicId), cancellationToken);

        return string.IsNullOrWhiteSpace(payload)
            ? null
            : JsonSerializer.Deserialize<GameOutput>(payload, JsonSerializerOptions);
    }

    public async Task SetAsync(GameOutput game, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(game, JsonSerializerOptions);

        await _distributedCache.SetStringAsync(
            GetGameKey(game.PublicId),
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.GameCacheTtlMinutes)
            },
            cancellationToken);
    }

    public static string GetGameKey(Guid publicId) => $"games:{publicId}";
}
