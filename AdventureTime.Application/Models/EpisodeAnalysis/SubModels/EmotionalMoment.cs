namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class EmotionalMoment
{
    public string Description { get; set; } = string.Empty;
    public double ImpactScore { get; set; }
    public List<string> CharactersInvolved { get; set; } = [];
    public string EmotionType { get; set; } = string.Empty;
    public string Significance { get; set; } = string.Empty;
    public List<string> NotableQuotes { get; set; } = []; // NEW!
}