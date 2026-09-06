using Calory.Ai.Orchestrator.Handlers;
using Calory.Ai.Orchestrator.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calory.Ai.Orchestrator.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public sealed class ChatController(
    IAiOrchestrator orchestrator) : ControllerBase
    {
        [HttpPost("stream")]
        public async Task Stream( [FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";

            await foreach (var update in orchestrator.StreamAsync(request.Message, request.ConversationId, cancellationToken))
            {
                if (string.IsNullOrEmpty(update.Text))
                    continue;

                await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(update.Text)}\n\n", cancellationToken);

                await Response.Body.FlushAsync(cancellationToken);
            }

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
