using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WeightLossTracker.Data;
using WeightLossTracker.Models;

namespace WeightLossTracker.Services;

public class GeminiApiException(string message) : Exception(message) { }

/// <summary>Result returned from a Gemini API call.</summary>
public record GeminiResult(string Text, int InputTokens, int OutputTokens, string Model);

/// <summary>
/// Calls the Google Gemini REST API and persists every interaction in <see cref="AiPromptLog"/>.
/// </summary>
public class GeminiService(
    IHttpClientFactory httpClientFactory,
    IOptions<GeminiOptions> options,
    AppDbContext db,
    ILogger<GeminiService> logger)
{
    private readonly GeminiOptions _options = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    /// <summary>Generates content and returns the result. Log ID is discarded.</summary>
    public async Task<GeminiResult> GenerateAsync(
        string prompt, string promptType, CancellationToken ct = default)
    {
        var (result, _) = await GenerateWithLogIdAsync(prompt, promptType, ct).ConfigureAwait(false);
        return result;
    }

    /// <summary>Generates content, persists the AI prompt log, and returns both the result and log ID.</summary>
    public async Task<(GeminiResult Result, int LogId)> GenerateWithLogIdAsync(
        string prompt, string promptType, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new GeminiApiException("Gemini API key not configured.");

        var (text, inputTokens, outputTokens) = await CallGeminiApiAsync(prompt, promptType, ct)
            .ConfigureAwait(false);

        var log = new AiPromptLog
        {
            CreatedAt = DateTime.UtcNow,
            PromptType = promptType,
            Prompt = prompt,
            Response = text,
            Model = _options.Model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };
        db.AiPromptLogs.Add(log);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return (new GeminiResult(text, inputTokens, outputTokens, _options.Model), log.Id);
    }

    private async Task<(string Text, int InputTokens, int OutputTokens)> CallGeminiApiAsync(
        string prompt, string promptType, CancellationToken ct)
    {
        var endpoint =
            $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent";

        var tokenLimit = promptType == "Exercise"
            ? _options.ExerciseMaxOutputTokens
            : _options.MaxOutputTokens;

        var body = JsonSerializer.Serialize(new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { maxOutputTokens = tokenLimit }
        });

        var http = httpClientFactory.CreateClient("Gemini");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Network error calling Gemini API");
            throw new GeminiApiException("Network error reaching Gemini API.");
        }

        var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new GeminiApiException("Rate limit reached — try again in a moment.");

        if (!response.IsSuccessStatusCode)
            throw new GeminiApiException($"Gemini API error ({(int)response.StatusCode}): {responseBody}");

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            int inputTokens = 0, outputTokens = 0;
            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                inputTokens = usage.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0;
                outputTokens = usage.TryGetProperty("candidatesTokenCount", out var ct2) ? ct2.GetInt32() : 0;
            }

            return (text, inputTokens, outputTokens);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse Gemini response: {Body}", responseBody);
            throw new GeminiApiException("Failed to parse Gemini response.");
        }
    }
}
