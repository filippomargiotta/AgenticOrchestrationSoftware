using System.Text;
using System.Text.Json;
using Aos.WebApi.Models;
using Aos.WebApi.Options;
using Microsoft.Extensions.Options;

namespace Aos.WebApi.Services;

public sealed class FileEventLogWriter : IEventLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly EventLogOptions _options;
    private readonly string _rootPath;

    public FileEventLogWriter(IOptions<EventLogOptions> options, IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _rootPath = hostEnvironment.ContentRootPath;
    }

    public async Task WriteAsync(EventLogEntry entry, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(_rootPath, _options.Directory);
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, _options.FileName);
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        await File.AppendAllTextAsync(path, json + Environment.NewLine, Encoding.UTF8, cancellationToken);
    }
}
