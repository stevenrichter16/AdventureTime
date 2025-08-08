using System.Text.Json;
using AdventureTime.Application.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AdventureTime.Application.Interfaces;
using AdventureTime.Models;
using Microsoft.Extensions.Options;

namespace AdventureTime.Infrastructure.Services;

public class ClaudeDeepAnalysisService : IDeepAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeDeepAnalysisService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AnthropicConfig _anthropicConfig;

    public ClaudeDeepAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ClaudeDeepAnalysisService> logger,
        IOptions<AnthropicConfig> anthropicConfig)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        _anthropicConfig = anthropicConfig.Value;
    }

    public async Task<EpisodeAnalysis> AnalyzeEpisodeAsync(Episode episode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(episode.TranscriptText))
        {
            throw new InvalidOperationException($"Episode {episode.Title} has no transcript");
        }

        _logger.LogInformation("Starting deep analysis for episode: {Title}", episode.Title);

        var prompt = BuildEpisodeAnalysisPrompt(episode);
        var response = await CallClaudeAsync(prompt, cancellationToken);

        try
        {
            var analysis = JsonSerializer.Deserialize<EpisodeAnalysis>(response, _jsonOptions)
                           ?? throw new InvalidOperationException("Failed to parse analysis response");

            analysis.EpisodeId = episode.Id;
            analysis.Title = episode.Title;
            analysis.AnalysisDate = DateTime.UtcNow;

            return analysis;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Claude's response for episode {Title}", episode.Title);
            throw new InvalidOperationException($"Failed to parse analysis for {episode.Title}", ex);
        }
    }

    public async Task<SeasonAnalysis> AnalyzeSeasonTrendsAsync(
        List<EpisodeAnalysis> episodeAnalyses,
        CancellationToken cancellationToken = default)
    {
        if (!episodeAnalyses.Any())
        {
            throw new ArgumentException("No episode analyses provided");
        }

        var season = episodeAnalyses.First().Title.Contains("S01") ? 1 :
            episodeAnalyses.First().Title.Contains("S02") ? 2 : 0; // Extract season properly

        _logger.LogInformation("Analyzing season {Season} trends from {Count} episodes",
            season, episodeAnalyses.Count);

        var prompt = BuildSeasonAnalysisPrompt(episodeAnalyses, season);
        var response = await CallClaudeAsync(prompt, cancellationToken);

        var analysis = JsonSerializer.Deserialize<SeasonAnalysis>(response, _jsonOptions)
                       ?? throw new InvalidOperationException("Failed to parse season analysis");

        analysis.Season = season;
        return analysis;
    }

    public async Task<CharacterDynamicsAnalysis> AnalyzeCharacterDynamicsAsync(
        List<Episode> episodes,
        string characterName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing character dynamics for {Character} across {Count} episodes",
            characterName, episodes.Count);

        var prompt = BuildCharacterAnalysisPrompt(episodes, characterName);
        var response = await CallClaudeAsync(prompt, cancellationToken);

        var analysis = JsonSerializer.Deserialize<CharacterDynamicsAnalysis>(response, _jsonOptions)
                       ?? throw new InvalidOperationException("Failed to parse character analysis");

        analysis.CharacterName = characterName;
        return analysis;
    }

    private string BuildEpisodeAnalysisPrompt(Episode episode)
    {
        return $@"
You are analyzing Adventure Time episode '{episode.Title}' (S{episode.Season}E{episode.EpisodeNumber}).

Transcript:
{episode.TranscriptText}

Please provide a deep analysis of this episode focusing on:
1. Overall emotional sentiment and tone
2. Character moods and development
3. Relationship dynamics between characters
4. Major themes and their emotional weight
5. The narrative arc and emotional journey
6. Key emotional moments

Analyze the emotional complexity, not just positive/negative. Consider:
- Character growth and change
- Relationship evolution
- Thematic depth
- Emotional nuance (bittersweet moments, complex feelings)
- How characters cope with challenges
- Power dynamics and how they shift

Format your response as JSON matching this structure:
{{
  ""sentiment"": {{
    ""positivityScore"": 0.0-1.0,
    ""intensityScore"": 0.0-1.0,
    ""complexityScore"": 0.0-1.0,
    ""dominantEmotion"": ""string"",
    ""toneDescription"": ""brief description of overall tone"",
    ""emotionalTags"": [""tag1"", ""tag2""]
  }},
  ""characterMoods"": {{
    ""CharacterName"": {{
      ""overallMood"": ""description"",
      ""positivityScore"": 0.0-1.0,
      ""emotionBreakdown"": {{
        ""joy"": 0.0-1.0,
        ""sadness"": 0.0-1.0,
        ""anger"": 0.0-1.0,
        ""fear"": 0.0-1.0,
        ""surprise"": 0.0-1.0
      }},
      ""characterGrowth"": ""how the character changed"",
      ""significantActions"": [""action1"", ""action2""]
    }}
  }},
  ""relationshipDynamics"": [
    {{
      ""character1"": ""Name1"",
      ""character2"": ""Name2"",
      ""relationshipType"": ""friendship/rivalry/etc"",
      ""harmonyScore"": -1.0 to 1.0,
      ""dynamicDescription"": ""description"",
      ""keyInteractions"": [""interaction1""],
      ""evolution"": ""how relationship changed""
    }}
  ],
  ""themes"": [
    {{
      ""theme"": ""theme name"",
      ""prominence"": 0.0-1.0,
      ""emotionalTone"": ""tone"",
      ""relatedMoments"": [""moment1""]
    }}
  ],
  ""storyArc"": {{
    ""arcType"": ""hero's journey/tragedy/etc"",
    ""storyBeats"": [
      {{
        ""beatType"": ""setup/conflict/climax/resolution"",
        ""description"": ""what happens"",
        ""emotionalIntensity"": 0.0-1.0,
        ""approximateTimestamp"": 0-100
      }}
    ],
    ""satisfactionScore"": 0.0-1.0,
    ""emotionalJourney"": ""description of emotional progression""
  }},
  ""keyMoments"": [
    {{
      ""description"": ""what happened"",
      ""impactScore"": 0.0-1.0,
      ""charactersInvolved"": [""Name1""],
      ""emotionType"": ""emotion"",
      ""significance"": ""why it matters""
    }}
  ]
}}

Focus on Adventure Time's unique blend of humor and depth. Pay attention to subtext and implied emotions.
Respond ONLY with valid JSON, no additional text.";
    }

    private string BuildSeasonAnalysisPrompt(List<EpisodeAnalysis> analyses, int season)
    {
        var summaries = analyses.Select(a => new
        {
            a.Title,
            a.Sentiment.DominantEmotion,
            a.Sentiment.PositivityScore,
            a.Sentiment.IntensityScore,
            Themes = string.Join(", ", a.Themes.Select(t => t.Theme)),
            KeyRelationships = a.RelationshipDynamics.Select(r => $"{r.Character1}-{r.Character2}: {r.HarmonyScore:F2}")
        });

        var summaryJson = JsonSerializer.Serialize(summaries, _jsonOptions);

        return $@"
You are analyzing Season {season} of Adventure Time based on individual episode analyses.

Episode summaries:
{summaryJson}

Analyze the season-wide patterns:
1. Overall emotional trends and how they evolve
2. Character growth arcs across the season
3. How key relationships evolved
4. Major recurring themes and their development
5. The emotional trajectory of the season

Consider:
- How early episodes compare to later ones
- Character development patterns
- Relationship stability or volatility
- Thematic consistency or evolution
- Overall emotional arc of the season

Format as JSON:
{{
  ""trends"": {{
    ""averagePositivity"": 0.0-1.0,
    ""emotionalVariance"": 0.0-1.0,
    ""dominantTone"": ""description"",
    ""recurringElements"": [""element1""],
    ""emotionFrequency"": {{""emotion"": count}}
  }},
  ""characterGrowth"": {{
    ""characterJourneys"": {{
      ""CharacterName"": {{
        ""startingState"": ""description"",
        ""endingState"": ""description"",
        ""keyDevelopments"": [""development1""],
        ""growthScore"": 0.0-1.0,
        ""growthDescription"": ""how they grew""
      }}
    }}
  }},
  ""relationshipChanges"": {{
    ""significantRelationships"": [
      {{
        ""relationship"": ""Character1 & Character2"",
        ""startingDynamic"": ""description"",
        ""endingDynamic"": ""description"",
        ""turningPoints"": [""episode or event""],
        ""stabilityScore"": 0.0-1.0
      }}
    ]
  }},
  ""majorThemes"": [
    {{
      ""theme"": ""theme name"",
      ""prominence"": 0.0-1.0,
      ""keyEpisodes"": [""episode1""],
      ""thematicEvolution"": ""how theme developed""
    }}
  ],
  ""emotionalArc"": {{
    ""dataPoints"": [
      {{
        ""episodeNumber"": 1,
        ""positivityScore"": 0.0-1.0,
        ""intensityScore"": 0.0-1.0,
        ""dominantEmotion"": ""emotion""
      }}
    ],
    ""overallShape"": ""ascending/descending/cyclical/stable"",
    ""description"": ""narrative of emotional journey""
  }}
}}

Respond ONLY with valid JSON.";
    }

    private string BuildCharacterAnalysisPrompt(List<Episode> episodes, string characterName)
    {
        var relevantTranscripts = episodes
            .Where(e => e.TranscriptText?.Contains(characterName) == true)
            .Select(e => new { e.Title, Transcript = ExtractCharacterLines(e.TranscriptText, characterName) })
            .ToList();

        return $@"
You are analyzing the character '{characterName}' across multiple Adventure Time episodes.

Episodes and relevant dialogue:
{JsonSerializer.Serialize(relevantTranscripts, _jsonOptions)}

Provide a deep character analysis including:
1. Personality profile and core traits
2. Emotional patterns and tendencies
3. Key relationships and how they function
4. Character arc and development
5. Defining moments

Consider:
- How the character typically responds to challenges
- Their emotional range and maturity
- Relationship patterns and dynamics
- Growth or lack thereof
- What drives and motivates them

Format as JSON:
{{
  ""personality"": {{
    ""coreTraits"": [""trait1""],
    ""emotionalTendencies"": {{""emotion"": 0.0-1.0}},
    ""motivationDescription"": ""what drives them"",
    ""recurringBehaviors"": [""behavior1""],
    ""copingMechanisms"": ""how they handle stress""
  }},
  ""emotionalPatterns"": {{
    ""emotionFrequency"": {{""emotion"": 0.0-1.0}},
    ""emotionalTriggers"": [""trigger1""],
    ""emotionalRange"": ""wide/narrow/volatile"",
    ""emotionalMaturity"": 0.0-1.0
  }},
  ""relationships"": {{
    ""OtherCharacterName"": {{
      ""relationshipNature"": ""description"",
      ""importanceScore"": 0.0-1.0,
      ""commonInteractionPatterns"": [""pattern1""],
      ""powerDynamic"": ""equal/mentor-student/etc"",
      ""conflictResolutionStyle"": ""how they resolve conflicts""
    }}
  }},
  ""overallArc"": {{
    ""arcDescription"": ""overall journey"",
    ""majorTurningPoints"": [""event1""],
    ""growthSummary"": ""how they changed"",
    ""consistencyScore"": 0.0-1.0
  }},
  ""definingMoments"": [
    {{
      ""episodeTitle"": ""title"",
      ""momentDescription"": ""what happened"",
      ""impact"": ""how it affected them"",
      ""revealedTraits"": [""trait1""]
    }}
  ]
}}

Respond ONLY with valid JSON.";
    }

    private string ExtractCharacterLines(string transcript, string characterName)
    {
        // Extract lines where the character speaks
        var lines = transcript.Split('\n')
            .Where(line => line.StartsWith(characterName + ":", StringComparison.OrdinalIgnoreCase))
            .Take(50) // Limit to prevent prompt from being too long
            .ToList();

        return string.Join("\n", lines);
    }

    private async Task<string> CallClaudeAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = _anthropicConfig.Model,
            max_tokens = 4000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7 // Some creativity for nuanced analysis
        };
        
        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        
        // Log the request for debugging
        _logger.LogDebug("Sending request to Anthropic API: {Request}", requestJson);
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json")
        };
        
        httpRequest.Headers.Add("x-api-key", _anthropicConfig.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        
        // If the request fails, let's see what Anthropic says
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Anthropic API error: Status {Status}, Body: {Body}", 
                response.StatusCode, errorContent);
            
            throw new InvalidOperationException($"Anthropic API error: {response.StatusCode} - {errorContent}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var apiResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseContent, _jsonOptions);
        
        return apiResponse?.Content?.FirstOrDefault()?.Text 
               ?? throw new InvalidOperationException("No content in Claude's response");
    }

    private class AnthropicResponse
    {
        public List<ContentBlock>? Content { get; set; }
    }

    private class ContentBlock
    {
        public string? Text { get; set; }
    }
}