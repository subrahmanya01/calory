using Calory.Ai.Orchestrator.Handlers;
using Calory.Ai.Orchestrator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Calory.Ai.Orchestrator.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public sealed class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task Stream([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            var authorization = Request.Headers[HeaderNames.Authorization].ToString();
            var accessToken = authorization["Bearer ".Length..].Trim();

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";

            await foreach (var update in _chatService.StreamAsync(request.Message, request.ConversationId, accessToken, cancellationToken))
            {
                if (string.IsNullOrEmpty(update.Text)) continue;

                await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(update.Text)}\n\n", cancellationToken);

                await Response.Body.FlushAsync(cancellationToken);
            }

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
