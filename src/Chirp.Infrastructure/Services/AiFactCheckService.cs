using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Chirp.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Chirp.Infrastructure.Services;

public class AiFactCheckService : IAiFactCheckService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _client;

    public AiFactCheckService(IConfiguration config)
    {
        _config = config;
        _client = new HttpClient();
    }

    public async Task<string> FactCheckAsync(string text)
    {
        var apiKey = _config["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
            return "AI not configured (no API key).";

        text = text.Length > 300 ? text[..300] : text;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = "gpt-4.1-mini",
            messages = new[]
            {
                new { role = "system", content = "Classify if a statement is Fact, Opinion, Prediction or Unverifiable." },
                new { role = "user", content = text }
            },
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            return $"AI error ({response.StatusCode}): {error}";
        }

        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return result.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}