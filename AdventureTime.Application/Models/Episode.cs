using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureTime.Models;

/// <summary>
/// Represents a single Adventure Time episode with metadata derived from fan analysis patterns
/// </summary>
public class Episode
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Range(1, 10)]
    public int Season { get; set; }
    
    [Range(1, 52)] // Some seasons have many episodes
    public int EpisodeNumber { get; set; }
    
    [MaxLength(20)]
    public string? ProductionCode { get; set; }
    
    public DateTime AirDate { get; set; }
    
    public double RuntimeMinutes { get; set; } = 11.0;
    
    [MaxLength(100)]
    public string? FocusCharacter { get; set; }
    
    [MaxLength(2000)]
    public string? Synopsis { get; set; }
    
    [Column(TypeName = "text")]
    public string? Plot { get; set; }
    
    // Using nvarchar(max) to handle full episode transcripts
    [Column(TypeName = "text")]
    public string? TranscriptText { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifiedAt { get; set; }
    
    [Column(TypeName = "jsonb")]
    public List<string>? MajorCharacters { get; set; }
    
    [Column(TypeName = "jsonb")]
    public List<string>? MinorCharacters { get; set; }
    
    [Column(TypeName = "jsonb")]
    public List<string>? Locations { get; set; }
    
    public int? DialogueLineCount { get; set; }
    
    [NotMapped]
    public string EpisodeCode => $"S{Season:D2}E{EpisodeNumber:D2}";
    
    [NotMapped]
    public bool HasTranscript => !string.IsNullOrEmpty(TranscriptText);
}
