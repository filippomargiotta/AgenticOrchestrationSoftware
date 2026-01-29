using Aos.WebApi.Models;

namespace Aos.WebApi.Services;

public interface IEventLogWriter
{
    Task WriteAsync(EventLogEntry entry, CancellationToken cancellationToken = default);
}
