using Microsoft.Agents.AI;
using System.Collections.Concurrent;

namespace Calory.Ai.Orchestrator.Handlers
{
    public sealed class AiOrchestrator: IAiOrchestrator
    {
        private readonly AIAgent _agent;
        public AiOrchestrator(AIAgent agent)
        {
            _agent = agent;
        }

        private readonly ConcurrentDictionary<string, Lazy<Task<AgentSession>>> sessions = new();

        public async IAsyncEnumerable<AgentResponseUpdate> StreamAsync(string message, string? conversationId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var id = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString("N") : conversationId;

            var session = await sessions.GetOrAdd(id, _ => new Lazy<Task<AgentSession>>(() => _agent.CreateSessionAsync(cancellationToken).AsTask())).Value;

            await foreach (var update in _agent.RunStreamingAsync(message, session, cancellationToken: cancellationToken))
            {
                yield return update;
            }
        }
    }
}
