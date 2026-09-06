using Microsoft.Agents.AI;

namespace Calory.Ai.Orchestrator.Handlers
{
    public interface IChatService
    {
        IAsyncEnumerable<AgentResponseUpdate> StreamAsync(string message, string? conversationId, string accessToken, CancellationToken cancellationToken);
    }
}
