using Aos.WebApi.Models;
using Aos.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aos.WebApi.Controllers;

[ApiController]
[Route("workflow")]
public class WorkflowController : ControllerBase
{
    private readonly IEventLogWriter _eventLogWriter;
    private readonly ITimeSource _timeSource;

    public WorkflowController(IEventLogWriter eventLogWriter, ITimeSource timeSource)
    {
        _eventLogWriter = eventLogWriter;
        _timeSource = timeSource;
    }

    [HttpPost("hello")]
    public async Task<IActionResult> Hello(CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid().ToString("N");
        var now = _timeSource.NowUtc();

        var manifest = new Manifest(
            ManifestVersion: "0.1",
            RunId: runId,
            Seed: new SeedInfo(
                SeedId: "seed-1",
                Algorithm: "xoroshiro128**",
                Value: 123456789,
                Derivation: "static placeholder"),
            TimeSource: new TimeSourceInfo(
                Mode: "record",
                Source: "system-utc",
                ClockId: "clock-1",
                Precision: "utc-millis",
                Notes: "recorded via injected time source"),
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
            return Problem(
                detail: string.Join(" ", manifestErrors),
                statusCode: StatusCodes.Status500InternalServerError);
        }

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
