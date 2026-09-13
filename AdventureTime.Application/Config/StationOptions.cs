namespace AdventureTime.Application.Config;

public class StationOptions
{
    public const string SectionName = "Station";
    public int Capacity { get; set; } = 3;
    public int TtlDefault { get; set; } = 1800;
    public int TtlMin { get; set; } = 60;
    public int TtlMax { get; set; } = 86400;
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(15);
    public AnalysisProvider AnalysisProvider { get; set; } = AnalysisProvider.Stub;
}