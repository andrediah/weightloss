namespace WeightLossTracker.Services;

/// <summary>Typed configuration for the Google Gemini API.</summary>
public record GeminiOptions
{
    public string ApiKey { get; init; } = "";
    public string Model { get; init; } = "gemini-2.5-flash";
    public int MaxOutputTokens { get; init; } = 2048;
    public int ExerciseMaxOutputTokens { get; init; } = 4096;
}
