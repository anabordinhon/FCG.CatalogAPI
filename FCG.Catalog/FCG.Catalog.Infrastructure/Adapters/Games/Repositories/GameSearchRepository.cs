using FCG.Catalog.Application.Games.Outputs;
using FCG.Catalog.Application.Games.Ports;
using FCG.Catalog.Infrastructure.Elastic;
using Nest;

namespace FCG.Catalog.Infrastructure.Adapters.Games.Repositories;

public class GameSearchRepository : IGameSearchRepository
{
    private readonly IElasticClient _client;
    private const string Index = "fcg-games";

    public GameSearchRepository(IElasticClient client)
        => _client = client;

    public async Task IndexAsync(GameIndexDto dto, CancellationToken ct)
    {
        var document = new GameDocument
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Developer = dto.Developer,
            Genre = dto.Genre,
            Price = dto.Price,
            IndexedAt = DateTime.UtcNow
        };

        var response = await _client.IndexAsync(
            document,
            i => i.Index(Index).Id(document.Id),
            ct
        );

        if (!response.IsValid)
            throw new InvalidOperationException(
                $"Falha ao indexar jogo {dto.Id}: {response.ServerError?.Error?.Reason}");
    }

    public async Task<IReadOnlyList<GameSearchDto>> SearchAsync(
        string term, int page, int pageSize, CancellationToken ct)
    {
        var response = await _client.SearchAsync<GameDocument>(s => s
            .Index(Index)
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(f => f
                        .Field(d => d.Name, boost: 3)
                        .Field(d => d.Description, boost: 1)
                        .Field(d => d.Developer, boost: 2)
                    )
                    .Query(term)
                    .Fuzziness(Fuzziness.Auto)
                    .PrefixLength(1)
                    .MaxExpansions(50)
                    .Operator(Operator.Or)
                    .Type(TextQueryType.BestFields)
                )
            )
            .Sort(sort => sort.Descending(SortSpecialField.Score)),
            ct
        );

        if (!response.IsValid)
            throw new InvalidOperationException(
                $"Busca falhou: {response.ServerError?.Error?.Reason}");

        return response.Hits
            .Select(h => new GameSearchDto(
                h.Source.Id,
                h.Source.Name,
                h.Source.Description,
                h.Source.Developer,
                h.Source.Genre,
                h.Source.Price,
                h.Score ?? 0
            ))
            .ToList();
    }
}