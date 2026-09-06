using Microsoft.Agents.AI;

namespace Calory.Ai.Orchestrator.Handlers
{
    public interface IAiOrchestrator
    {
        IAsyncEnumerable<AgentResponseUpdate> StreamAsync(string message, string? conversationId, CancellationToken cancellationToken);
    }
}
