namespace Aos.WebApi.Services;

public sealed class SystemTimeSource : ITimeSource
{
    public DateTimeOffset NowUtc() => DateTimeOffset.UtcNow;
}
