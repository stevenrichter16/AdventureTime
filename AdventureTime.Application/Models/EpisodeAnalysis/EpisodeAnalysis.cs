using AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

namespace AdventureTime.Application.Models.EpisodeAnalysis;

public class EpisodeAnalysis
{
    public int EpisodeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    
    // Overall sentiment
    public OverallSentiment Sentiment { get; set; } = new();
    
    // Character-specific analysis
    public Dictionary<string, CharacterMood> CharacterMoods { get; set; } = new();
    
    // Relationship dynamics
    public List<RelationshipDynamic> RelationshipDynamics { get; set; } = [];
    
    // Key themes and their emotional weight
    public List<ThemeAnalysis> Themes { get; set; } = [];
    
    // Narrative arc
    public NarrativeArc StoryArc { get; set; } = new();
    
    // Notable moments
    public List<EmotionalMoment> KeyMoments { get; set; } = [];
}