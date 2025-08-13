using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models;
using AdventureTime.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdventureTime.Infrastructure.Repositories;

/// <summary>
/// The concrete implementation of our episode service.
/// This is where the rubber meets the road - actual database operations happen here.
/// Notice how this class is focused solely on data operations, not HTTP concerns or command processing.
/// </summary>
public class EpisodeRepository : IEpisodeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EpisodeRepository> _logger;
    
    public EpisodeRepository(AppDbContext context, ILogger<EpisodeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Episode> CreateAsync(Episode episode, CancellationToken cancellationToken = default)
    {
        // The service handles the "how" of saving data
        // It doesn't know or care why we're saving - that's the handler's job
        _logger.LogDebug("Creating new episode: {Title}", episode.Title);
        
        _context.Episodes.Add(episode);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogDebug("Episode created with ID: {Id}", episode.Id);
        return episode;
    }
    
    public async Task<Episode?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Using async methods for all database operations
        // This keeps our application responsive even under load
        return await _context.Episodes
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    
    public async Task<Episode?> GetBySeasonAndNumberAsync(int season, int episodeNumber, CancellationToken cancellationToken = default)
    {
        // Notice how we use FirstOrDefaultAsync instead of FirstOrDefault
        // This is crucial for scalability - it doesn't block the thread while waiting for the database
        return await _context.Episodes
            .FirstOrDefaultAsync(e => e.Season == season && e.EpisodeNumber == episodeNumber, cancellationToken);
    }
    
    public async Task<Episode> UpdateAsync(Episode episode, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating episode ID: {Id}", episode.Id);
        
        // Set the modified timestamp
        episode.LastModifiedAt = DateTime.UtcNow;
        
        // Tell EF Core this entity has been modified
        _context.Episodes.Update(episode);
        await _context.SaveChangesAsync(cancellationToken);
        
        return episode;
    }
    
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var episode = await GetByIdAsync(id, cancellationToken);
        if (episode == null)
        {
            _logger.LogWarning("Attempted to delete non-existent episode with ID: {Id}", id);
            return false;
        }
        
        _context.Episodes.Remove(episode);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Deleted episode ID: {Id}, Title: {Title}", id, episode.Title);
        return true;
    }
    
    public async Task<List<Episode>> GetAllAsync(int? season = null, CancellationToken cancellationToken = default)
    {
        // Start building our query
        // IQueryable allows us to build up a query before executing it
        IQueryable<Episode> query = _context.Episodes;
        
        // Add filtering if requested
        if (season.HasValue)
        {
            query = query.Where(e => e.Season == season.Value);
        }
        
        // Order by season and episode number for a logical sequence
        query = query.OrderBy(e => e.Season).ThenBy(e => e.EpisodeNumber);
        
        // Execute the query and return the results
        return await query.ToListAsync(cancellationToken);
    }
}