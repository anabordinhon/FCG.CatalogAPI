using FCG.Catalog.Application.Games.Outputs;

namespace FCG.Catalog.Application.Games.Ports;
public interface IGameSearchRepository
{
    Task IndexAsync(GameIndexDto dto, CancellationToken ct);
    Task<IReadOnlyList<GameSearchDto>> SearchAsync(
        string term, int page, int pageSize, CancellationToken ct);

}
