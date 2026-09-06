namespace Calory.Ai.Orchestrator.Options;

public sealed class McpOptions
{
    public const string SectionName = "Mcp";
    public string Endpoint { get; set; } = "http://localhost:5290/mcp";
}