using System.ComponentModel.DataAnnotations;
using AdventureTime.Application.Interfaces;
using AdventureTime.Models;

namespace AdventureTime.Application.Models;

/// <summary>
/// Entity for storing episode analyses in the database
/// </summary>
public class EpisodeAnalysisEntity
{
    public int Id { get; set; }
    
    [Required]
    public int EpisodeId { get; set; }
    
    public Episode? Episode { get; set; } // Navigation property
    
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    
    // Overall sentiment stored as JSON
    public string SentimentJson { get; set; } = string.Empty;
    
    // Quick access fields for querying
    public double PositivityScore { get; set; }
    public double IntensityScore { get; set; }
    public double ComplexityScore { get; set; }
    public string DominantEmotion { get; set; } = string.Empty;
    
    // Store complex data as JSON
    public string CharacterMoodsJson { get; set; } = string.Empty;
    public string RelationshipDynamicsJson { get; set; } = string.Empty;
    public string ThemesJson { get; set; } = string.Empty;
    public string StoryArcJson { get; set; } = string.Empty;
    public string KeyMomentsJson { get; set; } = string.Empty;
    
    // Metadata
    public string? AnalysisSource { get; set; } // "Claude", "Manual", "Import", etc.
    public string? AnalysisVersion { get; set; } // Track which prompt/model version
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Convert to/from domain model
    public EpisodeAnalysis ToDomainModel()
    {
        return new EpisodeAnalysis
        {
            EpisodeId = EpisodeId,
            Title = Episode?.Title ?? string.Empty,
            AnalysisDate = AnalysisDate,
            Sentiment = string.IsNullOrEmpty(SentimentJson) 
                ? new OverallSentiment() 
                : System.Text.Json.JsonSerializer.Deserialize<OverallSentiment>(SentimentJson) ?? new OverallSentiment(),
            CharacterMoods = string.IsNullOrEmpty(CharacterMoodsJson)
                ? new Dictionary<string, CharacterMood>()
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, CharacterMood>>(CharacterMoodsJson) ?? new Dictionary<string, CharacterMood>(),
            RelationshipDynamics = string.IsNullOrEmpty(RelationshipDynamicsJson)
                ? new List<RelationshipDynamic>()
                : System.Text.Json.JsonSerializer.Deserialize<List<RelationshipDynamic>>(RelationshipDynamicsJson) ?? new List<RelationshipDynamic>(),
            Themes = string.IsNullOrEmpty(ThemesJson)
                ? new List<ThemeAnalysis>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ThemeAnalysis>>(ThemesJson) ?? new List<ThemeAnalysis>(),
            StoryArc = string.IsNullOrEmpty(StoryArcJson)
                ? new NarrativeArc()
                : System.Text.Json.JsonSerializer.Deserialize<NarrativeArc>(StoryArcJson) ?? new NarrativeArc(),
            KeyMoments = string.IsNullOrEmpty(KeyMomentsJson)
                ? new List<EmotionalMoment>()
                : System.Text.Json.JsonSerializer.Deserialize<List<EmotionalMoment>>(KeyMomentsJson) ?? new List<EmotionalMoment>()
        };
    }
    
    public static EpisodeAnalysisEntity FromDomainModel(EpisodeAnalysis analysis, string? source = null, string? version = null)
    {
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        
        return new EpisodeAnalysisEntity
        {
            EpisodeId = analysis.EpisodeId,
            AnalysisDate = analysis.AnalysisDate,
            SentimentJson = System.Text.Json.JsonSerializer.Serialize(analysis.Sentiment, jsonOptions),
            PositivityScore = analysis.Sentiment.PositivityScore,
            IntensityScore = analysis.Sentiment.IntensityScore,
            ComplexityScore = analysis.Sentiment.ComplexityScore,
            DominantEmotion = analysis.Sentiment.DominantEmotion,
            CharacterMoodsJson = System.Text.Json.JsonSerializer.Serialize(analysis.CharacterMoods, jsonOptions),
            RelationshipDynamicsJson = System.Text.Json.JsonSerializer.Serialize(analysis.RelationshipDynamics, jsonOptions),
            ThemesJson = System.Text.Json.JsonSerializer.Serialize(analysis.Themes, jsonOptions),
            StoryArcJson = System.Text.Json.JsonSerializer.Serialize(analysis.StoryArc, jsonOptions),
            KeyMomentsJson = System.Text.Json.JsonSerializer.Serialize(analysis.KeyMoments, jsonOptions),
            AnalysisSource = source,
            AnalysisVersion = version,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Additional entities for normalized data if you want better querying
public class CharacterMoodEntity
{
    public int Id { get; set; }
    public int EpisodeAnalysisId { get; set; }
    public EpisodeAnalysisEntity? EpisodeAnalysis { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string OverallMood { get; set; } = string.Empty;
    public double PositivityScore { get; set; }
    public double JoyScore { get; set; }
    public double SadnessScore { get; set; }
    public double AngerScore { get; set; }
    public double FearScore { get; set; }
    public double SurpriseScore { get; set; }
}

public class RelationshipDynamicEntity
{
    public int Id { get; set; }
    public int EpisodeAnalysisId { get; set; }
    public EpisodeAnalysisEntity? EpisodeAnalysis { get; set; }
    public string Character1 { get; set; } = string.Empty;
    public string Character2 { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public double HarmonyScore { get; set; }
    public string DynamicDescription { get; set; } = string.Empty;
}

public class ThemeEntity
{
    public int Id { get; set; }
    public int EpisodeAnalysisId { get; set; }
    public EpisodeAnalysisEntity? EpisodeAnalysis { get; set; }
    public string Theme { get; set; } = string.Empty;
    public double Prominence { get; set; }
    public string EmotionalTone { get; set; } = string.Empty;
}