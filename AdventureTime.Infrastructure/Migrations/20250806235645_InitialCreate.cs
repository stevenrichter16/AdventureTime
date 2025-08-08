using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdventureTime.Infrastructure.Migrations
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

            migrationBuilder.CreateTable(
                name: "EpisodeAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EpisodeId = table.Column<int>(type: "integer", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentimentJson = table.Column<string>(type: "jsonb", nullable: false),
                    PositivityScore = table.Column<double>(type: "double precision", nullable: false),
                    IntensityScore = table.Column<double>(type: "double precision", nullable: false),
                    ComplexityScore = table.Column<double>(type: "double precision", nullable: false),
                    DominantEmotion = table.Column<string>(type: "text", nullable: false),
                    CharacterMoodsJson = table.Column<string>(type: "jsonb", nullable: false),
                    RelationshipDynamicsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ThemesJson = table.Column<string>(type: "jsonb", nullable: false),
                    StoryArcJson = table.Column<string>(type: "jsonb", nullable: false),
                    KeyMomentsJson = table.Column<string>(type: "jsonb", nullable: false),
                    AnalysisSource = table.Column<string>(type: "text", nullable: true),
                    AnalysisVersion = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EpisodeAnalyses_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeAnalysis_Emotion",
                table: "EpisodeAnalyses",
                column: "DominantEmotion");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeAnalysis_EpisodeId",
                table: "EpisodeAnalyses",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeAnalysis_Scores",
                table: "EpisodeAnalyses",
                columns: new[] { "IntensityScore", "PositivityScore" });

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
                name: "EpisodeAnalyses");

            migrationBuilder.DropTable(
                name: "Episodes");
        }
    }
}
