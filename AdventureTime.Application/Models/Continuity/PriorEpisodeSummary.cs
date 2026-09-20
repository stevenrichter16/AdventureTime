namespace AdventureTime.Application.Models.Continuity;

public record PriorEpisodeSummary(
    int EpisodeId,
    int Season,
    int EpisodeNumber,
    string Title,
    string DominantEmotion,
    double PositivityScore,
    double IntensityScore,
    double ComplexityScore,
    IReadOnlyList<string> ThemeNames,
    IReadOnlyList<RelationshipHarmony> Relationships
);