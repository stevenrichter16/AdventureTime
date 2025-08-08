using MediatR;

namespace AdventureTime.Application.Commands.Episodes.CreateEpisode;

/// <summary>
/// This command represents the intention to create a new episode.
/// Think of it as a formal request or work order that contains all the information needed.
/// </summary>
public class CreateEpisodeCommand : IRequest<CreateEpisodeResult>
{
    // All the data needed to create an episode lives here
    // Notice how this is just pure data - no behavior, no database access
    public string Title { get; set; } = string.Empty;
    public int Season { get; set; }
    public int EpisodeNumber { get; set; }
    public string? ProductionCode { get; set; }
    public DateTime AirDate { get; set; }
    public double RuntimeMinutes { get; set; } = 11.0;
    public string? FocusCharacter { get; set; }
    public string? Synopsis { get; set; }
    public string? Plot { get; set; }
    public string? TranscriptText { get; set; }
    public List<string>? MajorCharacters { get; set; }
    public List<string>? MinorCharacters { get; set; }
    public List<string>? Locations { get; set; }
    public int? DialogueLineCount { get; set; }
}