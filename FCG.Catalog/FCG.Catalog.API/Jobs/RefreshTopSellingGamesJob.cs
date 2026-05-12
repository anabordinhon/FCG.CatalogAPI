using FCG.Catalog.Application.GamePurchases.Ports;
using Quartz;

namespace FCG.Catalog.API.Jobs;

public class RefreshTopSellingGamesJob : IJob
{
    private readonly ITopSellingGamesCacheService _topSellingGamesCacheService;

    public RefreshTopSellingGamesJob(ITopSellingGamesCacheService topSellingGamesCacheService)
    {
        _topSellingGamesCacheService = topSellingGamesCacheService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _topSellingGamesCacheService.RefreshTopSellingGamesAsync(context.CancellationToken);
    }
}
