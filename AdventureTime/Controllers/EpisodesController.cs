using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using AdventureTime.Application.Commands.Episodes.CreateEpisode;
using AdventureTime.Application.Enums;
using AdventureTime.Application.Interfaces;
using AdventureTime.Application.Queries.Episodes.GetEpisodeByIdQuery;
using AdventureTime.Models;

namespace AdventureTime.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpisodesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EpisodesController> _logger;
    private readonly IDeepAnalysisService _deepAnalysisService;
    private readonly IEpisodeAnalysisRepository _episodeAnalysisService;
    
    // Notice how much simpler our constructor is now!
    // We only depend on MediatR, not on the database context or services
    public EpisodesController(IMediator mediator, ILogger<EpisodesController> logger, IDeepAnalysisService deepAnalysisService, IEpisodeAnalysisRepository episodeAnalysisRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _deepAnalysisService = deepAnalysisService;
        _episodeAnalysisService = episodeAnalysisRepository;
    }
    
    /// <summary>
    /// Creates a new Adventure Time episode
    /// </summary>
    /// <param name="command">The episode data to create</param>
    /// <returns>The created episode</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Episode>> CreateEpisode([FromBody] CreateEpisodeCommand command)
    {
        _logger.LogInformation("Received request to create episode: {Title}", command.Title);
        
        // Send the command through MediatR - it will find the right handler
        var result = await _mediator.Send(command);
        
        if (result is { IsSuccess: true, Episode: not null })
            // 201 Created with location header pointing to the new resource
            return CreatedAtAction(
                nameof(GetEpisodeById),
                new { id = result.Episode.Id },
                result.Episode);
        
        return result.FailureType switch
        {
            FailureType.Conflict => Conflict(new 
            { 
                message = result.ErrorMessage,
                existingEpisodeId = result.ConflictingEpisodeId
            }),
            
            FailureType.ValidationFailed => BadRequest(new
            {
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            }),
            
            FailureType.InternalError => StatusCode(500, new 
            { 
                message = result.ErrorMessage 
            }),
            
            // This null case should never happen with our factory methods,
            // but it makes the compiler happy and catches any future bugs
            null => StatusCode(500, new { message = "An unexpected error occurred" }),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    /// <summary>
    /// Gets an episode by its ID
    /// </summary>
    /// <param name="id">The episode ID</param>
    /// <returns>The requested episode</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Episode>> GetEpisodeById(int id)
    {
        var query = new GetEpisodeByIdQuery { Id = id };
        var episode = await _mediator.Send(query);
        
        if (episode == null)
        {
            return NotFound(new { message = $"Episode with ID {id} not found" });
        }
        
        return Ok(episode);
    }

    [HttpPost]
    [Route("api/[controller]/analysis")]
    public async Task<ActionResult<EpisodeAnalysis>> CreateEpisodeAnalysis(int id)
    {
        var query = new GetEpisodeByIdQuery { Id = id };
        var episode = await _mediator.Send(query);
        
        if (episode == null)
        {
            return NotFound(new { message = $"Episode with ID {id} not found" });
        }
        
        var analysis = await _deepAnalysisService.AnalyzeEpisodeAsync(episode);
        //Debug.WriteLine("Analysis for episode with ID {id}: {analysis}", id, analysis);
        await _episodeAnalysisService.SaveAsync(analysis);
        return Ok(analysis);
    }
    
    // Additional endpoints would follow the same pattern:
    // - UpdateEpisode would use UpdateEpisodeCommand
    // - DeleteEpisode would use DeleteEpisodeCommand
    // - GetAllEpisodes would use GetAllEpisodesQuery
}