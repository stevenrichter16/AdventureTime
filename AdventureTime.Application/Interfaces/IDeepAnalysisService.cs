// In Application/Common/Interfaces/IDeepAnalysisService.cs

using AdventureTime.Application.Models;
using AdventureTime.Application.Models.CharacterAnalysis;
using AdventureTime.Application.Models.EpisodeAnalysis;
using AdventureTime.Application.Models.SeasonAnalysis;

namespace AdventureTime.Application.Interfaces;

public interface IDeepAnalysisService
{
    Task<EpisodeAnalysis> AnalyzeEpisodeAsync(Episode episode, CancellationToken cancellationToken = default);
    Task<SeasonAnalysis> AnalyzeSeasonTrendsAsync(List<EpisodeAnalysis> episodeAnalyses, CancellationToken cancellationToken = default);
    Task<CharacterDynamicsAnalysis> AnalyzeCharacterDynamicsAsync(List<Episode> episodes, string characterName, CancellationToken cancellationToken = default);
}