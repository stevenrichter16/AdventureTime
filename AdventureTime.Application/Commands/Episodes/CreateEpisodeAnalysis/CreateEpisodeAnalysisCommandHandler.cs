using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models.EpisodeAnalysis;
using AdventureTime.Application.Queries.Episodes.GetEpisodeByIdQuery;

namespace AdventureTime.Application.Commands.Episodes.CreateEpisodeAnalysis;

using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// The command handler is like a specialized worker that knows exactly how to handle one specific task.
/// It's the bridge between your command (the request) and your business logic (the service).
/// </summary>
public class CreateEpisodeAnalysisCommandHandler : IRequestHandler<CreateEpisodeAnalysisCommand, CreateEpisodeAnalysisResult>
{
    private readonly IMediator _mediator;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly ILogger<CreateEpisodeAnalysisCommandHandler> _logger;
    private readonly IDeepAnalysisService _deepAnalysisService;
    private readonly IEpisodeAnalysisRepository _episodeAnalysisRepository;
    
    // Dependencies are injected through the constructor
    // This follows the Dependency Inversion Principle - we depend on abstractions (interfaces) not concrete types
    public CreateEpisodeAnalysisCommandHandler(
        IMediator mediator,
        IEpisodeRepository episodeRepository, 
        ILogger<CreateEpisodeAnalysisCommandHandler> logger,
        IDeepAnalysisService deepAnalysisService,
        IEpisodeAnalysisRepository episodeAnalysisRepository)
    {
        _mediator = mediator;
        _episodeRepository = episodeRepository;
        _logger = logger;
        _deepAnalysisService = deepAnalysisService;
        _episodeAnalysisRepository = episodeAnalysisRepository;
    }
    
    /// <summary>
    /// This is where the magic happens - the Handle method processes the command
    /// </summary>
    public async Task<CreateEpisodeAnalysisResult> Handle(CreateEpisodeAnalysisCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CreateEpisodeAnalysisCommand for EpisodeId: {Id}", 
            request.Id);
        
        try
        {
            var query = new GetEpisodeByIdQuery { Id = request.Id };
            var episode = await _mediator.Send(query, cancellationToken);
            var episodeAnalysis = await _deepAnalysisService.AnalyzeEpisodeAsync(episode, cancellationToken);
            await _episodeAnalysisRepository.SaveAsync(episodeAnalysis);
            return CreateEpisodeAnalysisResult.Success(new EpisodeAnalysis());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating episode analysis for EpisodeId: {Id}", 
                request.Id);
                
            // In a production app, you might want to hide internal error details
            // and return a generic message instead
            return CreateEpisodeAnalysisResult.InternalError(
                $"An error occurred while creating the episode analysis: {ex.Message}");
        }
    }
}