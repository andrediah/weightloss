using Microsoft.Extensions.Options;

namespace WeightLossTracker.Services;

/// <summary>Registers all application services into the DI container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeightLossTrackerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GeminiOptions>(configuration.GetSection("Gemini"));

        // Named HTTP client — base address and default headers (except API key,
        // which is added per-request to support degraded mode with an empty key).
        services.AddHttpClient("Gemini");

        services.AddScoped<GeminiService>();
        services.AddScoped<ExerciseService>();
        services.AddScoped<MealService>();
        services.AddSingleton<AuthService>();

        return services;
    }
}
