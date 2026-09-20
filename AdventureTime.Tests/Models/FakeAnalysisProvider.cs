using System.Text.Json;
using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models;
using AdventureTime.Application.Models.CharacterAnalysis;
using AdventureTime.Application.Models.EpisodeAnalysis;
using AdventureTime.Application.Models.SeasonAnalysis;
using Microsoft.Extensions.Logging;

namespace AdventureTime.Tests.Models;

public class FakeAnalysisProvider : IDeepAnalysisService
{
    private readonly EpisodeAnalysis _fixedAnalysis = new();
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task<EpisodeAnalysis> AnalyzeEpisodeAsync(Episode episode,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_fixedAnalysis);
    }

    public async Task<SeasonAnalysis> AnalyzeSeasonTrendsAsync(List<EpisodeAnalysis> episodeAnalyses, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new SeasonAnalysis());
    }
    
    public async Task<CharacterDynamicsAnalysis> AnalyzeCharacterDynamicsAsync(List<Episode> episodes, string characterName, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new CharacterDynamicsAnalysis());
    }
}