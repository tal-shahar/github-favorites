using System.Text.Json;
using FavoritesAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FavoritesAPI.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<RepositoryAnalysis> RepositoryAnalyses => Set<RepositoryAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.GitHubId).IsUnique().HasFilter("\"GitHubId\" IS NOT NULL");
            builder.Property(x => x.PasswordHash).IsRequired(false);
            builder.Property(x => x.GitHubUsername).HasMaxLength(200);
            builder.Property(x => x.AvatarUrl).HasMaxLength(500);
            builder.Property(x => x.AccessToken).HasMaxLength(500);
            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now() at time zone 'utc'");
        });

        modelBuilder.Entity<Favorite>(builder =>
        {
            builder.ToTable("favorites");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RepoId)
                .HasColumnName("repo_id")
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(x => x.Owner)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(x => x.Description)
                .HasMaxLength(4000);
            builder.Property(x => x.Stars).HasColumnName("stars");
            builder.Property(x => x.RepoUpdatedAtUtc)
                .HasColumnName("repo_updated_at")
                .HasColumnType("timestamp with time zone");
            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now() at time zone 'utc'");
            builder.HasOne(x => x.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Analysis)
                .WithOne(a => a.Favorite)
                .HasForeignKey<RepositoryAnalysis>(a => a.FavoriteId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.UserId, x.RepoId }).IsUnique();
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
            builder.Property(x => x.License)
                .HasMaxLength(120);
            builder.Property(x => x.PrimaryLanguage)
                .HasColumnName("primary_language")
                .HasMaxLength(120);
            builder.Property(x => x.DefaultBranch)
                .HasColumnName("default_branch")
                .HasMaxLength(120);
            builder.Property(x => x.Topics)
                .HasColumnType("jsonb")
                .HasConversion(topicsConverter);
            builder.Property(x => x.Languages)
                .HasColumnType("jsonb")
                .HasConversion(languagesConverter);
            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now() at time zone 'utc'");
        });
    }
}

