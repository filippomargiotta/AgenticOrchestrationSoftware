namespace Aos.WebApi.Options;

public sealed class HelloWorkflowOptions
{
    public const string SectionName = "HelloWorkflow";

    public List<HelloWorkflowModelOptions> Models { get; set; } = [];
    public List<HelloWorkflowToolOptions> Tools { get; set; } = [];
    public List<HelloWorkflowPolicyOptions> PolicyDecisions { get; set; } = [];
}

public sealed class HelloWorkflowModelOptions
{
    public string ModelId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public sealed class HelloWorkflowToolOptions
{
    public string ToolId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public sealed class HelloWorkflowPolicyOptions
{
    public string PolicyId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
