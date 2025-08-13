namespace AdventureTime.Application.Entities.EpisodeAnalysis.SubEntities;

public class RelationshipDynamicEntity
{
    public int Id { get; set; }
    public int EpisodeAnalysisId { get; set; }
    public EpisodeAnalysisEntity? EpisodeAnalysis { get; set; }
    public string Character1 { get; set; } = string.Empty;
    public string Character2 { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public double HarmonyScore { get; set; }
    public string DynamicDescription { get; set; } = string.Empty;
}