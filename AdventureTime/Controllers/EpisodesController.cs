using AdventureTime.Data;
using AdventureTime.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdventureTime.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpisodesController(ILogger<EpisodesController> logger, AppDbContext context) : ControllerBase
{
    [HttpPost(Name = "AddEpisode")]
    public async Task<ActionResult<Episode>> Post([FromBody] Episode episode)
    {
        logger.LogInformation("Adding episode {Episode}", episode);
        try
        {
            var existingEpisode = context.Episodes
                .FirstOrDefault(e => e.EpisodeNumber == episode.EpisodeNumber 
                                     && e.Season == episode.Season);
            
            if (existingEpisode != null)
            {
                logger.LogWarning("Episode S{Season}E{Episode} already exists", 
                    episode.Season, episode.EpisodeNumber);
                    
                return Conflict(new 
                { 
                    message = $"Episode S{episode.Season:D2}E{episode.EpisodeNumber:D2} already exists",
                    existingId = existingEpisode.Id
                });
            }
            
            context.Episodes.Add(episode);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully saved episode. ID: {Id}, SxEx: S{SeasonNumber}E{EpisodeNumber}, Title: {Title}", episode.Id, episode.Season, episode.EpisodeNumber, episode.Title);
            
            return CreatedAtAction(
                nameof(GetEpisodeById), 
                new { id = episode.Id }, 
                episode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database error while saving episode");
                
            // This might happen if there's a constraint violation or connection issue
            return StatusCode(500, new 
            { 
                message = "An error occurred while saving the episode",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Episode>> GetEpisodeById(int id)
    {
        var episode = await context.Episodes.FindAsync(id);
            
        if (episode == null)
        {
            return NotFound(new { message = $"Episode with ID {id} not found" });
        }
            
        return Ok(episode);
    }
}