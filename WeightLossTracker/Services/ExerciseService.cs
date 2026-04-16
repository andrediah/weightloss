using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using WeightLossTracker.Data;
using WeightLossTracker.Models;

namespace WeightLossTracker.Services;

/// <summary>Handles AI workout generation and exercise history management.</summary>
public class ExerciseService(AppDbContext db, GeminiService gemini)
{
    private static readonly string[] Categories = ["Cardio", "Strength", "Flexibility"];
    private static readonly string[] DayNames =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    public string GetCategory(int dayIndex) => Categories[dayIndex % Categories.Length];

    public string BuildExercisePrompt(
        UserProfile profile,
        WorkoutScheduleDay day,
        string category,
        List<WorkoutScheduleDay> fullSchedule)
    {
        var scheduleDesc = string.Join(", ",
            fullSchedule.Select(s => $"{DayNames[s.DayOfWeek]}: {s.Location}"));

        return $"""
            You are a certified personal trainer creating a safe, personalised workout plan.

            USER PROFILE:
            - Fitness Level: {profile.FitnessLevel}
            - Injuries: {profile.Injuries}
            - Goal: {profile.Goals}
            - Starting Weight: {profile.StartingWeight} lbs → Goal: {profile.GoalWeight} lbs

            WEEKLY SCHEDULE: {scheduleDesc}

            TODAY'S WORKOUT:
            - Day: {DayNames[day.DayOfWeek]}
            - Location: {day.Location}
            - Focus Category: {category}

            CRITICAL SAFETY RULES:
            - Absolutely NO exercises that load the neck (no overhead press behind neck, no neck rolls, no heavy shrugs)
            - Absolutely NO exercises that compress or strain the lower back (no deadlifts, no heavy squats, no good mornings)
            - All exercises must be beginner-friendly
            - Include warm-up and cool-down

            Please provide a detailed {category} workout plan for {DayNames[day.DayOfWeek]} at {day.Location}.

            OUTPUT FORMAT:
            Use ## headers for Warm-Up, Main Workout, and Cool-Down sections.
            For each exercise, list on a single bullet line:
            - **Exercise Name** — sets × reps (or duration for timed exercises), rest period between sets, then a brief form cue.
            Example: - **Bodyweight Squats** — 3 × 12, 60 s rest. Keep weight in heels, chest up.
            Example: - **Jumping Jacks** — 3 × 45 s, 30 s rest. Land softly on balls of feet.
            Do NOT give general advice or motivational filler — only the structured exercise list.
            """;
    }

    public async Task<ExerciseSuggestion> GenerateDayWorkoutAsync(
        int dayOfWeek, int profileId, CancellationToken ct = default)
    {
        var profile = await db.UserProfiles.FindAsync([profileId], ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User profile not found.");

        var day = await db.WorkoutScheduleDays
            .FirstOrDefaultAsync(s => s.UserProfileId == profileId && s.DayOfWeek == dayOfWeek, ct)
            .ConfigureAwait(false)
            ?? throw new ArgumentException($"No schedule entry for day {dayOfWeek}.");

        if (day.Location == "Rest")
            throw new ArgumentException($"Day {dayOfWeek} is set to Rest — no workout to generate.");

        var fullSchedule = await db.WorkoutScheduleDays
            .Where(s => s.UserProfileId == profileId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var activeDays = fullSchedule
            .Where(s => s.Location != "Rest")
            .OrderBy(s => s.DayOfWeek)
            .ToList();

        var dayIndex = activeDays.FindIndex(s => s.DayOfWeek == dayOfWeek);
        var category = GetCategory(dayIndex >= 0 ? dayIndex : 0);

        var prompt = BuildExercisePrompt(profile, day, category, fullSchedule);
        var (result, logId) = await gemini.GenerateWithLogIdAsync(prompt, "Exercise", profileId, ct)
            .ConfigureAwait(false);

        var suggestion = new ExerciseSuggestion
        {
            CreatedAt = DateTime.UtcNow,
            UserProfileId = profileId,
            DayOfWeek = dayOfWeek,
            Location = day.Location,
            Category = category,
            Content = result.Text,
            AiPromptLogId = logId
        };

        db.ExerciseSuggestions.Add(suggestion);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return suggestion;
    }

    public async IAsyncEnumerable<(ExerciseSuggestion Suggestion, string? Error)> GenerateWeekWorkoutsAsync(
        int profileId, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var profile = await db.UserProfiles.FindAsync([profileId], ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User profile not found.");

        var fullSchedule = await db.WorkoutScheduleDays
            .Where(s => s.UserProfileId == profileId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var activeDays = fullSchedule
            .Where(s => s.Location != "Rest")
            .OrderBy(s => s.DayOfWeek)
            .ToList();

        for (int i = 0; i < activeDays.Count; i++)
        {
            var day = activeDays[i];
            var category = GetCategory(i);
            var prompt = BuildExercisePrompt(profile, day, category, fullSchedule);

            ExerciseSuggestion? suggestion = null;
            string? error = null;

            try
            {
                var (result, logId) = await gemini
                    .GenerateWithLogIdAsync(prompt, "Exercise", profileId, ct)
                    .ConfigureAwait(false);

                suggestion = new ExerciseSuggestion
                {
                    CreatedAt = DateTime.UtcNow,
                    UserProfileId = profileId,
                    DayOfWeek = day.DayOfWeek,
                    Location = day.Location,
                    Category = category,
                    Content = result.Text,
                    AiPromptLogId = logId
                };

                db.ExerciseSuggestions.Add(suggestion);
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            catch (GeminiApiException ex)
            {
                error = ex.Message;
            }

            yield return (suggestion!, error);

            if (error != null) yield break;
        }
    }
}
