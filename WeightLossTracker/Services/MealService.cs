using Microsoft.EntityFrameworkCore;
using WeightLossTracker.Data;
using WeightLossTracker.Models;

namespace WeightLossTracker.Services;

/// <summary>Handles meal logging queries and AI-powered nutrition advice.</summary>
public class MealService(AppDbContext db, GeminiService gemini)
{
    public string BuildMealAdvicePrompt(UserProfile profile, string question, List<MealLog> todayMeals)
    {
        var mealsDesc = todayMeals.Count > 0
            ? string.Join("\n", todayMeals.Select(m =>
                $"- {m.MealType}: {m.Description}{(m.Calories.HasValue ? $" ({m.Calories} cal)" : "")}"))
            : "No meals logged yet today.";

        return $"""
            You are a registered dietitian providing personalised nutrition advice.

            USER PROFILE:
            - Current Weight: {profile.StartingWeight} lbs → Goal: {profile.GoalWeight} lbs (lose {profile.StartingWeight - profile.GoalWeight} lbs)
            - Fitness Level: {profile.FitnessLevel}
            - Injuries: {profile.Injuries}
            - Goals: {profile.Goals}

            TODAY'S MEALS SO FAR:
            {mealsDesc}

            USER QUESTION: {question}

            Provide helpful, evidence-based nutrition advice. Be conversational and encouraging.
            Use ## for section headers and bullet points where appropriate.
            """;
    }

    public async Task<GeminiResult> GetNutritionAdviceAsync(string question, int profileId, CancellationToken ct = default)
    {
        var profile = await db.UserProfiles.FindAsync([profileId], ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User profile not found.");

        var today = DateTime.UtcNow.Date;
        var todayMeals = await db.MealLogs
            .AsNoTracking()
            .Where(m => m.UserProfileId == profileId && m.Date >= today && m.Date < today.AddDays(1))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var prompt = BuildMealAdvicePrompt(profile, question, todayMeals);
        return await gemini.GenerateAsync(prompt, "Meal", profileId, ct).ConfigureAwait(false);
    }
}
