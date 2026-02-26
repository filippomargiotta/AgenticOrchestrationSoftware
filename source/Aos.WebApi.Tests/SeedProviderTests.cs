using Aos.WebApi.Models;
using Aos.WebApi.Services;
using Xunit;

namespace Aos.WebApi.Tests;

public sealed class SeedProviderTests
{
    [Fact]
    public void GetLockedSeed_SameRunId_ReturnsSameSeed()
    {
        var provider = new LockedSeedProvider(new SequenceSeedGenerator(101, 202));

        var first = provider.GetLockedSeed("run-1");
        var second = provider.GetLockedSeed("run-1");

        Assert.Equal(first, second);
        Assert.Equal(101, first.Value);
    }

    [Fact]
    public void GetLockedSeed_DifferentRunIds_GeneratesDistinctSeeds()
    {
        var provider = new LockedSeedProvider(new SequenceSeedGenerator(101, 202));

        var first = provider.GetLockedSeed("run-1");
        var second = provider.GetLockedSeed("run-2");

        Assert.NotEqual(first.Value, second.Value);
        Assert.Equal("seed-run-1", first.SeedId);
        Assert.Equal("seed-run-2", second.SeedId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetLockedSeed_BlankRunId_Throws(string runId)
    {
        var provider = new LockedSeedProvider(new SequenceSeedGenerator(101));

        Assert.Throws<ArgumentException>(() => provider.GetLockedSeed(runId));
    }

    private sealed class SequenceSeedGenerator : ISeedGenerator
    {
        private readonly Queue<long> _values;

        public SequenceSeedGenerator(params long[] values)
        {
            _values = new Queue<long>(values);
        }

        public SeedInfo CreateSeed(string runId)
        {
            if (!_values.TryDequeue(out var value))
            {
                throw new InvalidOperationException("No more test seeds available.");
            }

            return new SeedInfo(
                SeedId: $"seed-{runId}",
                Algorithm: "test-sequence",
                Value: value,
                Derivation: "test");
        }
    }
}
