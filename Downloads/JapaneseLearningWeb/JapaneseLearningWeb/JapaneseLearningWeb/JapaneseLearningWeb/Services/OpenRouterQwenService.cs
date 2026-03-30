using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace JapaneseLearningWeb.Services
{
    public class OpenRouterQwenService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenRouterSettings _settings;

        public OpenRouterQwenService(HttpClient httpClient, IOptions<OpenRouterSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<string> AskAsync(string prompt)
        {
            var endpoint = "https://openrouter.ai/api/v1/chat/completions";

            var payload = new
            {
                model = _settings.Model ?? "mistralai/mistral-7b-instruct",
                messages = new[]
     {
        new { role = "user", content = prompt }
    },
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            request.Headers.Add("HTTP-Referer", "https://localhost"); // Bắt buộc nếu dùng local
            request.Headers.Add("X-Title", "QwenTranslator");

            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"[Lỗi API] {response.StatusCode} - {response.ReasonPhrase}\n{result}";
            }

            dynamic json = JsonConvert.DeserializeObject(result);
            return json.choices[0].message.content;
        }
    }
}
