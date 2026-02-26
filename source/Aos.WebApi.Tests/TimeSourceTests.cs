using Aos.WebApi.Models;
using Aos.WebApi.Services;
using Xunit;

namespace Aos.WebApi.Tests;

public sealed class TimeSourceTests
{
    [Fact]
    public void RecordingTimeSource_RecordsReturnedInstants_AndReportsRecordMode()
    {
        var t1 = new DateTimeOffset(2026, 2, 26, 10, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 2, 26, 10, 0, 1, TimeSpan.Zero);
        var inner = new StubTimeSource(
            [t1, t2],
            new TimeSourceInfo("record", "stub-clock", "clock-stub", "utc-millis", "stub"));
        var timeSource = new RecordingTimeSource(inner);

        var first = timeSource.NowUtc();
        var second = timeSource.NowUtc();
        var descriptor = timeSource.Describe();

        Assert.Equal(t1, first);
        Assert.Equal(t2, second);
        Assert.Equal(new[] { t1, t2 }, timeSource.GetRecordedInstants());
        Assert.Equal("record", descriptor.Mode);
        Assert.Contains("time recording enabled", descriptor.Notes);
    }

    [Fact]
    public void ReplayTimeSource_ReplaysRecordedInstantsInOrder_AndReportsReplayMode()
    {
        var t1 = new DateTimeOffset(2026, 2, 26, 11, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 2, 26, 11, 0, 1, TimeSpan.Zero);
        var timeSource = new ReplayTimeSource([t1, t2]);

        var first = timeSource.NowUtc();
        var second = timeSource.NowUtc();
        var descriptor = timeSource.Describe();

        Assert.Equal(t1, first);
        Assert.Equal(t2, second);
        Assert.Equal("replay", descriptor.Mode);
        Assert.Equal("recorded-sequence", descriptor.Source);
    }

    [Fact]
    public void ReplayTimeSource_WhenExhausted_Throws()
    {
        var t1 = new DateTimeOffset(2026, 2, 26, 12, 0, 0, TimeSpan.Zero);
        var timeSource = new ReplayTimeSource([t1]);

        _ = timeSource.NowUtc();

        Assert.Throws<InvalidOperationException>(() => timeSource.NowUtc());
    }

    private sealed class StubTimeSource : ITimeSource
    {
        private readonly Queue<DateTimeOffset> _values;
        private readonly TimeSourceInfo _descriptor;

        public StubTimeSource(IEnumerable<DateTimeOffset> values, TimeSourceInfo descriptor)
        {
            _values = new Queue<DateTimeOffset>(values);
            _descriptor = descriptor;
        }

        public DateTimeOffset NowUtc()
        {
            if (!_values.TryDequeue(out var value))
            {
                throw new InvalidOperationException("No more test instants available.");
            }

            return value;
        }

        public TimeSourceInfo Describe() => _descriptor;
    }
}
