using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLossTracker.Data;
using WeightLossTracker.Models;
using WeightLossTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ─────────────────────────────────────────────────────────────────
var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbFolder = Path.Combine(appData, "WeightLossTracker");
Directory.CreateDirectory(dbFolder);
var dbPath = Path.Combine(dbFolder, "tracker.db");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

// ─── Application Services ─────────────────────────────────────────────────────
builder.Services.AddWeightLossTrackerServices(builder.Configuration);

var app = builder.Build();

// ─── Auto-migrate on startup ──────────────────────────────────────────────────
// If the DB file exists but no migrations have been applied, delete it so
// Migrate() can start from a clean slate (handles leftover partial-state DBs).
using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (File.Exists(dbPath))
    {
        bool needsReset = false;
        using (var probe = new SqliteConnection($"Data Source={dbPath}"))
        {
            probe.Open();
            using var cmd = probe.CreateCommand();

            cmd.CommandText =
                "SELECT COUNT(*) FROM sqlite_master " +
                "WHERE type='table' AND name='__EFMigrationsHistory'";
            bool historyTableExists = (long)cmd.ExecuteScalar()! > 0;

            long appliedCount = 0;
            if (historyTableExists)
            {
                cmd.CommandText = "SELECT COUNT(*) FROM \"__EFMigrationsHistory\"";
                appliedCount = (long)cmd.ExecuteScalar()!;
            }

            needsReset = appliedCount == 0;
        }

        if (needsReset)
        {
            SqliteConnection.ClearAllPools();
            startupLogger.LogWarning(
                "Partial-state database detected (no completed migrations). Deleting and recreating.");
            File.Delete(dbPath);
            var walPath = dbPath + "-wal";
            var shmPath = dbPath + "-shm";
            if (File.Exists(walPath)) File.Delete(walPath);
            if (File.Exists(shmPath)) File.Delete(shmPath);
        }
    }

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ─── Degraded-mode warning ────────────────────────────────────────────────────
var apiKey = app.Configuration["Gemini:ApiKey"] ?? "";
if (string.IsNullOrWhiteSpace(apiKey))
    app.Logger.LogWarning(
        "Gemini:ApiKey is not configured. AI features will be unavailable (degraded mode).");

app.UseDefaultFiles();
app.UseStaticFiles();

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
app.MapGet("/api/dashboard", async (AppDbContext db) =>
{
    var profile = await db.UserProfiles.FindAsync(1);
    var entries = await db.WeightEntries.OrderBy(w => w.Date).ToListAsync();

    double? currentWeight = entries.LastOrDefault()?.Weight;
    double lostSoFar = currentWeight.HasValue
        ? Math.Max(0, (profile?.StartingWeight ?? 215) - currentWeight.Value)
        : 0;
    double toGoal = currentWeight.HasValue
        ? Math.Max(0, currentWeight.Value - (profile?.GoalWeight ?? 190))
        : 25;
    int daysLogged = entries.Count;
    double target = profile?.GoalWeight ?? 190;
    double start = profile?.StartingWeight ?? 215;
    double progressPct = start == target
        ? 0
        : Math.Min(100, Math.Round(lostSoFar / (start - target) * 100, 1));

    var labels = entries.Select(e => e.Date.ToString("yyyy-MM-dd")).ToList();
    var weights = entries.Select(e => e.Weight).ToList();

    // Linear regression trend line
    List<double?> trendLine = [];
    if (weights.Count >= 2)
    {
        int n = weights.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            sumX += i; sumY += weights[i];
            sumXY += i * weights[i]; sumX2 += i * i;
        }
        double denom = n * sumX2 - sumX * sumX;
        if (denom != 0)
        {
            double slope = (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;
            for (int i = 0; i < n; i++)
                trendLine.Add(Math.Round(intercept + slope * i, 2));
        }
    }

    return Results.Ok(new
    {
        currentWeight,
        lostSoFar = Math.Round(lostSoFar, 1),
        toGoal = Math.Round(toGoal, 1),
        daysLogged,
        progressPct,
        goalWeight = target,
        chart = new { labels, weights, trendLine }
    });
});

// ─── WEIGHT ───────────────────────────────────────────────────────────────────
app.MapGet("/api/weight", async (AppDbContext db) =>
    Results.Ok(await db.WeightEntries.OrderByDescending(w => w.Date).ToListAsync()));

app.MapPost("/api/weight", async (AppDbContext db, WeightEntryRequest req) =>
{
    if (req.Weight < 50 || req.Weight > 500)
        return Results.BadRequest("Weight must be between 50 and 500 lbs.");

    var today = DateTime.UtcNow.Date;
    var existing = await db.WeightEntries.FirstOrDefaultAsync(w => w.Date == today);
    if (existing != null)
    {
        existing.Weight = req.Weight;
        existing.Notes = req.Notes;
        await db.SaveChangesAsync();
        return Results.Ok(existing);
    }

    var entry = new WeightEntry { Date = today, Weight = req.Weight, Notes = req.Notes };
    db.WeightEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

app.MapPut("/api/weight/{id:int}", async (AppDbContext db, int id, WeightEntryRequest req) =>
{
    if (req.Weight < 50 || req.Weight > 500)
        return Results.BadRequest("Weight must be between 50 and 500 lbs.");

    var entry = await db.WeightEntries.FindAsync(id);
    if (entry == null) return Results.NotFound();
    entry.Weight = req.Weight;
    entry.Notes = req.Notes;
    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

app.MapDelete("/api/weight/{id:int}", async (AppDbContext db, int id) =>
{
    var entry = await db.WeightEntries.FindAsync(id);
    if (entry == null) return Results.NotFound();
    db.WeightEntries.Remove(entry);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ─── SCHEDULE ─────────────────────────────────────────────────────────────────
app.MapGet("/api/schedule", async (AppDbContext db) =>
    Results.Ok(await db.WorkoutScheduleDays.OrderBy(s => s.DayOfWeek).ToListAsync()));

app.MapPut("/api/schedule", async (AppDbContext db, List<ScheduleUpdateItem> items) =>
{
    foreach (var item in items)
    {
        var day = await db.WorkoutScheduleDays
            .FirstOrDefaultAsync(s => s.DayOfWeek == item.DayOfWeek);
        if (day != null) day.Location = item.Location;
    }
    await db.SaveChangesAsync();
    return Results.Ok(await db.WorkoutScheduleDays.OrderBy(s => s.DayOfWeek).ToListAsync());
});

// ─── EXERCISE ─────────────────────────────────────────────────────────────────
app.MapPost("/api/exercise/generate-day", async (
    ExerciseService svc, GeminiService gemini, GenerateDayRequest req) =>
{
    if (!gemini.IsConfigured)
        return Results.Problem("Gemini API key not configured.", statusCode: 503);
    if (req.DayOfWeek < 0 || req.DayOfWeek > 6)
        return Results.BadRequest("dayOfWeek must be 0–6.");
    try
    {
        var suggestion = await svc.GenerateDayWorkoutAsync(req.DayOfWeek);
        return Results.Ok(suggestion);
    }
    catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
    catch (GeminiApiException ex) { return Results.Problem(ex.Message, statusCode: 502); }
});

app.MapPost("/api/exercise/generate-week", async (
    ExerciseService svc, GeminiService gemini, HttpResponse response) =>
{
    if (!gemini.IsConfigured)
    {
        response.StatusCode = 503;
        await response.WriteAsJsonAsync(new { error = "Gemini API key not configured." });
        return;
    }

    var results = new List<object>();
    await foreach (var (suggestion, error) in svc.GenerateWeekWorkoutsAsync())
    {
        if (error != null)
        {
            results.Add(new { error });
            break;
        }
        results.Add(new
        {
            id = suggestion.Id,
            dayOfWeek = suggestion.DayOfWeek,
            location = suggestion.Location,
            category = suggestion.Category,
            content = suggestion.Content,
            createdAt = suggestion.CreatedAt
        });
    }
    await response.WriteAsJsonAsync(results);
});

app.MapGet("/api/exercise/history", async (AppDbContext db, int? dayOfWeek) =>
{
    var query = db.ExerciseSuggestions.AsQueryable();
    if (dayOfWeek.HasValue) query = query.Where(e => e.DayOfWeek == dayOfWeek.Value);
    return Results.Ok(await query.OrderByDescending(e => e.CreatedAt).ToListAsync());
});

app.MapDelete("/api/exercise/history/{id:int}", async (AppDbContext db, int id) =>
{
    var suggestion = await db.ExerciseSuggestions
        .Include(e => e.AiPromptLog)
        .FirstOrDefaultAsync(e => e.Id == id);
    if (suggestion == null) return Results.NotFound();
    var log = suggestion.AiPromptLog;
    db.ExerciseSuggestions.Remove(suggestion);
    if (log != null) db.AiPromptLogs.Remove(log);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ─── MEALS ────────────────────────────────────────────────────────────────────
app.MapGet("/api/meals/today", async (AppDbContext db) =>
{
    var today = DateTime.UtcNow.Date;
    return Results.Ok(await db.MealLogs
        .Where(m => m.Date >= today && m.Date < today.AddDays(1))
        .OrderBy(m => m.Date)
        .ToListAsync());
});

app.MapPost("/api/meals", async (AppDbContext db, MealLogRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.MealType) || string.IsNullOrWhiteSpace(req.Description))
        return Results.BadRequest("MealType and Description are required.");

    var meal = new MealLog
    {
        Date = DateTime.UtcNow,
        MealType = req.MealType,
        Description = req.Description,
        Calories = req.Calories,
        Notes = req.Notes
    };
    db.MealLogs.Add(meal);
    await db.SaveChangesAsync();
    return Results.Ok(meal);
});

app.MapDelete("/api/meals/{id:int}", async (AppDbContext db, int id) =>
{
    var meal = await db.MealLogs.FindAsync(id);
    if (meal == null) return Results.NotFound();
    db.MealLogs.Remove(meal);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/meals/advice", async (
    MealService mealSvc, GeminiService gemini, MealAdviceRequest req) =>
{
    if (!gemini.IsConfigured)
        return Results.Problem("Gemini API key not configured.", statusCode: 503);
    if (string.IsNullOrWhiteSpace(req.Question))
        return Results.BadRequest("Question is required.");
    try
    {
        var result = await mealSvc.GetNutritionAdviceAsync(req.Question);
        return Results.Ok(new
        {
            advice = result.Text,
            inputTokens = result.InputTokens,
            outputTokens = result.OutputTokens
        });
    }
    catch (GeminiApiException ex) { return Results.Problem(ex.Message, statusCode: 502); }
});

// ─── AI HISTORY ───────────────────────────────────────────────────────────────
app.MapGet("/api/ai-history", async (AppDbContext db, string? type) =>
{
    var query = db.AiPromptLogs.AsQueryable();
    if (!string.IsNullOrWhiteSpace(type)) query = query.Where(l => l.PromptType == type);
    return Results.Ok(await query.OrderByDescending(l => l.CreatedAt).ToListAsync());
});

app.MapDelete("/api/ai-history/{id:int}", async (AppDbContext db, int id) =>
{
    var log = await db.AiPromptLogs.FindAsync(id);
    if (log == null) return Results.NotFound();
    var linked = await db.ExerciseSuggestions.AnyAsync(e => e.AiPromptLogId == id);
    if (linked)
        return Results.Conflict(
            "A linked ExerciseSuggestion exists. Delete the exercise history entry first.");
    db.AiPromptLogs.Remove(log);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run("http://localhost:5000");

// ─── REQUEST DTOs ─────────────────────────────────────────────────────────────
record WeightEntryRequest(double Weight, string? Notes);
record ScheduleUpdateItem(int DayOfWeek, string Location);
record GenerateDayRequest(int DayOfWeek);
record MealLogRequest(string MealType, string Description, int? Calories, string? Notes);
record MealAdviceRequest(string Question);
