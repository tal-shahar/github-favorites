using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RepositoryAnalysisWorker.Entities;

namespace RepositoryAnalysisWorker.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RepositoryAnalysis> RepositoryAnalyses => Set<RepositoryAnalysis>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Favorite>(builder =>
        {
            builder.ToTable("favorites");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RepoId).HasColumnName("repo_id");
            builder.Property(x => x.RepoUpdatedAtUtc).HasColumnName("repo_updated_at");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at");
        });

        var topicsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new List<string>());

        var languagesConverter = new ValueConverter<Dictionary<string, long>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            v => JsonSerializer.Deserialize<Dictionary<string, long>>(v, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Dictionary<string, long>());

        modelBuilder.Entity<RepositoryAnalysis>(builder =>
        {
            builder.ToTable("repository_analyses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DefaultBranch).HasColumnName("default_branch");
            builder.Property(x => x.PrimaryLanguage).HasColumnName("primary_language");
            builder.Property(x => x.Topics)
                .HasColumnType("jsonb")
                .HasConversion(topicsConverter);
            builder.Property(x => x.Languages)
                .HasColumnType("jsonb")
                .HasConversion(languagesConverter);
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at");

            builder.HasOne(x => x.Favorite)
                .WithOne(f => f.Analysis)
                .HasForeignKey<RepositoryAnalysis>(x => x.FavoriteId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

