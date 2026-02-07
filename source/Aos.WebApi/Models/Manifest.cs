namespace Aos.WebApi.Models;

public sealed record Manifest(
    string ManifestVersion,
    string RunId,
    SeedInfo Seed,
    TimeSourceInfo TimeSource,
    IReadOnlyList<ModelRef> Models,
    IReadOnlyList<ToolRef> Tools,
    IReadOnlyList<PolicyDecision> PolicyDecisions,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc
);

public sealed record SeedInfo(
    string SeedId,
    string Algorithm,
    long Value,
    string? Derivation
);

public sealed record TimeSourceInfo(
    string Mode,
    string Source,
    string ClockId,
    string Precision,
    string? Notes
);

public sealed record ModelRef(
    string ModelId,
    string Provider,
    string Version
);

public sealed record ToolRef(
    string ToolId,
    string Version
);

public sealed record PolicyDecision(
    string PolicyId,
    string Decision,
    string? Reason
);
