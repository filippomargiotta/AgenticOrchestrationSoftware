namespace Aos.WebApi.Options;

public sealed class EventLogOptions
{
    public const string SectionName = "EventLog";

    public string Directory { get; set; } = "data";

    public string FileName { get; set; } = "eventlog.jsonl";
}
