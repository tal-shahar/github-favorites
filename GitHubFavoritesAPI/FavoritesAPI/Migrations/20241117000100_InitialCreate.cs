using System;
using FavoritesAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FavoritesAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241117000100_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    repo_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, defaultValue: string.Empty),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    repo_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_favorites_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repository_analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FavoriteId = table.Column<Guid>(type: "uuid", nullable: false),
                    License = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, defaultValue: string.Empty),
                    Topics = table.Column<string>(type: "jsonb", nullable: false),
                    Languages = table.Column<string>(type: "jsonb", nullable: false),
                    primary_language = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, defaultValue: string.Empty),
                    ReadmeLength = table.Column<int>(type: "integer", nullable: false),
                    OpenIssues = table.Column<int>(type: "integer", nullable: false),
                    Forks = table.Column<int>(type: "integer", nullable: false),
                    StarsSnapshot = table.Column<int>(type: "integer", nullable: false),
                    ActivityDays = table.Column<int>(type: "integer", nullable: false),
                    default_branch = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, defaultValue: string.Empty),
                    HealthScore = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repository_analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_repository_analyses_favorites_FavoriteId",
                        column: x => x.FavoriteId,
                        principalTable: "favorites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favorites_UserId_repo_id",
                table: "favorites",
                columns: new[] { "UserId", "repo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_repository_analyses_FavoriteId",
                table: "repository_analyses",
                column: "FavoriteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repository_analyses");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

