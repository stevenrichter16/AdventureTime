namespace AdventureTime.Application.Entities.EpisodeAnalysis.SubEntities;

public class CharacterMoodEntity
{
    public int Id { get; set; }
    public int EpisodeAnalysisId { get; set; }
    public EpisodeAnalysisEntity? EpisodeAnalysis { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string OverallMood { get; set; } = string.Empty;
    public double PositivityScore { get; set; }
    public double JoyScore { get; set; }
    public double SadnessScore { get; set; }
    public double AngerScore { get; set; }
    public double FearScore { get; set; }
    public double SurpriseScore { get; set; }
}