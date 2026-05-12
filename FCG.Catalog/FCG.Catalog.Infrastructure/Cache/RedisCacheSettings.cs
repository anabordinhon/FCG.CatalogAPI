namespace FCG.Catalog.Infrastructure.Cache;

public class RedisCacheSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public int GameCacheTtlMinutes { get; set; } = 60;
    public int TopSellingCacheTtlMinutes { get; set; } = 120;
}
