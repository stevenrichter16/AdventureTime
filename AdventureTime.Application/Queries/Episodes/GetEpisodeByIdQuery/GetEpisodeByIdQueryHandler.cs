using AdventureTime.Application.Interfaces;
using AdventureTime.Models;
using MediatR;

namespace AdventureTime.Application.Queries.Episodes.GetEpisodeByIdQuery;

/// <summary>
/// The query handler knows how to answer the question.
/// Notice how it's separate from the command handler - this is the "Segregation" in CQRS.
/// </summary>
public class GetEpisodeByIdQueryHandler : IRequestHandler<GetEpisodeByIdQuery, Episode?>
{
    private readonly IEpisodeService _episodeService;
    private readonly IDeepAnalysisService _deepAnalysisService;
    
    public GetEpisodeByIdQueryHandler(IEpisodeService episodeService, IDeepAnalysisService deepAnalysisService)
    {
        _episodeService = episodeService;
        _deepAnalysisService = deepAnalysisService;
    }
    
    public async Task<Episode?> Handle(GetEpisodeByIdQuery request, CancellationToken cancellationToken)
    {
        // Queries are typically simpler than commands
        // They just fetch and return data, no complex business logic
        var episode = await _episodeService.GetByIdAsync(request.Id, cancellationToken);
        var analysis = await _deepAnalysisService.AnalyzeEpisodeAsync(episode, cancellationToken);
        return episode;
    }
}