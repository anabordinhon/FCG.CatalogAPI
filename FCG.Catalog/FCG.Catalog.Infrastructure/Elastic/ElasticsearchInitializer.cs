
using Nest;

namespace FCG.Catalog.Infrastructure.Elastic;
public static class ElasticsearchInitializer
{
    public static async Task EnsureIndexAsync(IElasticClient client)
    {
        const string index = "fcg-games";
        var exists = await client.Indices.ExistsAsync(index);
        if (exists.Exists) return;

        await client.Indices.CreateAsync(index, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("fcg_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "asciifolding")
                        )
                    )
                )
            )
            .Map<GameDocument>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(d => d.Id))
                    .Text(t => t.Name(d => d.Name)
                        .Analyzer("fcg_analyzer")
                        .Fields(f => f.Keyword(k => k.Name("keyword"))))
                    .Text(t => t.Name(d => d.Description)
                        .Analyzer("fcg_analyzer"))
                    .Keyword(k => k.Name(d => d.Developer))
                    .Keyword(k => k.Name(d => d.Genre))
                    .Number(n => n.Name(d => d.Price).Type(NumberType.ScaledFloat).ScalingFactor(100))
                    .Date(d => d.Name(doc => doc.IndexedAt))
                )
            )
        );
    }
}