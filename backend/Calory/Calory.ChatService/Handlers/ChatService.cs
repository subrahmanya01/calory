using Microsoft.Agents.AI;
using Calory.Ai.Orchestrator.Options;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Calory.Ai.Orchestrator.Handlers
{
    public sealed class ChatService: IChatService
    {
        private readonly AzureOpenAIOptions openAiOptions;
        private readonly McpOptions mcpOptions;

        public ChatService(IOptions<AzureOpenAIOptions> openAiOptions, IOptions<McpOptions> mcpOptions)
        {
            this.openAiOptions = openAiOptions.Value;
            this.mcpOptions = mcpOptions.Value;
        }

        private readonly ConcurrentDictionary<string, Lazy<Task<AgentSession>>> sessions = new();

        public async IAsyncEnumerable<AgentResponseUpdate> StreamAsync(string message, string? conversationId, string accessToken, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using var runtime = await CaloryAgentRuntime.CreateAsync(openAiOptions, mcpOptions, accessToken, cancellationToken);
            var session = await runtime.Agent.CreateSessionAsync(cancellationToken);

            await foreach (var update in runtime.Agent.RunStreamingAsync(message, session, cancellationToken: cancellationToken))
            {
                yield return update;
            }
        }
    }
}
