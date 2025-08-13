using AdventureTime.Application.Models;

namespace AdventureTime.Application.Interfaces;

/// <summary>
/// This interface defines the contract for episode-related operations.
/// It's like a job description - it tells us what an EpisodeService must be able to do,
/// but not HOW it does it. This allows us to swap implementations later if needed.
/// </summary>
public interface IEpisodeRepository
{
    /// <summary>
    /// Creates a new episode in the database
    /// </summary>
    Task<Episode> CreateAsync(Episode episode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves an episode by its unique ID
    /// </summary>
    Task<Episode?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds an episode by season and episode number
    /// </summary>
    Task<Episode?> GetBySeasonAndNumberAsync(int season, int episodeNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing episode
    /// </summary>
    Task<Episode> UpdateAsync(Episode episode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an episode by ID
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all episodes, optionally filtered by season
    /// </summary>
    Task<List<Episode>> GetAllAsync(int? season = null, CancellationToken cancellationToken = default);
}