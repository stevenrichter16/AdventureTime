using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AdventureTime.Application.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models;
using AdventureTime.Application.Models.CharacterAnalysis;
using AdventureTime.Application.Models.EpisodeAnalysis;
using AdventureTime.Application.Models.SeasonAnalysis;
using Microsoft.Extensions.Options;

namespace AdventureTime.Infrastructure.Services;

public class Gpt5DeepAnalysisService : IDeepAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Gpt5DeepAnalysisService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly OpenAiConfig _openAIConfig;

    public Gpt5DeepAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<Gpt5DeepAnalysisService> logger,
        IOptions<OpenAiConfig> openAiConfig)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        _openAIConfig = openAiConfig.Value;
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
        var response = await CallGpt5Async(prompt, cancellationToken);
        
        // Sanitize the response before attempting to deserialize
        response = SanitizeJsonResponse(response);

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
            _logger.LogError(ex, "Failed to parse GPT-5's response for episode {Title}", episode.Title);
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
            episodeAnalyses.First().Title.Contains("S02") ? 2 : 0;

        _logger.LogInformation("Analyzing season {Season} trends from {Count} episodes",
            season, episodeAnalyses.Count);

        var prompt = BuildSeasonAnalysisPrompt(episodeAnalyses, season);
        var response = await CallGpt5Async(prompt, cancellationToken);
        
        // Sanitize the response
        response = SanitizeJsonResponse(response);

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
        var response = await CallGpt5Async(prompt, cancellationToken);
        
        // Sanitize the response
        response = SanitizeJsonResponse(response);

        var analysis = JsonSerializer.Deserialize<CharacterDynamicsAnalysis>(response, _jsonOptions)
                       ?? throw new InvalidOperationException("Failed to parse character analysis");

        analysis.CharacterName = characterName;
        return analysis;
    }

    private string SanitizeJsonResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return response;
        
        // Trim whitespace first
        response = response.Trim();
        
        // Remove ```json from the beginning if it exists
        if (response.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            response = response.Substring(7); // Remove "```json"
        }
        else if (response.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            response = response.Substring(3); // Remove just "```"
        }
        
        // Remove ``` from the end if it exists
        if (response.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            response = response.Substring(0, response.Length - 3);
        }
        
        // Trim any remaining whitespace
        return response.Trim();
    }

    private async Task<string> CallGpt5Async(string prompt, CancellationToken cancellationToken)
    {
        var body = new {
            model = "gpt-5",                     // or gpt-5-mini / gpt-5-nano
            input = new[] {
                new { role = "system", content = "You are a clear, down-to-earth analyst of Adventure Time episodes.\nWrite like you're explaining to a smart friend. Use everyday words. Prefer short, direct sentences.\nAvoid stacked hyphenated phrases and flowery language. No metaphors unless necessary.\nKeep quotes exact, but keep descriptions simple. Output valid JSON only (no markdown)." },
                new { role = "user",   content = prompt }
            },
            max_output_tokens = 16000,
        };

        var requestJson = JsonSerializer.Serialize(body);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses") {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAIConfig.ApiKey);
        var response = await _httpClient.SendAsync(httpRequest);
        
        // If the request fails, let's see what OpenAI says
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI API error: Status {Status}, Body: {Body}", 
                response.StatusCode, errorContent);
            
            throw new InvalidOperationException($"OpenAI API error: {response.StatusCode} - {errorContent}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

// Deserialize as a Responses API envelope
        var env = JsonSerializer.Deserialize<ResponsesEnvelope>(responseContent, _jsonOptions)
                  ?? throw new InvalidOperationException("Failed to parse Responses API envelope.");

        return ExtractAssistantText(env);
    }

    private string BuildEpisodeAnalysisPrompt(Episode episode)
{
    return $@"
You are analyzing Adventure Time episode '{episode.Title}' (S{episode.Season}E{episode.EpisodeNumber}).

Transcript:
{episode.TranscriptText}

Provide a deep, plain-language analysis. Keep it comprehensive but down-to-earth.

NUANCE / SUBTEXT CHECKLIST (apply throughout):
- Distinguish **genuine conflict** vs **performed/bit** conflict. If a character admits they're acting or signals it's a gag, mark it as [performative].
- Flag **vulnerability-as-leadership** when someone leads by revealing honest feelings (e.g., a confessional song). Mark as [vulnerable-leadership].
- Spot **humor as deflection** (jokes used to dodge feelings). Mark as [deflection] and name the avoided feeling.
- Note **denial → desire** patterns (e.g., ""No, I didn't!"" but behavior says otherwise). Mark as [denial→desire].
- Call out **power moves** (control, gatekeeping, tests) vs **repair moves** (apology, inclusion, softening).
- Prefer concrete evidence. Use **exact quotes** only from the transcript (no invention). Put supporting quotes in the designated quote arrays, not in other fields.

INTERPRETATION RULES:
- If a conflict is performative, treat intensity as lower and adjust relationship harmony accordingly.
- If vulnerable leadership occurs, emphasize it in the leader’s characterGrowth and in the story’s climactic beat.
- Separate **technical skill** from **authenticity**; if authenticity solves the trial, say so.
- Keep tone simple: short, direct sentences. Avoid flowery language and stacked hyphenated phrases.

Focus areas:
1) Overall emotional sentiment and tone
2) Character moods and development (INCLUDE most impactful dialogue)
3) Relationship dynamics
4) Major themes and their emotional weight
5) Narrative arc and emotional journey (INCLUDE key dialogue at beats)
6) Key emotional moments (INCLUDE quotes that make them impactful)

IMPORTANT QUOTES POLICY:
- For each character’s signatureLines, storyArc.storyBeats[].keyDialogue, and keyMoments[].notableQuotes, use **verbatim** lines from the transcript.

Mark nuance inline within existing string fields when relevant using bracket tags at the start of the sentence, e.g.:
- description: ""[performative] Jake storms off but later admits he was pretending.""
- characterGrowth: ""[vulnerable-leadership] Finn leads by singing honest feelings about PB.""

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
      ""overallMood"": ""description (use bracket tags when relevant)"",
      ""positivityScore"": 0.0-1.0,
      ""emotionBreakdown"": {{
        ""joy"": 0.0-1.0,
        ""sadness"": 0.0-1.0,
        ""anger"": 0.0-1.0,
        ""fear"": 0.0-1.0,
        ""surprise"": 0.0-1.0
      }},
      ""characterGrowth"": ""how the character changed (add [vulnerable-leadership]/[deflection]/etc. if applicable)"",
      ""significantActions"": [""action1"", ""action2""],
      ""signatureLines"": [""exact quote 1"", ""exact quote 2"", ""exact quote 3""]
    }}
  }},
  ""relationshipDynamics"": [
    {{
      ""character1"": ""Name1"",
      ""character2"": ""Name2"",
      ""relationshipType"": ""friendship/rivalry/etc"",
      ""harmonyScore"": -1.0 to 1.0,
      ""dynamicDescription"": ""description (mark [performative] or [genuine] for conflicts; note repair moves)"",
      ""keyInteractions"": [""interaction1 (can reference exact quotes listed below)""],
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
        ""description"": ""what happens (mark [performative] or [vulnerable-leadership] if relevant)"",
        ""emotionalIntensity"": 0.0-1.0,
        ""approximateTimestamp"": 0-100,
        ""keyDialogue"": ""the exact quote that defines this story beat""
      }}
    ],
    ""satisfactionScore"": 0.0-1.0,
    ""emotionalJourney"": ""description of emotional progression (concrete; avoid flourish)""
  }},
  ""keyMoments"": [
    {{
      ""description"": ""what happened (add bracket tag if relevant)"",
      ""impactScore"": 0.0-1.0,
      ""charactersInvolved"": [""Name1""],
      ""emotionType"": ""emotion"",
      ""significance"": ""why it matters (name the subtext explicitly)"",
      ""notableQuotes"": [""exact quote 1"", ""exact quote 2""]
    }}
  ]
}}

Respond ONLY with valid JSON, no additional text or markdown formatting.";
}

    // Keep your existing prompt building methods exactly the same
    private string BuildEpisodeAnalysisPrompt2(Episode episode)
    {
        return $@"
You are analyzing Adventure Time episode '{episode.Title}' (S{episode.Season}E{episode.EpisodeNumber}).

Transcript:
{episode.TranscriptText}

Please provide a deep analysis of this episode focusing on:
1. Overall emotional sentiment and tone
2. Character moods and development (INCLUDING their most impactful dialogue)
3. Relationship dynamics between characters
4. Major themes and their emotional weight
5. The narrative arc and emotional journey (INCLUDING key dialogue at story beats)
6. Key emotional moments (INCLUDING the specific quotes that make them impactful)

Analyze the emotional complexity, not just positive/negative. Consider:
- Character growth and change
- Relationship evolution
- Thematic depth
- Emotional nuance (bittersweet moments, complex feelings)
- How characters cope with challenges
- Power dynamics and how they shift
- The specific dialogue that reveals character

IMPORTANT: Extract actual dialogue/quotes from the transcript for:
- signatureLines: The most characteristic, memorable, or emotionally revealing lines for each character
- keyDialogue: The specific dialogue that defines each story beat
- notableQuotes: The exact quotes that make emotional moments impactful

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
      ""significantActions"": [""action1"", ""action2""],
      ""signatureLines"": [""exact quote 1"", ""exact quote 2"", ""exact quote 3""]
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
        ""approximateTimestamp"": 0-100,
        ""keyDialogue"": ""the exact quote that defines this story beat""
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
      ""significance"": ""why it matters"",
      ""notableQuotes"": [""exact quote 1"", ""exact quote 2""]
    }}
  ]
}}

Focus on Adventure Time's unique blend of humor and depth. Pay attention to subtext and implied emotions.
Respond ONLY with valid JSON, no additional text or markdown formatting.";
    }

    private string BuildSeasonAnalysisPrompt(List<EpisodeAnalysis> analyses, int season)
    {
        // Same as your existing implementation
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
        // Same as your existing implementation
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

    private class OpenAIResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
    private sealed class ResponsesEnvelope
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public List<OutputItem>? Output { get; set; }

        // Some responses include a convenience field; use if present.
        public string? OutputText { get; set; }
    }

    private sealed class OutputItem
    {
        public string? Id { get; set; }
        public string? Type { get; set; }          // "message", "reasoning", etc.
        public string? Role { get; set; }          // "assistant", "user" (often on message)
        public List<ContentPart>? Content { get; set; }
    }

    private sealed class ContentPart
    {
        public string? Type { get; set; }          // "output_text", "tool_result", ...
        public string? Text { get; set; }          // present when Type == "output_text"
    }

    private static string ExtractAssistantText(ResponsesEnvelope env)
    {
        // 1) Use top-level OutputText if present
        if (!string.IsNullOrWhiteSpace(env.OutputText))
            return env.OutputText!;

        // 2) Otherwise, find the assistant message and concatenate its output_text parts
        var msg = env.Output?
            .FirstOrDefault(o => string.Equals(o.Type, "message", StringComparison.OrdinalIgnoreCase)
                                 && (o.Role is null || string.Equals(o.Role, "assistant", StringComparison.OrdinalIgnoreCase)));

        var text = string.Concat(
            msg?.Content?
                .Where(c => string.Equals(c.Type, "output_text", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Text) 
            ?? Enumerable.Empty<string>());

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("No assistant output_text found in Responses API payload.");

        return text;
    }

}