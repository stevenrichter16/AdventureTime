namespace AdventureTime.Application.Models.EpisodeAnalysis.SubModels;

public class RelationshipDynamic
{
    public string Character1 { get; set; } = string.Empty;
    public string Character2 { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // friendship, rivalry, romantic, mentor-student
    public double HarmonyScore { get; set; } // -1 (conflict) to 1 (harmony)
    public string DynamicDescription { get; set; } = string.Empty;
    public List<string> KeyInteractions { get; set; } = new();
    public string Evolution { get; set; } = string.Empty; // How relationship changed
}