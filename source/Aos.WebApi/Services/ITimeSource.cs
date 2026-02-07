namespace Aos.WebApi.Services;

public interface ITimeSource
{
    DateTimeOffset NowUtc();
}
