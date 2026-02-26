using System.Diagnostics;
using Aos.WebApi.Models;
using Aos.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aos.WebApi.Controllers;

[ApiController]
[Route("workflow")]
public class WorkflowController : ControllerBase
{
    private readonly IEventLogWriter _eventLogWriter;
    private readonly ISeedProvider _seedProvider;
    private readonly ITimeSource _timeSource;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IEventLogWriter eventLogWriter,
        ISeedProvider seedProvider,
        ITimeSource timeSource,
        ILogger<WorkflowController> logger)
    {
        _eventLogWriter = eventLogWriter;
        _seedProvider = seedProvider;
        _timeSource = timeSource;
        _logger = logger;
    }

    [HttpPost("hello")]
    public async Task<IActionResult> Hello(CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid().ToString("N");
        var now = _timeSource.NowUtc();
        var seed = _seedProvider.GetLockedSeed(runId);
        var timeSourceInfo = _timeSource.Describe();

        _logger.LogInformation("Starting workflow hello for run {RunId}", runId);
        Activity.Current?.SetTag("aos.run_id", runId);
        Activity.Current?.SetTag("aos.workflow", "hello");

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
            _logger.LogError(
                "Manifest validation failed for run {RunId}: {Errors}",
                runId,
                string.Join(" ", manifestErrors));
            return Problem(
                detail: string.Join(" ", manifestErrors),
                statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation(
            "Manifest validated for run {RunId} with version {Version}",
            runId,
            manifest.ManifestVersion);

        var entry = new Aos.WebApi.Models.EventLogEntry(
            RunId: runId,
            EventType: "workflow.hello",
            Data: new { Message = "hello", ManifestVersion = manifest.ManifestVersion },
            OccurredAtUtc: now);

        await _eventLogWriter.WriteAsync(entry, cancellationToken);

        _logger.LogInformation("Completed workflow hello for run {RunId}", runId);

        return Ok(new
        {
            RunId = runId,
            Manifest = manifest
        });
    }
}
