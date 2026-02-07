using Aos.WebApi.Models;
using Xunit;

namespace Aos.WebApi.Tests;

public sealed class ManifestValidatorTests
{
    [Fact]
    public void ValidManifest_PassesValidation()
    {
        var now = DateTimeOffset.UtcNow;
        var manifest = new Manifest(
            ManifestVersion: "0.1",
            RunId: "run-1",
            Seed: new SeedInfo(
                SeedId: "seed-1",
                Algorithm: "xoroshiro128**",
                Value: 123,
                Derivation: "static test"),
            TimeSource: new TimeSourceInfo(
                Mode: "record",
                Source: "system-utc",
                ClockId: "clock-1",
                Precision: "utc-millis",
                Notes: null),
            Models: new[] { new ModelRef("model-1", "local", "0.0") },
            Tools: new[] { new ToolRef("tool-1", "0.0") },
            PolicyDecisions: new[] { new PolicyDecision("policy-1", "allow", null) },
            StartedAtUtc: now,
            CompletedAtUtc: now);

        var errors = ManifestValidator.Validate(manifest);

        Assert.Empty(errors);
    }

    [Fact]
    public void MissingFields_FailsValidation()
    {
        var manifest = new Manifest(
            ManifestVersion: "",
            RunId: "",
            Seed: new SeedInfo(
                SeedId: "",
                Algorithm: "",
                Value: 0,
                Derivation: null),
            TimeSource: new TimeSourceInfo(
                Mode: "invalid",
                Source: "",
                ClockId: "",
                Precision: "",
                Notes: null),
            Models: Array.Empty<ModelRef>(),
            Tools: Array.Empty<ToolRef>(),
            PolicyDecisions: Array.Empty<PolicyDecision>(),
            StartedAtUtc: DateTimeOffset.UtcNow,
            CompletedAtUtc: null);

        var errors = ManifestValidator.Validate(manifest);

        Assert.NotEmpty(errors);
    }
}
