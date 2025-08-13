namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class StoryBeat
{
    public string BeatType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double EmotionalIntensity { get; set; }
    public int ApproximateTimestamp { get; set; }
    public string KeyDialogue { get; set; } = string.Empty; // NEW!
}