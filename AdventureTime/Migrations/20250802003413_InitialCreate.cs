using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdventureTime.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AirDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RuntimeMinutes = table.Column<double>(type: "double precision", nullable: false),
                    FocusCharacter = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Synopsis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Plot = table.Column<string>(type: "text", nullable: true),
                    TranscriptText = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MajorCharacters = table.Column<string>(type: "jsonb", nullable: true),
                    MinorCharacters = table.Column<string>(type: "jsonb", nullable: true),
                    Locations = table.Column<string>(type: "jsonb", nullable: true),
                    DialogueLineCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episode_Season_Number",
                table: "Episodes",
                columns: new[] { "Season", "EpisodeNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Episodes");
        }
    }
}
