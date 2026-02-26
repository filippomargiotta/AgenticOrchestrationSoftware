using Aos.WebApi.Models;

namespace Aos.WebApi.Services;

public sealed class HelloWorkflowService : IHelloWorkflowService
{
    private readonly ISeedProvider _seedProvider;
    private readonly ITimeSource _timeSource;

    public HelloWorkflowService(
        ISeedProvider seedProvider,
        ITimeSource timeSource)
    {
        _seedProvider = seedProvider;
        _timeSource = timeSource;
    }

    public HelloWorkflowArtifacts CreateHelloArtifacts(string runId)
    {
        if (string.IsNullOrWhiteSpace(runId))
        {
            throw new ArgumentException("Run id is required.", nameof(runId));
        }

        var now = _timeSource.NowUtc();
        var seed = _seedProvider.GetLockedSeed(runId);
        var timeSourceInfo = _timeSource.Describe();

        var manifest = new Manifest(
            ManifestVersion: "0.1",
            RunId: runId,
            Seed: seed,
            TimeSource: timeSourceInfo,
            Models: new[]
            {
                new ModelRef("local-null", "local", "0.0")
            },
            Tools: new[]
            {
                new ToolRef("noop", "0.0")
            },
            PolicyDecisions: new[]
            {
                new PolicyDecision("policy-allow", "allow", "placeholder")
            },
            StartedAtUtc: now,
            CompletedAtUtc: now);

        var manifestErrors = ManifestValidator.Validate(manifest);
        if (manifestErrors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", manifestErrors));
        }

        var entry = new EventLogEntry(
            RunId: runId,
            EventType: "workflow.hello",
            Data: new { Message = "hello", ManifestVersion = manifest.ManifestVersion },
            OccurredAtUtc: now);

        return new HelloWorkflowArtifacts(
            Manifest: manifest,
            EventLogEntries: new[] { entry });
    }
}
