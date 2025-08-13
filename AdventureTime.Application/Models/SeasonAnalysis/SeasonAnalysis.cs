namespace AdventureTime.Application.Models.SeasonAnalysis;

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