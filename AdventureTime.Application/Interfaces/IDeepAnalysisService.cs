// In Application/Common/Interfaces/IDeepAnalysisService.cs

using AdventureTime.Models;

namespace AdventureTime.Application.Interfaces;

public interface IDeepAnalysisService
{
    Task<EpisodeAnalysis> AnalyzeEpisodeAsync(Episode episode, CancellationToken cancellationToken = default);
    Task<SeasonAnalysis> AnalyzeSeasonTrendsAsync(List<EpisodeAnalysis> episodeAnalyses, CancellationToken cancellationToken = default);
    Task<CharacterDynamicsAnalysis> AnalyzeCharacterDynamicsAsync(List<Episode> episodes, string characterName, CancellationToken cancellationToken = default);
}

// Domain models for analysis results
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
    public List<RelationshipDynamic> RelationshipDynamics { get; set; } = new();
    
    // Key themes and their emotional weight
    public List<ThemeAnalysis> Themes { get; set; } = new();
    
    // Narrative arc
    public NarrativeArc StoryArc { get; set; } = new();
    
    // Notable moments
    public List<EmotionalMoment> KeyMoments { get; set; } = new();
}

public class OverallSentiment
{
    public double PositivityScore { get; set; } // 0-1
    public double IntensityScore { get; set; } // 0-1, how emotionally charged
    public double ComplexityScore { get; set; } // 0-1, emotional complexity
    public string DominantEmotion { get; set; } = "Neutral";
    public string ToneDescription { get; set; } = string.Empty;
    public List<string> EmotionalTags { get; set; } = new(); // e.g., "bittersweet", "triumphant", "melancholic"
}

public class CharacterMood
{
    public string CharacterName { get; set; } = string.Empty;
    public string OverallMood { get; set; } = string.Empty;
    public double PositivityScore { get; set; }
    public Dictionary<string, double> EmotionBreakdown { get; set; } = new(); // joy, sadness, anger, fear, etc.
    public string CharacterGrowth { get; set; } = string.Empty; // How character changed in episode
    public List<string> SignificantActions { get; set; } = new();
}

public class RelationshipDynamic
{
    public string Character1 { get; set; } = string.Empty;
    public string Character2 { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // friendship, rivalry, romantic, mentor-student
    public double HarmonyScore { get; set; } // -1 (conflict) to 1 (harmony)
    public string DynamicDescription { get; set; } = string.Empty;
    public List<string> KeyInteractions { get; set; } = new();
    public string Evolution { get; set; } = string.Empty; // How relationship changed
}

public class ThemeAnalysis
{
    public string Theme { get; set; } = string.Empty; // e.g., "friendship", "loss", "growing up"
    public double Prominence { get; set; } // 0-1, how prominent in episode
    public string EmotionalTone { get; set; } = string.Empty;
    public List<string> RelatedMoments { get; set; } = new();
}

public class NarrativeArc
{
    public string ArcType { get; set; } = string.Empty; // "hero's journey", "tragedy", "comedy", etc.
    public List<StoryBeat> StoryBeats { get; set; } = new();
    public double SatisfactionScore { get; set; } // How satisfying the resolution was
    public string EmotionalJourney { get; set; } = string.Empty; // Description of emotional progression
}

public class StoryBeat
{
    public string BeatType { get; set; } = string.Empty; // "setup", "conflict", "climax", "resolution"
    public string Description { get; set; } = string.Empty;
    public double EmotionalIntensity { get; set; }
    public int ApproximateTimestamp { get; set; } // Rough position in episode
}

public class EmotionalMoment
{
    public string Description { get; set; } = string.Empty;
    public double ImpactScore { get; set; } // 0-1
    public List<string> CharactersInvolved { get; set; } = new();
    public string EmotionType { get; set; } = string.Empty;
    public string Significance { get; set; } = string.Empty; // Why this moment matters
}

// Season-level analysis
public class SeasonAnalysis
{
    public int Season { get; set; }
    public SeasonalTrends Trends { get; set; } = new();
    public CharacterEvolution CharacterGrowth { get; set; } = new();
    public RelationshipEvolution RelationshipChanges { get; set; } = new();
    public List<SeasonalTheme> MajorThemes { get; set; } = new();
    public EmotionalTrajectory EmotionalArc { get; set; } = new();
}

public class SeasonalTrends
{
    public double AveragePositivity { get; set; }
    public double EmotionalVariance { get; set; } // How much emotions fluctuate
    public string DominantTone { get; set; } = string.Empty;
    public List<string> RecurringElements { get; set; } = new();
    public Dictionary<string, int> EmotionFrequency { get; set; } = new();
}

public class CharacterEvolution
{
    public Dictionary<string, CharacterJourney> CharacterJourneys { get; set; } = new();
}

public class CharacterJourney
{
    public string CharacterName { get; set; } = string.Empty;
    public string StartingState { get; set; } = string.Empty;
    public string EndingState { get; set; } = string.Empty;
    public List<string> KeyDevelopments { get; set; } = new();
    public double GrowthScore { get; set; } // How much the character evolved
    public string GrowthDescription { get; set; } = string.Empty;
}

public class RelationshipEvolution
{
    public List<RelationshipJourney> SignificantRelationships { get; set; } = new();
}

public class RelationshipJourney
{
    public string Relationship { get; set; } = string.Empty; // e.g., "Finn & Jake"
    public string StartingDynamic { get; set; } = string.Empty;
    public string EndingDynamic { get; set; } = string.Empty;
    public List<string> TurningPoints { get; set; } = new();
    public double StabilityScore { get; set; } // How stable vs volatile
}

public class SeasonalTheme
{
    public string Theme { get; set; } = string.Empty;
    public double Prominence { get; set; }
    public List<string> KeyEpisodes { get; set; } = new();
    public string ThematicEvolution { get; set; } = string.Empty;
}

public class EmotionalTrajectory
{
    public List<EmotionalDataPoint> DataPoints { get; set; } = new();
    public string OverallShape { get; set; } = string.Empty; // "ascending", "descending", "cyclical", etc.
    public string Description { get; set; } = string.Empty;
}

public class EmotionalDataPoint
{
    public int EpisodeNumber { get; set; }
    public double PositivityScore { get; set; }
    public double IntensityScore { get; set; }
    public string DominantEmotion { get; set; } = string.Empty;
}

// Character-specific deep dive
public class CharacterDynamicsAnalysis
{
    public string CharacterName { get; set; } = string.Empty;
    public PersonalityProfile Personality { get; set; } = new();
    public EmotionalPattern EmotionalPatterns { get; set; } = new();
    public Dictionary<string, RelationshipProfile> Relationships { get; set; } = new();
    public CharacterArc OverallArc { get; set; } = new();
    public List<DefiningMoment> DefiningMoments { get; set; } = new();
}

public class PersonalityProfile
{
    public List<string> CoreTraits { get; set; } = new();
    public Dictionary<string, double> EmotionalTendencies { get; set; } = new();
    public string MotivationDescription { get; set; } = string.Empty;
    public List<string> RecurringBehaviors { get; set; } = new();
    public string CopingMechanisms { get; set; } = string.Empty;
}

public class EmotionalPattern
{
    public Dictionary<string, double> EmotionFrequency { get; set; } = new();
    public List<string> EmotionalTriggers { get; set; } = new();
    public string EmotionalRange { get; set; } = string.Empty; // "wide", "narrow", "volatile"
    public double EmotionalMaturity { get; set; }
}

public class RelationshipProfile
{
    public string OtherCharacter { get; set; } = string.Empty;
    public string RelationshipNature { get; set; } = string.Empty;
    public double ImportanceScore { get; set; } // How important this relationship is
    public List<string> CommonInteractionPatterns { get; set; } = new();
    public string PowerDynamic { get; set; } = string.Empty; // "equal", "mentor-student", etc.
    public string ConflictResolutionStyle { get; set; } = string.Empty;
}

public class CharacterArc
{
    public string ArcDescription { get; set; } = string.Empty;
    public List<string> MajorTurningPoints { get; set; } = new();
    public string GrowthSummary { get; set; } = string.Empty;
    public double ConsistencyScore { get; set; } // How consistent vs contradictory
}

public class DefiningMoment
{
    public string EpisodeTitle { get; set; } = string.Empty;
    public string MomentDescription { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public List<string> RevealedTraits { get; set; } = new();
}