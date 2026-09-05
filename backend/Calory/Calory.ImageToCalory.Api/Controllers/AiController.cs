using Calory.ImageToCalory.Api.Models;
using Calory.ImageToCalory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calory.ImageToCalory.Api.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<AnalyzeImageResponse>> Analyze( [FromForm] AnalyzeImageRequest request, CancellationToken cancellationToken)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest(new { success = false, message = "Image is required." });
            }

            try
            {
                var result = await _aiService.AnalyzeImageAsync( request, cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
