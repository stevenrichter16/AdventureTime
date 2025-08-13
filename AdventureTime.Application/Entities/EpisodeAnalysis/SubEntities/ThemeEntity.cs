namespace AdventureTime.Application.Entities.EpisodeAnalysis.SubEntities;

public class ThemeEntity
{
    public int Id { get; set; }
    public int EpisodeAnalysisId { get; set; }
    public EpisodeAnalysisEntity? EpisodeAnalysis { get; set; }
    public string Theme { get; set; } = string.Empty;
    public double Prominence { get; set; }
    public string EmotionalTone { get; set; } = string.Empty;
}