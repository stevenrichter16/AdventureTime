using AdventureTime.Application.Models.EpisodeAnalysis;

namespace AdventureTime.Application.Interfaces;

public interface IEpisodeAnalysisRepository
{
    Task<EpisodeAnalysis?> GetByEpisodeIdAsync(int episodeId, CancellationToken cancellationToken = default);
    // Task<List<EpisodeAnalysis>> GetBySeasonAsync(int season, CancellationToken cancellationToken = default);
    Task<EpisodeAnalysis> SaveAsync(EpisodeAnalysis analysis, string? source = null, string? version = null, CancellationToken cancellationToken = default);
    // Task<List<EpisodeAnalysis>> SaveBatchAsync(List<EpisodeAnalysis> analyses, string? source = null, string? version = null, CancellationToken cancellationToken = default);
    // Task<bool> ExistsAsync(int episodeId, CancellationToken cancellationToken = default);
    // Task<EpisodeAnalysis?> GetLatestByEpisodeIdAsync(int episodeId, CancellationToken cancellationToken = default);
    // Task<List<EpisodeAnalysis>> GetByCharacterAsync(string characterName, CancellationToken cancellationToken = default);
    // Task<List<EpisodeAnalysis>> GetByEmotionAsync(string emotion, double minIntensity = 0.7, CancellationToken cancellationToken = default);
    // Task<Dictionary<string, double>> GetEmotionDistributionAsync(int? season = null, CancellationToken cancellationToken = default);
}