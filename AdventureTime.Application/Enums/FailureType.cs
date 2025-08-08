namespace AdventureTime.Application.Enums;

/// <summary>
/// Explicitly enumerates all possible failure types.
/// This is like having different diagnostic codes for different car problems
/// instead of just a generic "check engine" light.
/// </summary>
public enum FailureType
{
    /// <summary>
    /// The episode already exists (same season and episode number)
    /// </summary>
    Conflict,
    
    /// <summary>
    /// The request data didn't pass validation rules
    /// </summary>
    ValidationFailed,
    
    /// <summary>
    /// An unexpected error occurred (database down, etc.)
    /// </summary>
    InternalError
}