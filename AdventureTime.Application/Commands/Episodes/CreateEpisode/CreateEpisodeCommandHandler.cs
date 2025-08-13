using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureTime.Application.Commands.Episodes.CreateEpisode;

/// <summary>
/// The command handler is like a specialized worker that knows exactly how to handle one specific task.
/// It's the bridge between your command (the request) and your business logic (the service).
/// </summary>
public class CreateEpisodeCommandHandler : IRequestHandler<CreateEpisodeCommand, CreateEpisodeResult>
{
    private readonly IEpisodeRepository _episodeRepository;
    private readonly ILogger<CreateEpisodeCommandHandler> _logger;
    
    // Dependencies are injected through the constructor
    // This follows the Dependency Inversion Principle - we depend on abstractions (interfaces) not concrete types
    public CreateEpisodeCommandHandler(
        IEpisodeRepository episodeRepository, 
        ILogger<CreateEpisodeCommandHandler> logger)
    {
        _episodeRepository = episodeRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// This is where the magic happens - the Handle method processes the command
    /// </summary>
    public async Task<CreateEpisodeResult> Handle(CreateEpisodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CreateEpisodeCommand for S{Season}E{Episode}: {Title}", 
            request.Season, request.EpisodeNumber, request.Title);
        
        try
        {
            // First, let's check if this episode already exists
            // This is business logic that belongs in the handler, not the controller
            var existingEpisode = await _episodeRepository.GetBySeasonAndNumberAsync(
                request.Season, 
                request.EpisodeNumber, 
                cancellationToken);
            
            if (existingEpisode != null)
            {
                _logger.LogWarning("Episode S{Season}E{Episode} already exists with ID {Id}", 
                    request.Season, request.EpisodeNumber, existingEpisode.Id);
                    
                return CreateEpisodeResult.Conflict(
                    existingEpisode.Id,
                    $"Episode S{request.Season:D2}E{request.EpisodeNumber:D2} already exists");
            }
            
            // Map the command to our domain model
            // This separation means our API can evolve independently from our database model
            var episode = new Episode
            {
                Title = request.Title,
                Season = request.Season,
                EpisodeNumber = request.EpisodeNumber,
                ProductionCode = request.ProductionCode,
                AirDate = request.AirDate,
                RuntimeMinutes = request.RuntimeMinutes,
                FocusCharacter = request.FocusCharacter,
                Synopsis = request.Synopsis,
                Plot = request.Plot,
                TranscriptText = request.TranscriptText,
                MajorCharacters = request.MajorCharacters,
                MinorCharacters = request.MinorCharacters,
                Locations = request.Locations,
                DialogueLineCount = request.DialogueLineCount,
                CreatedAt = DateTime.UtcNow
            };
            
            // Delegate the actual saving to the service
            var savedEpisode = await _episodeRepository.CreateAsync(episode, cancellationToken);
            
            _logger.LogInformation("Successfully created episode with ID {Id}", savedEpisode.Id);
            
            return CreateEpisodeResult.Success(savedEpisode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating episode S{Season}E{Episode}", 
                request.Season, request.EpisodeNumber);
                
            // In a production app, you might want to hide internal error details
            // and return a generic message instead
            return CreateEpisodeResult.InternalError(
                $"An error occurred while creating the episode: {ex.Message}");
        }
    }
}