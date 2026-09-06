using System.ClientModel;
using Calory.Ai.Orchestrator.Options;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;

namespace Calory.Ai.Orchestrator.Handlers;

public sealed class CaloryAgentRuntime : IAsyncDisposable
{
    private readonly McpClient mcpClient;

    private CaloryAgentRuntime(AIAgent agent, McpClient mcpClient)
    {
        Agent = agent;
        this.mcpClient = mcpClient;
    }

    public AIAgent Agent { get; }

    public static async Task<CaloryAgentRuntime> CreateAsync(
        AzureOpenAIOptions openAiOptions,
        McpOptions mcpOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(openAiOptions);
        ArgumentNullException.ThrowIfNull(mcpOptions);

        if (string.IsNullOrWhiteSpace(openAiOptions.Endpoint))
            throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured.");
        if (string.IsNullOrWhiteSpace(openAiOptions.DeploymentName))
            throw new InvalidOperationException("AzureOpenAI:DeploymentName is not configured.");
        if (string.IsNullOrWhiteSpace(mcpOptions.Endpoint))
            throw new InvalidOperationException("Mcp:Endpoint is not configured.");

        var projectEndpoint = openAiOptions.Endpoint.TrimEnd('/');
        projectEndpoint = projectEndpoint
            .Replace("/openai/v1/responses", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("/openai/v1", string.Empty, StringComparison.OrdinalIgnoreCase);

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(openAiOptions.ApiKey ?? string.Empty),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{projectEndpoint}/openai/v1/")
            });

        IChatClient chatClient = openAiClient
            .GetChatClient(openAiOptions.DeploymentName)
            .AsIChatClient();

        var mcpTransport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(mcpOptions.Endpoint),
                TransportMode = HttpTransportMode.StreamableHttp
            });

        var mcpClient = await McpClient.CreateAsync(mcpTransport, cancellationToken: cancellationToken);
        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);

        var agent = chatClient.AsAIAgent(
            instructions: """
                You are the AI assistant for the Calory application.

                You help users understand and manage nutrition, meals, goals,
                calories, and trends.

                You have access to tools provided by the Calory MCP server.
                Use those tools whenever application data is required.
                Never invent user-specific data when a tool can retrieve it.
                Never claim that data was saved unless a tool confirms it.
                Keep responses concise, clear, and useful.
                """,
            name: "CaloryAssistant",
            tools: mcpTools.Cast<AITool>().ToList());

        return new CaloryAgentRuntime(agent, mcpClient);
    }

    public ValueTask DisposeAsync() => mcpClient.DisposeAsync();
}
