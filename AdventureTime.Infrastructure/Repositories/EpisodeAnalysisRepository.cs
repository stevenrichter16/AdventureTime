// In Infrastructure/Repositories/EpisodeAnalysisRepository.cs

using AdventureTime.Application.Entities;
using AdventureTime.Application.Entities.EpisodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models;
using AdventureTime.Application.Models.EpisodeAnalysis;
using AdventureTime.Infrastructure.Data;

namespace AdventureTime.Infrastructure.Repositories;

public class EpisodeAnalysisRepository : IEpisodeAnalysisRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EpisodeAnalysisRepository> _logger;
    
    public EpisodeAnalysisRepository(AppDbContext context, ILogger<EpisodeAnalysisRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    

    //
    // public async Task<EpisodeAnalysis?> GetLatestByEpisodeIdAsync(int episodeId, CancellationToken cancellationToken = default)
    // {
    //     var entity = await _context.EpisodeAnalyses
    //         .Include(ea => ea.Episode)
    //         .Where(ea => ea.EpisodeId == episodeId)
    //         .OrderByDescending(ea => ea.AnalysisDate)
    //         .FirstOrDefaultAsync(cancellationToken);
    //         
    //     return entity?.ToDomainModel();
    // }
    //
    // public async Task<List<EpisodeAnalysis>> GetBySeasonAsync(int season, CancellationToken cancellationToken = default)
    // {
    //     var entities = await _context.EpisodeAnalyses
    //         .Include(ea => ea.Episode)
    //         .Where(ea => ea.Episode!.Season == season)
    //         .OrderBy(ea => ea.Episode!.EpisodeNumber)
    //         .ToListAsync(cancellationToken);
    //         
    //     return entities.Select(e => e.ToDomainModel()).ToList();
    // }
    public async Task<EpisodeAnalysis?> GetByEpisodeIdAsync(int episodeId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.EpisodeAnalyses
            .Include(ea => ea.Episode)
            .FirstOrDefaultAsync(ea => ea.EpisodeId == episodeId, cancellationToken);
            
        return entity?.ToDomainModel();
    }
    
    public async Task<EpisodeAnalysis> SaveAsync(
        EpisodeAnalysis analysis, 
        string? source = null, 
        string? version = null, 
        CancellationToken cancellationToken = default)
    {
        // Check if analysis already exists
        var existing = await _context.EpisodeAnalyses
            .FirstOrDefaultAsync(ea => ea.EpisodeId == analysis.EpisodeId, cancellationToken);
            
        if (existing != null)
        {
            // Update existing
            _logger.LogInformation("Updating existing analysis for episode {EpisodeId}", analysis.EpisodeId);
            
            var entity = EpisodeAnalysisEntity.FromDomainModel(analysis, source, version);
            entity.Id = existing.Id;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(existing).CurrentValues.SetValues(entity);
        }
        else
        {
            // Create new
            _logger.LogInformation("Creating new analysis for episode {EpisodeId}", analysis.EpisodeId);
            
            var entity = EpisodeAnalysisEntity.FromDomainModel(analysis, source, version);
            _context.EpisodeAnalyses.Add(entity);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Reload with navigation properties
        return (await GetByEpisodeIdAsync(analysis.EpisodeId, cancellationToken))!;
    }
    
    // public async Task<List<EpisodeAnalysis>> SaveBatchAsync(
    //     List<EpisodeAnalysis> analyses, 
    //     string? source = null, 
    //     string? version = null, 
    //     CancellationToken cancellationToken = default)
    // {
    //     var results = new List<EpisodeAnalysis>();
    //     
    //     // Process in batches to avoid memory issues
    //     foreach (var batch in analyses.Chunk(10))
    //     {
    //         foreach (var analysis in batch)
    //         {
    //             var saved = await SaveAsync(analysis, source, version, cancellationToken);
    //             results.Add(saved);
    //         }
    //     }
    //     
    //     return results;
    // }
    //
    // public async Task<bool> ExistsAsync(int episodeId, CancellationToken cancellationToken = default)
    // {
    //     return await _context.EpisodeAnalyses
    //         .AnyAsync(ea => ea.EpisodeId == episodeId, cancellationToken);
    // }
    //
    // public async Task<List<EpisodeAnalysis>> GetByCharacterAsync(string characterName, CancellationToken cancellationToken = default)
    // {
    //     // This queries the JSON data - might be slow for large datasets
    //     // Consider using the normalized CharacterMoodEntity table for better performance
    //     var entities = await _context.EpisodeAnalyses
    //         .Include(ea => ea.Episode)
    //         .Where(ea => ea.CharacterMoodsJson.Contains($"\"{characterName}\""))
    //         .ToListAsync(cancellationToken);
    //         
    //     return entities.Select(e => e.ToDomainModel()).ToList();
    // }
    //
    // public async Task<List<EpisodeAnalysis>> GetByEmotionAsync(
    //     string emotion, 
    //     double minIntensity = 0.7, 
    //     CancellationToken cancellationToken = default)
    // {
    //     var entities = await _context.EpisodeAnalyses
    //         .Include(ea => ea.Episode)
    //         .Where(ea => ea.DominantEmotion == emotion && ea.IntensityScore >= minIntensity)
    //         .OrderByDescending(ea => ea.IntensityScore)
    //         .ToListAsync(cancellationToken);
    //         
    //     return entities.Select(e => e.ToDomainModel()).ToList();
    // }
    //
    // public async Task<Dictionary<string, double>> GetEmotionDistributionAsync(
    //     int? season = null, 
    //     CancellationToken cancellationToken = default)
    // {
    //     var query = _context.EpisodeAnalyses.AsQueryable();
    //     
    //     if (season.HasValue)
    //     {
    //         query = query.Where(ea => ea.Episode!.Season == season.Value);
    //     }
    //     
    //     var emotions = await query
    //         .GroupBy(ea => ea.DominantEmotion)
    //         .Select(g => new { Emotion = g.Key, Count = g.Count() })
    //         .ToListAsync(cancellationToken);
    //         
    //     var total = emotions.Sum(e => e.Count);
    //     
    //     return emotions.ToDictionary(
    //         e => e.Emotion,
    //         e => total > 0 ? (double)e.Count / total : 0
    //     );
    // }
}