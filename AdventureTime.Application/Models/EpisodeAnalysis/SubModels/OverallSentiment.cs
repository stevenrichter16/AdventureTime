namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class OverallSentiment
{
    public double PositivityScore { get; set; } // 0-1
    public double IntensityScore { get; set; } // 0-1, how emotionally charged
    public double ComplexityScore { get; set; } // 0-1, emotional complexity
    public string DominantEmotion { get; set; } = "Neutral";
    public string ToneDescription { get; set; } = string.Empty;
    public List<string> EmotionalTags { get; set; } = new(); // e.g., "bittersweet", "triumphant", "melancholic"
}