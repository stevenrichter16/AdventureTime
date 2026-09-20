using AdventureTime.Application.Interfaces;
using AdventureTime.Tests.Models;
using AdventureTime.Application.Models;
using AdventureTime.Application.Models.Continuity;
using AdventureTime.Application.Models.EpisodeAnalysis;

namespace AdventureTime.Tests.Services;

public class FakeAnalysisProviderTests
{
    private readonly IDeepAnalysisService _provider = new FakeAnalysisProvider();
    
    [Fact]
    public async Task AnalyzeEpisode_With_Context_Returns_Same_Episode_As_AnalyzeEpisode_Without_Context()
    {
        
        var episodeAnalysisOverload = await _provider.AnalyzeEpisodeAsync(new Episode(), new ContinuityContext([]), CancellationToken.None);
        var episodeAnalysis = await _provider.AnalyzeEpisodeAsync(new Episode(), CancellationToken.None);
        Assert.Same(episodeAnalysisOverload, episodeAnalysis);
    }
    
    [Fact]
    public async Task AnalyzeEpisode_Returns_EpisodeAnalysis()
    {
        
        var episodeAnalysis = await _provider.AnalyzeEpisodeAsync(new Episode());
        Assert.NotNull(episodeAnalysis);
    }

    [Fact]
    public async Task AnalyzeSeasonTrends_Returns_SeasonAnalysis()
    {
        var seasonAnalysis = await _provider.AnalyzeSeasonTrendsAsync(new List<EpisodeAnalysis>());
        Assert.NotNull(seasonAnalysis);
    }

    [Fact]
    public async Task EmptyContextCollectinon_Has_0_Length()
    {
        var continuityContext = new ContinuityContext([]);
        Assert.Empty(continuityContext.Priors);
        Assert.True(continuityContext.Priors.Count == 0);
    }
}