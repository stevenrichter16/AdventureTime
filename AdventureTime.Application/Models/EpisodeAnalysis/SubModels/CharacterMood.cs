namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class CharacterMood
{
    public string CharacterName { get; set; } = string.Empty;
    public string OverallMood { get; set; } = string.Empty;
    public double PositivityScore { get; set; }
    public Dictionary<string, double> EmotionBreakdown { get; set; } = new();
    public string CharacterGrowth { get; set; } = string.Empty;
    public List<string> SignificantActions { get; set; } = new();
    public List<string> SignatureLines { get; set; } = new(); // NEW!
}