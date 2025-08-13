using AdventureTime.Application.Enums;
using AdventureTime.Application.Models.EpisodeAnalysis;

namespace AdventureTime.Application.Commands.Episodes.CreateEpisodeAnalysis;

public class CreateEpisodeAnalysisResult
{
    // Making the constructor private ensures results can only be created through our factory methods
    // This guarantees that every result is in a valid state
    private CreateEpisodeAnalysisResult() { }
    
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
    public EpisodeAnalysis? EpisodeAnalysis { get; private set; }
    
    public string? ErrorMessage { get; private set; }
    
    /// <summary>
    /// Additional error details (like validation errors)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }
    
    public static CreateEpisodeAnalysisResult Success(EpisodeAnalysis episodeAnalysis) => new()
    {
        IsSuccess = true,
        EpisodeAnalysis = episodeAnalysis ?? throw new ArgumentNullException(nameof(episodeAnalysis))
    };
    
    public static CreateEpisodeAnalysisResult Conflict(string message) => new()
    {
        IsSuccess = false,
        FailureType = Enums.FailureType.Conflict,
        ErrorMessage = message
    };
    
    public static CreateEpisodeAnalysisResult ValidationFailed(Dictionary<string, string[]> errors) => new()
    {
        IsSuccess = false,
        FailureType = Enums.FailureType.ValidationFailed,
        ValidationErrors = errors,
        ErrorMessage = "One or more validation errors occurred."
    };
    
    public static CreateEpisodeAnalysisResult InternalError(string message, Exception? exception = null) => new()
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