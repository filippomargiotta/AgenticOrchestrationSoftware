namespace Aos.WebApi.Models;

public static class ManifestValidator
{
    public static IReadOnlyList<string> Validate(Manifest manifest)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.ManifestVersion))
        {
            errors.Add("ManifestVersion is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.RunId))
        {
            errors.Add("RunId is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Seed.SeedId))
        {
            errors.Add("Seed.SeedId is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Seed.Algorithm))
        {
            errors.Add("Seed.Algorithm is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.TimeSource.Mode))
        {
            errors.Add("TimeSource.Mode is required.");
        }
        else if (!manifest.TimeSource.Mode.Equals("record", StringComparison.OrdinalIgnoreCase) &&
                 !manifest.TimeSource.Mode.Equals("replay", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("TimeSource.Mode must be 'record' or 'replay'.");
        }

        if (string.IsNullOrWhiteSpace(manifest.TimeSource.Source))
        {
            errors.Add("TimeSource.Source is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.TimeSource.ClockId))
        {
            errors.Add("TimeSource.ClockId is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.TimeSource.Precision))
        {
            errors.Add("TimeSource.Precision is required.");
        }

        if (manifest.Models.Count == 0)
        {
            errors.Add("At least one ModelRef is required.");
        }

        if (manifest.Tools.Count == 0)
        {
            errors.Add("At least one ToolRef is required.");
        }

        if (manifest.PolicyDecisions.Count == 0)
        {
            errors.Add("At least one PolicyDecision is required.");
        }

        if (manifest.CompletedAtUtc is not null &&
            manifest.CompletedAtUtc.Value < manifest.StartedAtUtc)
        {
            errors.Add("CompletedAtUtc cannot be earlier than StartedAtUtc.");
        }

        return errors;
    }
}
