namespace AdventureTime.Application.Models.Continuity;

public record ContinuityContext(IReadOnlyList<PriorEpisodeSummary> Priors);