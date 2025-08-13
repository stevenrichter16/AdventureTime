namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class ThemeAnalysis
{
    public string Theme { get; set; } = string.Empty; // e.g., "friendship", "loss", "growing up"
    public double Prominence { get; set; } // 0-1, how prominent in episode
    public string EmotionalTone { get; set; } = string.Empty;
    public List<string> RelatedMoments { get; set; } = [];
}