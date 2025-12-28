using FCG.Catalog.Domain.GamePurchases.Entities;
using FCG.Catalog.Domain.Games.Entities;
using FCG.Catalog.Domain.Promotions.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infraestructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }

    public DbSet<Game> Games { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<GamePurchase> GamePurchases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    }
}