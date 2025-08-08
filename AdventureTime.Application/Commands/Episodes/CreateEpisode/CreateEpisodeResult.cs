using AdventureTime.Application.Enums;
using AdventureTime.Models;

namespace AdventureTime.Application.Commands.Episodes.CreateEpisode;

/// <summary>
/// Represents the result of attempting to create an episode.
/// This is an example of the "Result" pattern - a way to handle success and failure
/// without throwing exceptions, making the possible outcomes explicit and type-safe.
/// </summary>
public class CreateEpisodeResult
{
    // Making the constructor private ensures results can only be created through our factory methods
    // This guarantees that every result is in a valid state
    private CreateEpisodeResult() { }
    
    /// <summary>
    /// Indicates whether the operation succeeded
    /// </summary>
    public bool IsSuccess { get; private set; }
    
    /// <summary>
    /// The type of failure, if any. This makes error handling explicit and exhaustive.
    /// </summary>
    public FailureType? FailureType { get; private set; }
    
    /// <summary>
    /// The created episode (only populated on success)
    /// </summary>
    public Episode? Episode { get; private set; }
    
    /// <summary>
    /// The ID of an existing episode (only populated for conflicts)
    /// </summary>
    public int? ConflictingEpisodeId { get; private set; }
    
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    /// <summary>
    /// Additional error details (like validation errors)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }
    
    // Factory methods make it impossible to create an inconsistent result
    // Each method clearly shows what information is available in each scenario
    
    public static CreateEpisodeResult Success(Episode episode) => new()
    {
        IsSuccess = true,
        Episode = episode ?? throw new ArgumentNullException(nameof(episode))
    };
    
    public static CreateEpisodeResult Conflict(int existingEpisodeId, string message) => new()
    {
        IsSuccess = false,
        FailureType = Enums.FailureType.Conflict,
        ConflictingEpisodeId = existingEpisodeId,
        ErrorMessage = message
    };
    
    public static CreateEpisodeResult ValidationFailed(Dictionary<string, string[]> errors) => new()
    {
        IsSuccess = false,
        FailureType = Enums.FailureType.ValidationFailed,
        ValidationErrors = errors,
        ErrorMessage = "One or more validation errors occurred."
    };
    
    public static CreateEpisodeResult InternalError(string message, Exception? exception = null) => new()
    {
        IsSuccess = false,
        FailureType = Enums.FailureType.InternalError,
        ErrorMessage = message,
        // In production, you might log the exception details but not expose them
        ValidationErrors = exception != null 
            ? new Dictionary<string, string[]> { ["Exception"] = new[] { exception.GetType().Name } }
            : null
    };
}

