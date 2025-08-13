using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models;
using MediatR;

namespace AdventureTime.Application.Queries.Episodes.GetEpisodeByIdQuery;

/// <summary>
/// The query handler knows how to answer the question.
/// Notice how it's separate from the command handler - this is the "Segregation" in CQRS.
/// </summary>
public class GetEpisodeByIdQueryHandler : IRequestHandler<GetEpisodeByIdQuery, Episode?>
{
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IEpisodeAnalysisRepository _episodeAnalysisRepository;
    private readonly IDeepAnalysisService _deepAnalysisService;
    
    public GetEpisodeByIdQueryHandler(IEpisodeRepository episodeRepository, IDeepAnalysisService deepAnalysisService)
    {
        _episodeRepository = episodeRepository;
        _deepAnalysisService = deepAnalysisService;
    }
    
    public async Task<Episode?> Handle(GetEpisodeByIdQuery request, CancellationToken cancellationToken)
    {
        // Queries are typically simpler than commands
        // They just fetch and return data, no complex business logic
        var episode = await _episodeRepository.GetByIdAsync(request.Id, cancellationToken);
        //var analysis = await _deepAnalysisService.AnalyzeEpisodeAsync(episode, cancellationToken);
        return episode;
    }
}