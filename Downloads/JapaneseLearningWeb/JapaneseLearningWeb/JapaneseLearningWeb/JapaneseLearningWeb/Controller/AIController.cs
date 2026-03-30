using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly GeminiService _gemini;

    public AiController(GeminiService gemini)
    {
        _gemini = gemini;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("Message is empty");

        var reply = await _gemini.ChatAsync(req.Message, req.Level);

        return Ok(new { reply });
    }
}
