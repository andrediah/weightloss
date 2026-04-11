using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WeightLossTracker.Data;
using WeightLossTracker.Models;

namespace WeightLossTracker.Services;

public class GeminiApiException(string message) : Exception(message) { }

public class GeminiResult
{
    public string Text { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public string Model { get; set; } = "";
}

public class GeminiService(IConfiguration config, AppDbContext db, ILogger<GeminiService> logger)
{
    private readonly string _apiKey = config["Gemini:ApiKey"] ?? "";
    private readonly string _model = config["Gemini:Model"] ?? "gemini-2.5-flash";
    private readonly int _maxOutputTokens = int.TryParse(config["Gemini:MaxOutputTokens"], out var t) ? t : 2048;
    private readonly int _exerciseMaxOutputTokens = int.TryParse(config["Gemini:ExerciseMaxOutputTokens"], out var e) ? e : 4096;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<GeminiResult> GenerateAsync(string prompt, string promptType, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new GeminiApiException("Gemini API key not configured.");

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";

        var tokenLimit = promptType == "Exercise" ? _exerciseMaxOutputTokens : _maxOutputTokens;
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { maxOutputTokens = tokenLimit }
        };

        var json = JsonSerializer.Serialize(body);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

        HttpResponseMessage response;
        try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            response = await http.PostAsync(endpoint, content, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Network error calling Gemini API");
            throw new GeminiApiException("Network error reaching Gemini API.");
        }

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            throw new GeminiApiException("Rate limit reached — try again in a moment.");

        if (!response.IsSuccessStatusCode)
            throw new GeminiApiException($"Gemini API error ({(int)response.StatusCode}): {responseBody}");

        string text;
        int inputTokens = 0, outputTokens = 0;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                inputTokens = usage.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0;
                outputTokens = usage.TryGetProperty("candidatesTokenCount", out var ct2) ? ct2.GetInt32() : 0;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse Gemini response: {Body}", responseBody);
            throw new GeminiApiException("Failed to parse Gemini response.");
        }

        // Log to DB
        var log = new AiPromptLog
        {
            CreatedAt = DateTime.UtcNow,
            PromptType = promptType,
            Prompt = prompt,
            Response = text,
            Model = _model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };
        db.AiPromptLogs.Add(log);
        await db.SaveChangesAsync(ct);

        return new GeminiResult
        {
            Text = text,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            Model = _model,
            // Store log id for caller
        };
    }

    // Returns (result, logId)
    public async Task<(GeminiResult Result, int LogId)> GenerateWithLogIdAsync(string prompt, string promptType, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new GeminiApiException("Gemini API key not configured.");

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";

        var tokenLimit = promptType == "Exercise" ? _exerciseMaxOutputTokens : _maxOutputTokens;
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { maxOutputTokens = tokenLimit }
        };

        var json = JsonSerializer.Serialize(body);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

        HttpResponseMessage response;
        try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            response = await http.PostAsync(endpoint, content, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Network error calling Gemini API");
            throw new GeminiApiException("Network error reaching Gemini API.");
        }

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            throw new GeminiApiException("Rate limit reached — try again in a moment.");

        if (!response.IsSuccessStatusCode)
            throw new GeminiApiException($"Gemini API error ({(int)response.StatusCode}): {responseBody}");

        string text;
        int inputTokens = 0, outputTokens = 0;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                inputTokens = usage.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0;
                outputTokens = usage.TryGetProperty("candidatesTokenCount", out var ct2) ? ct2.GetInt32() : 0;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse Gemini response: {Body}", responseBody);
            throw new GeminiApiException("Failed to parse Gemini response.");
        }

        var log = new AiPromptLog
        {
            CreatedAt = DateTime.UtcNow,
            PromptType = promptType,
            Prompt = prompt,
            Response = text,
            Model = _model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };
        db.AiPromptLogs.Add(log);
        await db.SaveChangesAsync(ct);

        return (new GeminiResult { Text = text, InputTokens = inputTokens, OutputTokens = outputTokens, Model = _model }, log.Id);
    }
}
