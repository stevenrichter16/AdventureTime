using AdventureTime.Application.Config;
using AdventureTime.Infrastructure;
using AdventureTime.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace AdventureTime.Tests.Config;

// F1.1-T5: proves StationOptions binds and validates correctly through the real
// AddInfrastructure wiring. No database, no PostgresFixture -- everything here
// is in-memory config + DI resolution.
public class StationOptionsTests
{
    // ---------------------------------------------------------------- helpers

    // Builds a fake IConfiguration for one test, out of just the Station:* keys that
    // test cares about. Every test needs ConnectionStrings:DefaultConnection to exist
    // (see our earlier conversation on why), so we seed it once here and let each
    // caller layer their own Station:* keys on top.
    private static IConfiguration BuildConfig(Dictionary<string, string?> stationKeys)
    {
        var allConfigKeys = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused;Username=u;Password=p",
        };
        foreach (var (key, value) in stationKeys)
        {
            allConfigKeys[key] = value;
        }

        // ConfigurationBuilder is the same type ASP.NET Core uses to build the real
        // configuration from appsettings.json; AddInMemoryCollection is a "source" for
        // it, just like the JSON file source or environment variables are in the real
        // app. Build() turns all the added sources into one IConfiguration.
        var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(allConfigKeys).Build();
        return configBuilder;
    }

    // Runs the *real* AddInfrastructure against a fake configuration, then resolves
    // IOptions<StationOptions> the same way any real component in the app would.
    private static IOptions<StationOptions> Resolve(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<StationOptions>>();
    }

    // ---------------------------------------------------------------- tests

    [Fact]
    public void Defaults_MatchTr35()
    {
        // Arrange: no Station:* keys at all -- only the fixed connection string.
        var configuration = BuildConfig(new Dictionary<string, string?>());

        // Act: resolve the options through the real DI wiring.
        var options = Resolve(configuration);

        // Assert: every property should have fallen back to its TR-35 default.
        // options.Value is where the actual bound-and-validated StationOptions lives --
        // IOptions<T> is just a thin wrapper around it.
        Assert.Equal(3, options.Value.Capacity);
        Assert.Equal(TimeSpan.FromSeconds(15), options.Value.SweepInterval);
        Assert.Equal(TimeSpan.FromMinutes(10), options.Value.ProcessingTimeout);
        Assert.Equal(1800, options.Value.TtlDefault);
        Assert.Equal(60, options.Value.TtlMin);
        Assert.Equal(86400, options.Value.TtlMax);
        Assert.Equal(AnalysisProvider.Stub, options.Value.AnalysisProvider);
    }

    [Fact]
    public void Binding_ParsesTimeSpanAndEnum()
    {
        // Arrange: this time we DO set Station:* keys, as strings -- exactly the way
        // they'd appear in appsettings.json or a docker-compose environment entry.
        var configuration = BuildConfig(new Dictionary<string, string?>
        {
            ["Station:Capacity"] = "5",
            ["Station:SweepInterval"] = "00:00:05",
            ["Station:ProcessingTimeout"] = "00:00:30",
            ["Station:AnalysisProvider"] = "Claude",
        });

        var options = Resolve(configuration);

        // Assert: the binder correctly turned each string into its real typed value --
        // "00:00:05" into an actual TimeSpan, "Claude" into the actual enum member.
        Assert.Equal(5, options.Value.Capacity);
        Assert.Equal(TimeSpan.FromSeconds(5), options.Value.SweepInterval);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Value.ProcessingTimeout);
        Assert.Equal(AnalysisProvider.Claude, options.Value.AnalysisProvider);
    }

    // [Theory] + [InlineData] is xUnit's way of running the SAME test method multiple
    // times with different inputs. Each InlineData line becomes one full run of the
    // method, with its values passed in as the method's parameters, in order.
    //
    // Note on the third row: the underlying rule is "TtlDefault <= TtlMax". Rather than
    // overriding both TtlDefault AND TtlMax to break that rule, we just lower TtlMax
    // below TtlDefault's own default of 1800 -- one key changed, one rule broken,
    // nothing else disturbed. Keeping each row to a single change is what makes it
    // obvious which rule a given row is actually testing.
    [Theory]
    [InlineData("Station:Capacity", "0", "Station:Capacity must be greater than or equal to 1")]
    [InlineData("Station:TtlDefault", "30", "Station:TtlMin must be less than or equal to Station:TtlDefault")]
    [InlineData("Station:TtlMax", "1000", "Station:TtlDefault must be less than or equal to Station:TtlMax")]
    [InlineData("Station:SweepInterval", "00:00:00", "Station:SweepInterval must be greater than 0 seconds")]
    [InlineData("Station:ProcessingTimeout", "00:00:00", "Station:ProcessingTimeout must be greater than or equal to 1 second")]
    public void InvalidConfiguration_ThrowsWithSpecificMessage(string stationKey, string stationValue, string expectedMessage)
    {
        var configuration = BuildConfig(new Dictionary<string, string?> { [stationKey] = stationValue });
        var options = Resolve(configuration);

        // The five .Validate(...) calls in AddInfrastructure don't run at Bind() time --
        // they run the first time something actually reads .Value. So the *resolving*
        // above always succeeds; it's this next line, touching .Value, that triggers
        // validation and is expected to throw.
        var exception = Assert.Throws<OptionsValidationException>(() => options.Value);

        // OptionsValidationException.Failures is a collection of every message from
        // every failed .Validate() call (there's only one broken rule per row here,
        // so exactly one message -- but this assertion doesn't care how many others
        // are in there, only that ours is present).
        Assert.Contains(expectedMessage, exception.Failures);
    }

    [Fact]
    public void InternalsVisibleTo_TargetsTestProject()
    {
        // This one has nothing to do with StationOptions -- it's checking that
        // AdventureTime.Infrastructure's csproj actually grants AdventureTime.Tests
        // access to its internal types (via <InternalsVisibleTo> from F1.1-T1), by
        // reading that fact straight off the compiled assembly's attributes.
        var attributes = typeof(AppDbContext).Assembly
            .GetCustomAttributes<System.Runtime.CompilerServices.InternalsVisibleToAttribute>();

        Assert.Contains(attributes, a => a.AssemblyName == "AdventureTime.Tests");
    }
}
