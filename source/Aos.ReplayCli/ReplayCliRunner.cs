using System.Text;
using System.Text.Json;
using Aos.WebApi.Models;
using Aos.WebApi.Options;
using Aos.WebApi.Services;

namespace Aos.ReplayCli;

public static class ReplayCliRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken)
    {
        if (!TryParseArgs(args, out var request, out var parseError))
        {
            await stderr.WriteLineAsync(parseError);
            await stderr.WriteLineAsync("Usage: aos-replay --manifest <path> --eventlog <path>");
            return 2;
        }

        try
        {
            var manifest = await LoadManifestAsync(request.ManifestPath, cancellationToken);
            var expectedEntries = await LoadEventLogEntriesAsync(request.EventLogPath, cancellationToken);

            var manifestErrors = ManifestValidator.Validate(manifest);
            if (manifestErrors.Count > 0)
            {
                await stderr.WriteLineAsync($"Manifest validation failed: {string.Join(" ", manifestErrors)}");
                return 1;
            }

            if (expectedEntries.Count == 0)
            {
                await stderr.WriteLineAsync("Event log is empty.");
                return 1;
            }

            var replayTimeSource = new ReplayTimeSource(expectedEntries.Select(entry => entry.OccurredAtUtc));
            var helloWorkflowOptions = CreateOptionsFromManifest(manifest);
            var service = new HelloWorkflowService(
                new FixedSeedProvider(manifest.Seed),
                replayTimeSource,
                Microsoft.Extensions.Options.Options.Create(helloWorkflowOptions));

            var actual = service.CreateHelloArtifacts(manifest.RunId);
            var mismatches = GetDeterministicMismatches(manifest, actual.Manifest);

            var expectedEventLogJson = SerializeEventLogLines(expectedEntries);
            var actualEventLogJson = SerializeEventLogLines(actual.EventLogEntries);
            if (!string.Equals(expectedEventLogJson, actualEventLogJson, StringComparison.Ordinal))
            {
                mismatches.Add("Event log bytes differ from replay output.");
            }

            if (mismatches.Count > 0)
            {
                foreach (var mismatch in mismatches)
                {
                    await stderr.WriteLineAsync($"Mismatch: {mismatch}");
                }

                return 1;
            }

            await stdout.WriteLineAsync($"Replay verified for run {manifest.RunId}.");
            return 0;
        }
        catch (FileNotFoundException ex)
        {
            await stderr.WriteLineAsync(ex.Message);
            return 2;
        }
        catch (DirectoryNotFoundException ex)
        {
            await stderr.WriteLineAsync(ex.Message);
            return 2;
        }
        catch (JsonException ex)
        {
            await stderr.WriteLineAsync($"Invalid JSON input: {ex.Message}");
            return 2;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            await stderr.WriteLineAsync($"Replay failed: {ex.Message}");
            return 1;
        }
    }

    private static bool TryParseArgs(string[] args, out ReplayRequest request, out string error)
    {
        request = default;
        error = string.Empty;

        if (args.Length == 0)
        {
            error = "Missing required arguments.";
            return false;
        }

        string? manifestPath = null;
        string? eventLogPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--manifest" when i + 1 < args.Length:
                    manifestPath = args[++i];
                    break;
                case "--eventlog" when i + 1 < args.Length:
                    eventLogPath = args[++i];
                    break;
                default:
                    error = $"Unknown or incomplete argument: {args[i]}";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(manifestPath) || string.IsNullOrWhiteSpace(eventLogPath))
        {
            error = "Both --manifest and --eventlog are required.";
            return false;
        }

        request = new ReplayRequest(manifestPath, eventLogPath);
        return true;
    }

    private static async Task<Manifest> LoadManifestAsync(string path, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var manifest = JsonSerializer.Deserialize<Manifest>(json, JsonOptions);
        if (manifest is null)
        {
            throw new JsonException("Manifest JSON deserialized to null.");
        }

        return manifest;
    }

    private static async Task<IReadOnlyList<EventLogEntry>> LoadEventLogEntriesAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var text = await File.ReadAllTextAsync(path, cancellationToken);
        var entries = new List<EventLogEntry>();

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var entry = JsonSerializer.Deserialize<EventLogEntry>(line, JsonOptions);
            if (entry is null)
            {
                throw new JsonException("Event log line deserialized to null.");
            }

            entries.Add(entry);
        }

        return entries;
    }

    private static HelloWorkflowOptions CreateOptionsFromManifest(Manifest manifest)
    {
        return new HelloWorkflowOptions
        {
            Models = manifest.Models
                .Select(model => new HelloWorkflowModelOptions
                {
                    ModelId = model.ModelId,
                    Provider = model.Provider,
                    Version = model.Version
                })
                .ToList(),
            Tools = manifest.Tools
                .Select(tool => new HelloWorkflowToolOptions
                {
                    ToolId = tool.ToolId,
                    Version = tool.Version
                })
                .ToList(),
            PolicyDecisions = manifest.PolicyDecisions
                .Select(policy => new HelloWorkflowPolicyOptions
                {
                    PolicyId = policy.PolicyId,
                    Decision = policy.Decision,
                    Reason = policy.Reason
                })
                .ToList()
        };
    }

    private static List<string> GetDeterministicMismatches(Manifest expected, Manifest actual)
    {
        var mismatches = new List<string>();

        if (!string.Equals(expected.ManifestVersion, actual.ManifestVersion, StringComparison.Ordinal))
        {
            mismatches.Add("ManifestVersion differs.");
        }

        if (!string.Equals(expected.RunId, actual.RunId, StringComparison.Ordinal))
        {
            mismatches.Add("RunId differs.");
        }

        if (expected.Seed != actual.Seed)
        {
            mismatches.Add("Seed differs.");
        }

        if (!expected.Models.SequenceEqual(actual.Models))
        {
            mismatches.Add("Models differ.");
        }

        if (!expected.Tools.SequenceEqual(actual.Tools))
        {
            mismatches.Add("Tools differ.");
        }

        if (!expected.PolicyDecisions.SequenceEqual(actual.PolicyDecisions))
        {
            mismatches.Add("PolicyDecisions differ.");
        }

        if (expected.StartedAtUtc != actual.StartedAtUtc)
        {
            mismatches.Add("StartedAtUtc differs.");
        }

        if (expected.CompletedAtUtc != actual.CompletedAtUtc)
        {
            mismatches.Add("CompletedAtUtc differs.");
        }

        return mismatches;
    }

    private static string SerializeEventLogLines(IEnumerable<EventLogEntry> entries)
    {
        var builder = new StringBuilder();
        foreach (var entry in entries)
        {
            builder.Append(JsonSerializer.Serialize(entry, JsonOptions));
            builder.Append('\n');
        }

        return builder.ToString();
    }

    private sealed class FixedSeedProvider : ISeedProvider
    {
        private readonly SeedInfo _seed;

        public FixedSeedProvider(SeedInfo seed)
        {
            _seed = seed;
        }

        public SeedInfo GetLockedSeed(string runId)
            => _seed;
    }

    private readonly record struct ReplayRequest(string ManifestPath, string EventLogPath);
}
