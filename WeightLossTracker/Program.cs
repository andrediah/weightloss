using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WeightLossTracker.Data;
using WeightLossTracker.Models;
using WeightLossTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ─────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    if (string.IsNullOrEmpty(appData))
        appData = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/tmp", ".local", "share");
    var dbFolder = Path.Combine(appData, "WeightLossTracker");
    Directory.CreateDirectory(dbFolder);
    connectionString = $"Data Source={Path.Combine(dbFolder, "tracker.db")}";
}
else
{
    var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
    if (!string.IsNullOrEmpty(dataSource))
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
    }
}

var dbPath = new SqliteConnectionStringBuilder(connectionString).DataSource;
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));

// ─── Application Services ─────────────────────────────────────────────────────
builder.Services.AddWeightLossTrackerServices(builder.Configuration);

// ─── Authentication ───────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.Strict;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
        opt.SlidingExpiration = true;
        opt.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ─── Data Protection ──────────────────────────────────────────────────────────
// Persist keys to disk so auth cookies and antiforgery tokens survive restarts.
var keysDir = builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(Path.GetDirectoryName(dbPath) ?? AppContext.BaseDirectory, "dp-keys");
Directory.CreateDirectory(keysDir);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("WeightLossTracker");

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

var adminKey = app.Configuration["Admin:ApiKey"];
if (string.IsNullOrWhiteSpace(adminKey))
    app.Logger.LogWarning("Admin:ApiKey is not configured. The /api/admin/users endpoint will reject all requests.");

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ─── AUTH ─────────────────────────────────────────────────────────────────────
app.MapPost("/api/auth/login", async (
    AppDbContext db, IAuthService auth, HttpContext ctx, LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username and password are required.");

    if (req.Password.Length > 1000)
        return Results.BadRequest("Password too long.");

    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username == req.Username.Trim().ToLower());

    // Dummy hash (work factor 12) — prevents timing oracle on unknown usernames. Regenerate if WorkFactor changes.
    var hashToCheck = user?.PasswordHash ?? "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/bkiZ7hD3hZJOX6K3i";
    var passwordValid = auth.VerifyPassword(req.Password, hashToCheck);

    if (user is null || !passwordValid)
        return Results.Unauthorized();

    var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
    if (profile is null)
        return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new("profileId", profile.Id.ToString()),
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));

    return Results.Ok(new { username = user.Username, profileId = profile.Id });
});

app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.MapGet("/api/auth/me", async (AppDbContext db, HttpContext ctx) =>
{
    if (!(ctx.User.Identity?.IsAuthenticated ?? false))
        return Results.Unauthorized();

    var username = ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
    var profileIdClaim = ctx.User.FindFirst("profileId")?.Value;
    if (!int.TryParse(profileIdClaim, out var profileId) || profileId <= 0)
        return Results.Unauthorized();

    var profile = await db.UserProfiles.FindAsync(profileId);
    if (profile is null) return Results.Unauthorized();

    return Results.Ok(new
    {
        username,
        profileId,
        profile = new
        {
            profile.Id,
            profile.Name,
            profile.StartingWeight,
            profile.GoalWeight,
            profile.StartDate,
            profile.FitnessLevel,
            profile.Injuries,
            profile.Goals
        }
    });
}).AllowAnonymous();

// ─── ADMIN ────────────────────────────────────────────────────────────────────
app.MapPost("/api/admin/users", async (
    AppDbContext db, IAuthService auth, IConfiguration config,
    HttpContext ctx, CreateUserRequest req) =>
{
    var expectedKey = config["Admin:ApiKey"] ?? "";
    var providedKey = ctx.Request.Headers["X-Admin-Key"].FirstOrDefault() ?? "";

    var expectedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(expectedKey));
    var providedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(providedKey));
    bool keyValid = CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);

    if (string.IsNullOrWhiteSpace(expectedKey) || !keyValid)
        return Results.Forbid();

    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username and password are required.");

    if (req.Password.Length > 1000)
        return Results.BadRequest("Password too long.");

    if (string.IsNullOrWhiteSpace(req.Profile.Name))
        return Results.BadRequest("Profile name is required.");

    var normalizedUsername = req.Username.Trim().ToLower();
    var user = new User
    {
        Username = normalizedUsername,
        PasswordHash = auth.HashPassword(req.Password),
        CreatedAt = DateTime.UtcNow
    };

    await using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(); // user.Id populated here

        var profile = new UserProfile
        {
            UserId = user.Id,
            Name = req.Profile.Name.Trim(),
            StartingWeight = req.Profile.StartingWeight,
            GoalWeight = req.Profile.GoalWeight,
            StartDate = req.Profile.StartDate,
            FitnessLevel = req.Profile.FitnessLevel ?? "",
            Injuries = req.Profile.Injuries ?? "",
            Goals = req.Profile.Goals ?? ""
        };
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync(); // profile.Id populated here

        for (int dow = 0; dow <= 6; dow++)
        {
            db.WorkoutScheduleDays.Add(new WorkoutScheduleDay
            {
                UserProfileId = profile.Id,
                DayOfWeek = dow,
                Location = "Rest"
            });
        }
        await db.SaveChangesAsync();

        await transaction.CommitAsync();
        return Results.Ok(new { userId = user.Id, profileId = profile.Id, username = user.Username });
    }
    catch (DbUpdateException ex)
        when (ex.InnerException is SqliteException sqEx
              && sqEx.SqliteErrorCode == 19
              && sqEx.Message.Contains("Users.Username"))
    {
        await transaction.RollbackAsync();
        return Results.Conflict("Username already taken.");
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
});

// ─── Helper: extract active profile ID from the authenticated user's claims ───
static int GetProfileId(HttpContext ctx)
{
    var value = ctx.User.FindFirst("profileId")?.Value;
    if (int.TryParse(value, out var id) && id > 0) return id;
    throw new InvalidOperationException("Authenticated user has no valid profileId claim.");
}

// ─── PROFILES ─────────────────────────────────────────────────────────────────
app.MapGet("/api/profiles", async (AppDbContext db, HttpContext ctx) =>
{
    var profileId = GetProfileId(ctx);
    var profile = await db.UserProfiles.FindAsync(profileId);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
}).RequireAuthorization();


app.MapGet("/api/profiles/{id:int}", async (AppDbContext db, HttpContext ctx, int id) =>
{
    if (id != GetProfileId(ctx)) return Results.Forbid();
    var profile = await db.UserProfiles.FindAsync(id);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
}).RequireAuthorization();

app.MapPut("/api/profiles/{id:int}", async (AppDbContext db, HttpContext ctx, int id, ProfileRequest req) =>
{
    if (id != GetProfileId(ctx)) return Results.Forbid();
    var profile = await db.UserProfiles.FindAsync(id);
    if (profile is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest("Name is required.");

    profile.Name = req.Name.Trim();
    profile.StartingWeight = req.StartingWeight;
    profile.GoalWeight = req.GoalWeight;
    profile.StartDate = req.StartDate;
    profile.FitnessLevel = req.FitnessLevel ?? "";
    profile.Injuries = req.Injuries ?? "";
    profile.Goals = req.Goals ?? "";
    await db.SaveChangesAsync();
    return Results.Ok(profile);
}).RequireAuthorization();


// ─── DASHBOARD ────────────────────────────────────────────────────────────────
app.MapGet("/api/dashboard", async (AppDbContext db, HttpContext ctx) =>
{
    var profileId = GetProfileId(ctx);
    var profile = await db.UserProfiles.FindAsync(profileId);
    var entries = await db.WeightEntries
        .Where(w => w.UserProfileId == profileId)
        .OrderBy(w => w.Date)
        .ToListAsync();

    double? currentWeight = entries.LastOrDefault()?.Weight;
    double start = profile?.StartingWeight ?? 215;
    double target = profile?.GoalWeight ?? 190;
    double lostSoFar = currentWeight.HasValue
        ? Math.Max(0, start - currentWeight.Value)
        : 0;
    double toGoal = currentWeight.HasValue
        ? Math.Max(0, currentWeight.Value - target)
        : start - target;
    int daysLogged = entries.Count;
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

    var latestBp = await db.BloodPressureEntries
        .Where(b => b.UserProfileId == profileId)
        .OrderByDescending(b => b.RecordedAt)
        .Select(b => new { b.Systolic, b.Diastolic, b.Pulse, b.RecordedAt })
        .FirstOrDefaultAsync();

    return Results.Ok(new
    {
        currentWeight,
        startingWeight = start,
        lostSoFar = Math.Round(lostSoFar, 1),
        toGoal = Math.Round(toGoal, 1),
        daysLogged,
        progressPct,
        goalWeight = target,
        chart = new { labels, weights, trendLine },
        latestBp
    });
}).RequireAuthorization();

// ─── WEIGHT ───────────────────────────────────────────────────────────────────
app.MapGet("/api/weight", async (AppDbContext db, HttpContext ctx) =>
{
    var profileId = GetProfileId(ctx);
    return Results.Ok(await db.WeightEntries
        .Where(w => w.UserProfileId == profileId)
        .OrderByDescending(w => w.Date)
        .ToListAsync());
}).RequireAuthorization();

app.MapPost("/api/weight", async (AppDbContext db, HttpContext ctx, WeightEntryRequest req) =>
{
    if (req.Weight < 50 || req.Weight > 500)
        return Results.BadRequest("Weight must be between 50 and 500 lbs.");

    var profileId = GetProfileId(ctx);
    var today = DateTime.UtcNow.Date;
    var existing = await db.WeightEntries
        .FirstOrDefaultAsync(w => w.UserProfileId == profileId && w.Date == today);
    if (existing != null)
    {
        existing.Weight = req.Weight;
        existing.Notes = req.Notes;
        await db.SaveChangesAsync();
        return Results.Ok(existing);
    }

    var entry = new WeightEntry
    {
        UserProfileId = profileId,
        Date = today,
        Weight = req.Weight,
        Notes = req.Notes
    };
    db.WeightEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Ok(entry);
}).RequireAuthorization();

app.MapPut("/api/weight/{id:int}", async (AppDbContext db, HttpContext ctx, int id, WeightEntryRequest req) =>
{
    if (req.Weight < 50 || req.Weight > 500)
        return Results.BadRequest("Weight must be between 50 and 500 lbs.");

    var profileId = GetProfileId(ctx);
    var entry = await db.WeightEntries.FindAsync(id);
    if (entry == null || entry.UserProfileId != profileId) return Results.NotFound();
    entry.Weight = req.Weight;
    entry.Notes = req.Notes;
    await db.SaveChangesAsync();
    return Results.Ok(entry);
}).RequireAuthorization();

app.MapDelete("/api/weight/{id:int}", async (AppDbContext db, HttpContext ctx, int id) =>
{
    var profileId = GetProfileId(ctx);
    var entry = await db.WeightEntries.FindAsync(id);
    if (entry == null || entry.UserProfileId != profileId) return Results.NotFound();
    db.WeightEntries.Remove(entry);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

// ─── BLOOD PRESSURE ───────────────────────────────────────────────────────────
app.MapGet("/api/bp", async (AppDbContext db, HttpContext ctx) =>
{
    var profileId = GetProfileId(ctx);
    return Results.Ok(await db.BloodPressureEntries
        .Where(b => b.UserProfileId == profileId)
        .OrderByDescending(b => b.RecordedAt)
        .Select(b => new {
            b.Id,
            b.Systolic,
            b.Diastolic,
            b.Pulse,
            b.Notes,
            b.RecordedAt,
            b.Category
        })
        .ToListAsync());
}).RequireAuthorization();

app.MapPost("/api/bp", async (AppDbContext db, HttpContext ctx, BpEntryRequest req) =>
{
    if (req.Systolic < 60 || req.Systolic > 250)
        return Results.BadRequest("Systolic must be between 60 and 250 mmHg.");
    if (req.Diastolic < 40 || req.Diastolic > 150)
        return Results.BadRequest("Diastolic must be between 40 and 150 mmHg.");
    if (req.Pulse < 30 || req.Pulse > 200)
        return Results.BadRequest("Pulse must be between 30 and 200 bpm.");

    var profileId = GetProfileId(ctx);
    var entry = new BloodPressureEntry
    {
        UserProfileId = profileId,
        Systolic      = req.Systolic,
        Diastolic     = req.Diastolic,
        Pulse         = req.Pulse,
        Notes         = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
        RecordedAt    = req.RecordedAt ?? DateTime.UtcNow,
        Category      = BpCategoryService.Classify(req.Systolic, req.Diastolic)
    };
    db.BloodPressureEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/api/bp/{entry.Id}", new {
        entry.Id, entry.Systolic, entry.Diastolic, entry.Pulse,
        entry.Notes, entry.RecordedAt, entry.Category
    });
}).RequireAuthorization();

app.MapDelete("/api/bp/{id:int}", async (AppDbContext db, HttpContext ctx, int id) =>
{
    var profileId = GetProfileId(ctx);
    var entry = await db.BloodPressureEntries
        .FirstOrDefaultAsync(b => b.Id == id && b.UserProfileId == profileId);
    if (entry is null) return Results.NotFound();
    db.BloodPressureEntries.Remove(entry);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// ─── SCHEDULE ─────────────────────────────────────────────────────────────────
app.MapGet("/api/schedule", async (AppDbContext db, HttpContext ctx) =>
{
    var profileId = GetProfileId(ctx);
    return Results.Ok(await db.WorkoutScheduleDays
        .Where(s => s.UserProfileId == profileId)
        .OrderBy(s => s.DayOfWeek)
        .ToListAsync());
}).RequireAuthorization();

app.MapPut("/api/schedule", async (AppDbContext db, HttpContext ctx, List<ScheduleUpdateItem> items) =>
{
    var profileId = GetProfileId(ctx);
    foreach (var item in items)
    {
        var day = await db.WorkoutScheduleDays
            .FirstOrDefaultAsync(s => s.UserProfileId == profileId && s.DayOfWeek == item.DayOfWeek);
        if (day != null) day.Location = item.Location;
    }
    await db.SaveChangesAsync();
    return Results.Ok(await db.WorkoutScheduleDays
        .Where(s => s.UserProfileId == profileId)
        .OrderBy(s => s.DayOfWeek)
        .ToListAsync());
}).RequireAuthorization();

// ─── EXERCISE ─────────────────────────────────────────────────────────────────
app.MapPost("/api/exercise/generate-day", async (
    ExerciseService svc, GeminiService gemini, HttpContext ctx, GenerateDayRequest req) =>
{
    if (!gemini.IsConfigured)
        return Results.Problem("Gemini API key not configured.", statusCode: 503);
    if (req.DayOfWeek < 0 || req.DayOfWeek > 6)
        return Results.BadRequest("dayOfWeek must be 0–6.");
    try
    {
        var profileId = GetProfileId(ctx);
        var suggestion = await svc.GenerateDayWorkoutAsync(req.DayOfWeek, profileId);
        return Results.Ok(suggestion);
    }
    catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
    catch (GeminiApiException ex) { return Results.Problem(ex.Message, statusCode: 502); }
}).RequireAuthorization();

app.MapPost("/api/exercise/generate-week", async (
    ExerciseService svc, GeminiService gemini, HttpContext ctx, HttpResponse response) =>
{
    if (!gemini.IsConfigured)
    {
        response.StatusCode = 503;
        await response.WriteAsJsonAsync(new { error = "Gemini API key not configured." });
        return;
    }

    var profileId = GetProfileId(ctx);
    var results = new List<object>();
    await foreach (var (suggestion, error) in svc.GenerateWeekWorkoutsAsync(profileId))
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
}).RequireAuthorization();

app.MapGet("/api/exercise/history", async (AppDbContext db, HttpContext ctx, int? dayOfWeek) =>
{
    var profileId = GetProfileId(ctx);
    var query = db.ExerciseSuggestions
        .Where(e => e.UserProfileId == profileId);
    if (dayOfWeek.HasValue) query = query.Where(e => e.DayOfWeek == dayOfWeek.Value);
    return Results.Ok(await query.OrderByDescending(e => e.CreatedAt).ToListAsync());
}).RequireAuthorization();

app.MapDelete("/api/exercise/history/{id:int}", async (AppDbContext db, HttpContext ctx, int id) =>
{
    var profileId = GetProfileId(ctx);
    var suggestion = await db.ExerciseSuggestions
        .Include(e => e.AiPromptLog)
        .FirstOrDefaultAsync(e => e.Id == id && e.UserProfileId == profileId);
    if (suggestion == null) return Results.NotFound();
    var log = suggestion.AiPromptLog;
    db.ExerciseSuggestions.Remove(suggestion);
    if (log != null) db.AiPromptLogs.Remove(log);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

// ─── MEALS ────────────────────────────────────────────────────────────────────
app.MapGet("/api/meals/today", async (AppDbContext db, HttpContext ctx) =>
{
    var profileId = GetProfileId(ctx);
    var today = DateTime.UtcNow.Date;
    return Results.Ok(await db.MealLogs
        .Where(m => m.UserProfileId == profileId && m.Date >= today && m.Date < today.AddDays(1))
        .OrderBy(m => m.Date)
        .ToListAsync());
}).RequireAuthorization();

app.MapPost("/api/meals", async (AppDbContext db, HttpContext ctx, MealLogRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.MealType) || string.IsNullOrWhiteSpace(req.Description))
        return Results.BadRequest("MealType and Description are required.");

    var profileId = GetProfileId(ctx);
    var meal = new MealLog
    {
        UserProfileId = profileId,
        Date = DateTime.UtcNow,
        MealType = req.MealType,
        Description = req.Description,
        Calories = req.Calories,
        Notes = req.Notes
    };
    db.MealLogs.Add(meal);
    await db.SaveChangesAsync();
    return Results.Ok(meal);
}).RequireAuthorization();

app.MapDelete("/api/meals/{id:int}", async (AppDbContext db, HttpContext ctx, int id) =>
{
    var profileId = GetProfileId(ctx);
    var meal = await db.MealLogs.FindAsync(id);
    if (meal == null || meal.UserProfileId != profileId) return Results.NotFound();
    db.MealLogs.Remove(meal);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/meals/advice", async (
    MealService mealSvc, GeminiService gemini, HttpContext ctx, MealAdviceRequest req) =>
{
    if (!gemini.IsConfigured)
        return Results.Problem("Gemini API key not configured.", statusCode: 503);
    if (string.IsNullOrWhiteSpace(req.Question))
        return Results.BadRequest("Question is required.");
    try
    {
        var profileId = GetProfileId(ctx);
        var result = await mealSvc.GetNutritionAdviceAsync(req.Question, profileId);
        return Results.Ok(new
        {
            advice = result.Text,
            inputTokens = result.InputTokens,
            outputTokens = result.OutputTokens
        });
    }
    catch (GeminiApiException ex) { return Results.Problem(ex.Message, statusCode: 502); }
}).RequireAuthorization();

// ─── AI HISTORY ───────────────────────────────────────────────────────────────
app.MapGet("/api/ai-history", async (AppDbContext db, HttpContext ctx, string? type) =>
{
    var profileId = GetProfileId(ctx);
    var query = db.AiPromptLogs
        .Where(l => l.UserProfileId == profileId);
    if (!string.IsNullOrWhiteSpace(type)) query = query.Where(l => l.PromptType == type);
    return Results.Ok(await query.OrderByDescending(l => l.CreatedAt).ToListAsync());
}).RequireAuthorization();

app.MapDelete("/api/ai-history/{id:int}", async (AppDbContext db, HttpContext ctx, int id) =>
{
    var profileId = GetProfileId(ctx);
    var log = await db.AiPromptLogs.FindAsync(id);
    if (log == null || log.UserProfileId != profileId) return Results.NotFound();
    var linked = await db.ExerciseSuggestions.AnyAsync(e => e.AiPromptLogId == id);
    if (linked)
        return Results.Conflict(
            "A linked ExerciseSuggestion exists. Delete the exercise history entry first.");
    db.AiPromptLogs.Remove(log);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.Run("http://localhost:5000");

// ─── REQUEST DTOs ─────────────────────────────────────────────────────────────
record WeightEntryRequest(double Weight, string? Notes);
record ScheduleUpdateItem(int DayOfWeek, string Location);
record GenerateDayRequest(int DayOfWeek);
record MealLogRequest(string MealType, string Description, int? Calories, string? Notes);
record MealAdviceRequest(string Question);
record ProfileRequest(string Name, double StartingWeight, double GoalWeight,
    DateTime StartDate, string? FitnessLevel, string? Injuries, string? Goals);
record CreateUserRequest(string Username, string Password, ProfileRequest Profile);
record LoginRequest(string Username, string Password)
{
    public override string ToString() =>
        $"LoginRequest {{ Username = {Username}, Password = [REDACTED] }}";
}
record BpEntryRequest(int Systolic, int Diastolic, int Pulse, string? Notes, DateTime? RecordedAt);
