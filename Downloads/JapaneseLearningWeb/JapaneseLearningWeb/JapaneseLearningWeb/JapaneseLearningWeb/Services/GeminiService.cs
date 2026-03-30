using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> ChatAsync(string userMessage, string level)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"];

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var prompt = BuildPrompt(userMessage, level);

        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Gemini API error: " + responseText);

        using var doc = JsonDocument.Parse(responseText);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }

    private string BuildPrompt(string userMessage, string level)
    {
        return $"""
Bạn là giáo viên tiếng Nhật trình độ {level}.
Nhiệm vụ:
- Trả lời NGẮN GỌN
- Ưu tiên tiếng Nhật
- Nếu cần, giải thích ngắn bằng tiếng Việt
- Không dùng thuật ngữ khó

Câu hỏi của học viên:
{userMessage}
""";
    }
}
