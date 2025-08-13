namespace AdventureTime.Application.Commands.Episodes.CreateEpisodeAnalysis;

using MediatR;

/// <summary>
/// This command represents the intention to create a new episode.
/// Think of it as a formal request or work order that contains all the information needed.
/// </summary>
public class CreateEpisodeAnalysisCommand : IRequest<CreateEpisodeAnalysisResult>
{
    public int Id { get; set; }
}