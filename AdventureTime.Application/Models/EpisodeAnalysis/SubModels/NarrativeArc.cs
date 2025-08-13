namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class NarrativeArc
{
    public string ArcType { get; set; } = string.Empty; // "hero's journey", "tragedy", "comedy", etc.
    public List<StoryBeat> StoryBeats { get; set; } = [];
    public double SatisfactionScore { get; set; } // How satisfying the resolution was
    public string EmotionalJourney { get; set; } = string.Empty; // Description of emotional progression
}