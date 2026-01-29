using Aos.WebApi.Models;
using Aos.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aos.WebApi.Controllers;

[ApiController]
[Route("workflow")]
public class WorkflowController : ControllerBase
{
    private readonly IEventLogWriter _eventLogWriter;

    public WorkflowController(IEventLogWriter eventLogWriter)
    {
        _eventLogWriter = eventLogWriter;
    }

    [HttpPost("hello")]
    public async Task<IActionResult> Hello(CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid().ToString("N");
        var now = DateTimeOffset.UtcNow;

        var manifest = new Manifest(
            ManifestVersion: "0.1",
            RunId: runId,
            Seed: new SeedInfo(
                SeedId: "seed-1",
                Algorithm: "xoroshiro128**",
                Value: 123456789),
            TimeSource: new TimeSourceInfo(
                Source: "system-utc",
                ClockId: "clock-1",
                Notes: "non-deterministic placeholder"),
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

        var entry = new EventLogEntry(
            RunId: runId,
            EventType: "workflow.hello",
            Data: new { Message = "hello", ManifestVersion = manifest.ManifestVersion },
            OccurredAtUtc: now);

        await _eventLogWriter.WriteAsync(entry, cancellationToken);

        return Ok(new
        {
            RunId = runId,
            Manifest = manifest
        });
    }
}
