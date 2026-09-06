namespace Calory.Ai.Orchestrator.Options
{
    public sealed class AzureOpenAIOptions
    {
        public string Endpoint { get; set; } = string.Empty;

        public string DeploymentName { get; set; } = string.Empty;

        public string? ApiKey { get; set; }
    }
}
