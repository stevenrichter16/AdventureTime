namespace AdventureTime.Application.Models.CharacterAnalysis;

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