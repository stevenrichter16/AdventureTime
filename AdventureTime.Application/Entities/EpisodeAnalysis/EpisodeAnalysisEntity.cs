using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AdventureTime.Application.Models;
using AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

namespace AdventureTime.Application.Entities.EpisodeAnalysis;

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
    public Models.EpisodeAnalysis.EpisodeAnalysis ToDomainModel()
    {
       var analysis = new Models.EpisodeAnalysis.EpisodeAnalysis
        {
            EpisodeId = EpisodeId,
            Title = Episode?.Title ?? string.Empty,
            AnalysisDate = AnalysisDate,
            Sentiment = string.IsNullOrEmpty(SentimentJson) 
                ? new OverallSentiment() 
                : System.Text.Json.JsonSerializer.Deserialize<OverallSentiment>(SentimentJson, JsonOptions) ?? new OverallSentiment(),
            CharacterMoods = string.IsNullOrEmpty(CharacterMoodsJson)
                ? new Dictionary<string, CharacterMood>()
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, CharacterMood>>(CharacterMoodsJson, JsonOptions) ?? new Dictionary<string, CharacterMood>(),
            RelationshipDynamics = string.IsNullOrEmpty(RelationshipDynamicsJson)
                ? new List<RelationshipDynamic>()
                : System.Text.Json.JsonSerializer.Deserialize<List<RelationshipDynamic>>(RelationshipDynamicsJson, JsonOptions) ?? new List<RelationshipDynamic>(),
            Themes = string.IsNullOrEmpty(ThemesJson)
                ? new List<ThemeAnalysis>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ThemeAnalysis>>(ThemesJson, JsonOptions) ?? new List<ThemeAnalysis>(),
            StoryArc = string.IsNullOrEmpty(StoryArcJson)
                ? new NarrativeArc()
                : System.Text.Json.JsonSerializer.Deserialize<NarrativeArc>(StoryArcJson, JsonOptions) ?? new NarrativeArc(),
            KeyMoments = string.IsNullOrEmpty(KeyMomentsJson)
                ? new List<EmotionalMoment>()
                : System.Text.Json.JsonSerializer.Deserialize<List<EmotionalMoment>>(KeyMomentsJson, JsonOptions) ?? new List<EmotionalMoment>()
        };
        return analysis;
    }
    
    public static EpisodeAnalysisEntity FromDomainModel(Models.EpisodeAnalysis.EpisodeAnalysis analysis, string? source = null, string? version = null)
    {
        return new EpisodeAnalysisEntity
        {
            EpisodeId = analysis.EpisodeId,
            AnalysisDate = analysis.AnalysisDate,
            SentimentJson = System.Text.Json.JsonSerializer.Serialize(analysis.Sentiment, JsonOptions),
            PositivityScore = analysis.Sentiment.PositivityScore,
            IntensityScore = analysis.Sentiment.IntensityScore,
            ComplexityScore = analysis.Sentiment.ComplexityScore,
            DominantEmotion = analysis.Sentiment.DominantEmotion,
            CharacterMoodsJson = System.Text.Json.JsonSerializer.Serialize(analysis.CharacterMoods, JsonOptions),
            RelationshipDynamicsJson = System.Text.Json.JsonSerializer.Serialize(analysis.RelationshipDynamics, JsonOptions),
            ThemesJson = System.Text.Json.JsonSerializer.Serialize(analysis.Themes, JsonOptions),
            StoryArcJson = System.Text.Json.JsonSerializer.Serialize(analysis.StoryArc, JsonOptions),
            KeyMomentsJson = System.Text.Json.JsonSerializer.Serialize(analysis.KeyMoments, JsonOptions),
            AnalysisSource = source,
            AnalysisVersion = version,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false // Keep JSON compact in DB
    };
}


