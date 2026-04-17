# User Authentication Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add username/password authentication with server-side cookie sessions so each hosted user has their own account and is tied to exactly one UserProfile.

**Architecture:** BCrypt password hashing via `BCrypt.Net-Next`, ASP.NET Core Cookie Authentication middleware for session management, claims store `userId` and `profileId` to avoid per-request DB lookups. All `/api/*` routes require authorization. Static files remain public. A key-protected `POST /api/admin/users` endpoint is the only way to create accounts.

**Tech Stack:** .NET 10 Minimal API, ASP.NET Core Cookie Authentication (built-in), `BCrypt.Net-Next` NuGet package, EF Core + SQLite, xUnit for unit tests, vanilla JS frontend.

---

## File Map

**New files:**
- `WeightLossTracker/Models/User.cs` — User entity
- `WeightLossTracker/Services/AuthService.cs` — BCrypt hashing and verification
- `WeightLossTracker.Tests/WeightLossTracker.Tests.csproj` — xUnit test project
- `WeightLossTracker.Tests/AuthServiceTests.cs` — unit tests for AuthService

**Modified files:**
- `WeightLossTracker/WeightLossTracker.csproj` — add BCrypt.Net-Next
- `WeightLossTracker/Models/Models.cs` — add `UserId` FK to UserProfile
- `WeightLossTracker/Data/AppDbContext.cs` — add Users DbSet, configure relationship, remove seed data
- `WeightLossTracker/Program.cs` — auth middleware, new `GetProfileId`, new endpoints, protect routes
- `WeightLossTracker/appsettings.json` — add `Admin:ApiKey`
- `WeightLossTracker/appsettings.example.json` — add `Admin:ApiKey`
- `WeightLossTracker/wwwroot/index.html` — add login view, remove profile selector, add username + logout
- `WeightLossTracker/wwwroot/js/api.js` — remove X-Profile-Id header, add auth routes, handle 401
- `WeightLossTracker/wwwroot/js/app.js` — replace `initProfile` with `initAuth`, simplify profile view

---

## Task 1: Set Up xUnit Test Project

**Files:**
- Create: `WeightLossTracker.Tests/WeightLossTracker.Tests.csproj`

- [ ] **Step 1: Create the test project**

```bash
cd C:/Development/Weightloss
dotnet new xunit -n WeightLossTracker.Tests -o WeightLossTracker.Tests --framework net10.0
```

Expected output: `The template "xUnit Test Project" was created successfully.`

- [ ] **Step 2: Add a project reference to the main app**

```bash
cd C:/Development/Weightloss
dotnet add WeightLossTracker.Tests/WeightLossTracker.Tests.csproj reference WeightLossTracker/WeightLossTracker.csproj
```

Expected output: `Reference ..\WeightLossTracker\WeightLossTracker.csproj added to the project.`

- [ ] **Step 3: Verify the test project builds**

```bash
cd C:/Development/Weightloss
dotnet build WeightLossTracker.Tests/WeightLossTracker.Tests.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker.Tests/
git commit -m "feat: add xUnit test project"
```

---

## Task 2: Add BCrypt.Net-Next NuGet Package

**Files:**
- Modify: `WeightLossTracker/WeightLossTracker.csproj`

- [ ] **Step 1: Add BCrypt.Net-Next to the main project**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet add package BCrypt.Net-Next
```

Expected output: `PackageReference for package 'BCrypt.Net-Next' version X.X.X added to file...`

- [ ] **Step 2: Add it to the test project as well**

```bash
cd C:/Development/Weightloss
dotnet add WeightLossTracker.Tests/WeightLossTracker.Tests.csproj package BCrypt.Net-Next
```

- [ ] **Step 3: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/WeightLossTracker.csproj WeightLossTracker.Tests/WeightLossTracker.Tests.csproj
git commit -m "feat: add BCrypt.Net-Next package for password hashing"
```

---

## Task 3: Create User Model

**Files:**
- Create: `WeightLossTracker/Models/User.cs`

- [ ] **Step 1: Create `User.cs`**

```csharp
namespace WeightLossTracker.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
```

Save to `WeightLossTracker/Models/User.cs`.

- [ ] **Step 2: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add WeightLossTracker/Models/User.cs
git commit -m "feat: add User model"
```

---

## Task 4: Add UserId FK to UserProfile + Update AppDbContext

**Files:**
- Modify: `WeightLossTracker/Models/Models.cs`
- Modify: `WeightLossTracker/Data/AppDbContext.cs`

- [ ] **Step 1: Add `UserId` to `UserProfile` in `Models.cs`**

In `WeightLossTracker/Models/Models.cs`, replace the `UserProfile` class with:

```csharp
public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = "";
    public double StartingWeight { get; set; }
    public double GoalWeight { get; set; }
    public DateTime StartDate { get; set; }
    public string FitnessLevel { get; set; } = "";
    public string Injuries { get; set; } = "";
    public string Goals { get; set; } = "";
}
```

- [ ] **Step 2: Replace `AppDbContext.cs` entirely**

Replace the full content of `WeightLossTracker/Data/AppDbContext.cs` with:

```csharp
using Microsoft.EntityFrameworkCore;
using WeightLossTracker.Models;

namespace WeightLossTracker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<WeightEntry> WeightEntries => Set<WeightEntry>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<WorkoutScheduleDay> WorkoutScheduleDays => Set<WorkoutScheduleDay>();
    public DbSet<ExerciseSuggestion> ExerciseSuggestions => Set<ExerciseSuggestion>();
    public DbSet<AiPromptLog> AiPromptLogs => Set<AiPromptLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User.Username must be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // User -> UserProfile 1:1
        modelBuilder.Entity<UserProfile>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        // Composite unique index: one weight entry per calendar day per profile
        modelBuilder.Entity<WeightEntry>()
            .HasIndex(w => new { w.UserProfileId, w.Date })
            .IsUnique();

        modelBuilder.Entity<WeightEntry>()
            .HasOne(w => w.UserProfile)
            .WithMany()
            .HasForeignKey(w => w.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index: one schedule entry per day of week per profile
        modelBuilder.Entity<WorkoutScheduleDay>()
            .HasIndex(s => new { s.UserProfileId, s.DayOfWeek })
            .IsUnique();

        modelBuilder.Entity<WorkoutScheduleDay>()
            .HasOne(s => s.UserProfile)
            .WithMany()
            .HasForeignKey(s => s.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // MealLog -> UserProfile cascade
        modelBuilder.Entity<MealLog>()
            .HasOne(m => m.UserProfile)
            .WithMany()
            .HasForeignKey(m => m.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // ExerciseSuggestion -> UserProfile cascade
        modelBuilder.Entity<ExerciseSuggestion>()
            .HasOne(e => e.UserProfile)
            .WithMany()
            .HasForeignKey(e => e.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // ExerciseSuggestion -> AiPromptLog 1:1, restrict delete
        modelBuilder.Entity<ExerciseSuggestion>()
            .HasIndex(e => e.AiPromptLogId)
            .IsUnique();

        modelBuilder.Entity<ExerciseSuggestion>()
            .HasOne(e => e.AiPromptLog)
            .WithOne()
            .HasForeignKey<ExerciseSuggestion>(e => e.AiPromptLogId)
            .OnDelete(DeleteBehavior.Restrict);

        // AiPromptLog -> UserProfile cascade
        modelBuilder.Entity<AiPromptLog>()
            .HasOne(l => l.UserProfile)
            .WithMany()
            .HasForeignKey(l => l.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

Note: the seed data block (`modelBuilder.Entity<UserProfile>().HasData(...)` and `modelBuilder.Entity<WorkoutScheduleDay>().HasData(...)`) has been removed entirely — clean slate as decided.

- [ ] **Step 3: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/Models/Models.cs WeightLossTracker/Data/AppDbContext.cs
git commit -m "feat: add UserId FK to UserProfile, add Users DbSet, remove seed data"
```

---

## Task 5: Create AuthService + Unit Tests

**Files:**
- Create: `WeightLossTracker/Services/AuthService.cs`
- Create: `WeightLossTracker.Tests/AuthServiceTests.cs`

- [ ] **Step 1: Write the failing tests first**

Create `WeightLossTracker.Tests/AuthServiceTests.cs`:

```csharp
using WeightLossTracker.Services;
using Xunit;

namespace WeightLossTracker.Tests;

public class AuthServiceTests
{
    private readonly AuthService _sut = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        var hash = _sut.HashPassword("secret123");
        Assert.False(string.IsNullOrEmpty(hash));
    }

    [Fact]
    public void HashPassword_DoesNotReturnPlaintext()
    {
        var hash = _sut.HashPassword("secret123");
        Assert.NotEqual("secret123", hash);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        var hash = _sut.HashPassword("correct");
        Assert.True(_sut.VerifyPassword("correct", hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var hash = _sut.HashPassword("correct");
        Assert.False(_sut.VerifyPassword("wrong", hash));
    }

    [Fact]
    public void HashPassword_ProducesUniqueHashesEachCall()
    {
        var h1 = _sut.HashPassword("same");
        var h2 = _sut.HashPassword("same");
        Assert.NotEqual(h1, h2);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail (AuthService doesn't exist yet)**

```bash
cd C:/Development/Weightloss
dotnet test WeightLossTracker.Tests/ --no-build 2>&1 || true
dotnet build WeightLossTracker.Tests/
dotnet test WeightLossTracker.Tests/
```

Expected: build error or test failure — `AuthService` type not found.

- [ ] **Step 3: Create `AuthService.cs`**

Create `WeightLossTracker/Services/AuthService.cs`:

```csharp
namespace WeightLossTracker.Services;

public class AuthService
{
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
```

- [ ] **Step 4: Register AuthService in `ServiceCollectionExtensions.cs`**

Open `WeightLossTracker/Services/ServiceCollectionExtensions.cs` and add `services.AddSingleton<AuthService>();` inside the extension method. The exact location is inside the `AddWeightLossTrackerServices` method body. Add it as the last line before the closing brace.

- [ ] **Step 5: Run tests to verify they pass**

```bash
cd C:/Development/Weightloss
dotnet test WeightLossTracker.Tests/ -v normal
```

Expected:
```
Passed! - Failed: 0, Passed: 5, Skipped: 0, Total: 5
```

- [ ] **Step 6: Commit**

```bash
git add WeightLossTracker/Services/AuthService.cs WeightLossTracker/Services/ServiceCollectionExtensions.cs WeightLossTracker.Tests/AuthServiceTests.cs
git commit -m "feat: add AuthService with BCrypt hashing, add unit tests"
```

---

## Task 6: Configure Auth Middleware + Update appsettings

**Files:**
- Modify: `WeightLossTracker/Program.cs` (middleware registration section only)
- Modify: `WeightLossTracker/appsettings.json`
- Modify: `WeightLossTracker/appsettings.example.json`

- [ ] **Step 1: Add auth services to `Program.cs`**

In `Program.cs`, after the `builder.Services.AddWeightLossTrackerServices(builder.Configuration);` line and before `var app = builder.Build();`, add:

```csharp
// ─── Authentication ───────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.Strict;
        opt.ExpireTimeSpan = TimeSpan.FromDays(14);
        opt.SlidingExpiration = true;
        opt.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
```

- [ ] **Step 2: Add the using statement at the top of `Program.cs`**

Add to the top of `Program.cs`:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
```

- [ ] **Step 3: Add `UseAuthentication` and `UseAuthorization` in `Program.cs`**

After `app.UseStaticFiles();` (around line 76), add:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

- [ ] **Step 4: Add `Admin:ApiKey` to `appsettings.json`**

Add the `Admin` section to `WeightLossTracker/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Gemini": {
    "ApiKey": "AIzaSyAeJxZFYIisO-5T8CBprAKaOylubayjmyQ",
    "Model": "gemini-2.5-flash",
    "MaxOutputTokens": 1024
  },
  "Admin": {
    "ApiKey": ""
  }
}
```

- [ ] **Step 5: Add `Admin:ApiKey` to `appsettings.example.json`**

Add the `Admin` section to `WeightLossTracker/appsettings.example.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Gemini": {
    "ApiKey": "<your-google-ai-api-key>",
    "Model": "gemini-2.5-flash",
    "MaxOutputTokens": 2048,
    "ExerciseMaxOutputTokens": 4096
  },
  "Admin": {
    "ApiKey": "<generate-a-strong-random-secret>"
  }
}
```

- [ ] **Step 6: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add WeightLossTracker/Program.cs WeightLossTracker/appsettings.json WeightLossTracker/appsettings.example.json
git commit -m "feat: configure cookie authentication middleware and admin key config"
```

---

## Task 7: Replace GetProfileId + Protect Existing API Routes

**Files:**
- Modify: `WeightLossTracker/Program.cs`

The current `GetProfileId` reads from `X-Profile-Id` header. Replace it with a version that reads `profileId` from the authenticated user's claims. The `profileId` claim is stored at login (added in Task 8).

- [ ] **Step 1: Replace the `GetProfileId` helper in `Program.cs`**

Find the line:
```csharp
static int GetProfileId(HttpContext ctx) =>
    int.TryParse(ctx.Request.Headers["X-Profile-Id"].FirstOrDefault(), out var id) && id > 0
        ? id : 1;
```

Replace it with:
```csharp
static int GetProfileId(HttpContext ctx) =>
    int.TryParse(
        ctx.User.FindFirst("profileId")?.Value,
        out var id) && id > 0 ? id : 0;
```

- [ ] **Step 2: Add `.RequireAuthorization()` to every existing API route**

For each `app.Map*` call for routes starting with `/api/`, add `.RequireAuthorization()` at the end of the call chain.

The routes to update are:
- `app.MapGet("/api/profiles", ...)` → add `.RequireAuthorization()`
- `app.MapPost("/api/profiles", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/profiles/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapPut("/api/profiles/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapDelete("/api/profiles/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/dashboard", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/weight", ...)` → add `.RequireAuthorization()`
- `app.MapPost("/api/weight", ...)` → add `.RequireAuthorization()`
- `app.MapPut("/api/weight/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapDelete("/api/weight/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/schedule", ...)` → add `.RequireAuthorization()`
- `app.MapPut("/api/schedule", ...)` → add `.RequireAuthorization()`
- `app.MapPost("/api/exercise/generate-day", ...)` → add `.RequireAuthorization()`
- `app.MapPost("/api/exercise/generate-week", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/exercise/history", ...)` → add `.RequireAuthorization()`
- `app.MapDelete("/api/exercise/history/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/meals/today", ...)` → add `.RequireAuthorization()`
- `app.MapPost("/api/meals", ...)` → add `.RequireAuthorization()`
- `app.MapDelete("/api/meals/{id:int}", ...)` → add `.RequireAuthorization()`
- `app.MapPost("/api/meals/advice", ...)` → add `.RequireAuthorization()`
- `app.MapGet("/api/ai-history", ...)` → add `.RequireAuthorization()`
- `app.MapDelete("/api/ai-history/{id:int}", ...)` → add `.RequireAuthorization()`

Example — before:
```csharp
app.MapGet("/api/dashboard", async (AppDbContext db, HttpContext ctx) =>
{
    ...
});
```

After:
```csharp
app.MapGet("/api/dashboard", async (AppDbContext db, HttpContext ctx) =>
{
    ...
}).RequireAuthorization();
```

For routes that use `app.MapPost` with a lambda that doesn't end with `});` but ends with `});` followed by a `;`, the `.RequireAuthorization()` goes before the final `;`.

For the `generate-week` route which uses `await response.WriteAsJsonAsync(results);` and `return;` inside (no `Results.*` return), the pattern is:
```csharp
app.MapPost("/api/exercise/generate-week", async (...) =>
{
    ...
}).RequireAuthorization();
```

- [ ] **Step 3: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/Program.cs
git commit -m "feat: replace GetProfileId helper with claims-based, protect all API routes"
```

---

## Task 8: Add Auth Endpoints (login, logout, me)

**Files:**
- Modify: `WeightLossTracker/Program.cs`

Add these three endpoints **before** the `// ─── PROFILES` section and **after** the `app.UseAuthentication(); app.UseAuthorization();` calls. These routes do NOT get `.RequireAuthorization()`.

- [ ] **Step 1: Add the auth endpoints block to `Program.cs`**

Insert the following block after `app.UseAuthorization();` and before `// ─── Helper: extract active profile ID`:

```csharp
// ─── AUTH ─────────────────────────────────────────────────────────────────────
app.MapPost("/api/auth/login", async (
    AppDbContext db, AuthService auth, HttpContext ctx, LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username and password are required.");

    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username == req.Username.Trim().ToLower());

    if (user is null || !auth.VerifyPassword(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
    if (profile is null)
        return Results.Problem("No profile associated with this account.", statusCode: 500);

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
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();

    var username = ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
    var profileId = GetProfileId(ctx);
    if (profileId == 0) return Results.Unauthorized();

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
});
```

- [ ] **Step 2: Add `LoginRequest` to the DTOs at the bottom of `Program.cs`**

After the existing records at the bottom of `Program.cs`, add:

```csharp
record LoginRequest(string Username, string Password);
```

- [ ] **Step 3: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/Program.cs
git commit -m "feat: add /api/auth/login, /api/auth/logout, /api/auth/me endpoints"
```

---

## Task 9: Add Admin Endpoint (POST /api/admin/users)

**Files:**
- Modify: `WeightLossTracker/Program.cs`

Add this endpoint in a new `// ─── ADMIN` section, directly after the `// ─── AUTH` section. It does NOT use `.RequireAuthorization()` — it uses its own key check.

- [ ] **Step 1: Add the admin endpoint to `Program.cs`**

Insert after the auth endpoints block:

```csharp
// ─── ADMIN ────────────────────────────────────────────────────────────────────
app.MapPost("/api/admin/users", async (
    AppDbContext db, AuthService auth, IConfiguration config,
    HttpContext ctx, CreateUserRequest req) =>
{
    var expectedKey = config["Admin:ApiKey"] ?? "";
    var providedKey = ctx.Request.Headers["X-Admin-Key"].FirstOrDefault() ?? "";

    if (string.IsNullOrWhiteSpace(expectedKey) || providedKey != expectedKey)
        return Results.Forbid();

    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username and password are required.");

    if (string.IsNullOrWhiteSpace(req.Profile.Name))
        return Results.BadRequest("Profile name is required.");

    var normalizedUsername = req.Username.Trim().ToLower();
    var exists = await db.Users.AnyAsync(u => u.Username == normalizedUsername);
    if (exists)
        return Results.Conflict("Username already taken.");

    var user = new User
    {
        Username = normalizedUsername,
        PasswordHash = auth.HashPassword(req.Password),
        CreatedAt = DateTime.UtcNow
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

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
    await db.SaveChangesAsync(); // Save profile first so profile.Id is populated

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

    return Results.Ok(new { userId = user.Id, profileId = profile.Id, username = user.Username });
});
```

- [ ] **Step 2: Add `CreateUserRequest` DTO at the bottom of `Program.cs`**

After `record LoginRequest(string Username, string Password);`, add:

```csharp
record CreateUserRequest(string Username, string Password, ProfileRequest Profile);
```

- [ ] **Step 3: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/Program.cs
git commit -m "feat: add /api/admin/users endpoint with admin key protection"
```

---

## Task 10: Generate EF Migration

**Files:**
- Create: `WeightLossTracker/Migrations/XXXXXXXX_AddUserAuthentication.cs` (auto-generated)

- [ ] **Step 1: Ensure EF Core tools are installed**

```bash
dotnet tool list -g
```

If `dotnet-ef` is not listed, install it:

```bash
dotnet tool install --global dotnet-ef
```

- [ ] **Step 2: Generate the migration**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet ef migrations add AddUserAuthentication
```

Expected output: `Build started... Build succeeded. Done. To undo this action, use 'ef migrations remove'`

- [ ] **Step 3: Review the generated migration**

Open the generated file in `Migrations/XXXXXXXX_AddUserAuthentication.cs`. Verify it:
- Creates the `Users` table with `Id`, `Username`, `PasswordHash`, `CreatedAt`
- Adds a unique index on `Users.Username`
- Adds `UserId` column to `UserProfiles`
- Adds a unique index on `UserProfiles.UserId`
- Adds the FK constraint from `UserProfiles.UserId` to `Users.Id`
- Does NOT have a `Down` that would break the clean-slate approach

If the migration also drops seed data inserts, that is expected.

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/Migrations/
git commit -m "feat: add EF migration for User table and UserProfile.UserId FK"
```

---

## Task 11: Update api.js

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/api.js`

Replace the full content of `WeightLossTracker/wwwroot/js/api.js` with:

```javascript
// Bridge API — maps named actions to HTTP calls
const Bridge = (() => {
  async function call(action, data) {
    const routes = {
      // Auth
      login:                { method: 'POST',   url: '/api/auth/login' },
      logout:               { method: 'POST',   url: '/api/auth/logout' },
      getMe:                { method: 'GET',    url: '/api/auth/me' },
      // Profile
      updateProfile:        { method: 'PUT',    url: '/api/profiles/{id}' },
      // Dashboard
      getDashboard:         { method: 'GET',    url: '/api/dashboard' },
      // Weight
      getWeightEntries:     { method: 'GET',    url: '/api/weight' },
      saveWeight:           { method: 'POST',   url: '/api/weight' },
      updateWeight:         { method: 'PUT',    url: '/api/weight/{id}' },
      deleteWeight:         { method: 'DELETE', url: '/api/weight/{id}' },
      // Schedule
      getSchedule:          { method: 'GET',    url: '/api/schedule' },
      saveSchedule:         { method: 'PUT',    url: '/api/schedule' },
      // Exercise
      generateDayWorkout:   { method: 'POST',   url: '/api/exercise/generate-day' },
      generateWeekWorkouts: { method: 'POST',   url: '/api/exercise/generate-week' },
      getExerciseHistory:   { method: 'GET',    url: '/api/exercise/history' },
      deleteExerciseHistory:{ method: 'DELETE', url: '/api/exercise/history/{id}' },
      // Meals
      getTodayMeals:        { method: 'GET',    url: '/api/meals/today' },
      addMeal:              { method: 'POST',   url: '/api/meals' },
      deleteMeal:           { method: 'DELETE', url: '/api/meals/{id}' },
      getMealAdvice:        { method: 'POST',   url: '/api/meals/advice' },
      // AI history
      getAiHistory:         { method: 'GET',    url: '/api/ai-history' },
      deleteAiHistory:      { method: 'DELETE', url: '/api/ai-history/{id}' },
    };

    const route = routes[action];
    if (!route) throw new Error(`Unknown action: ${action}`);

    let url = route.url;
    let body = undefined;

    if (data && data.id !== undefined) {
      url = url.replace('{id}', data.id);
    }

    if (route.method === 'GET' && data) {
      const params = new URLSearchParams();
      if (data.dayOfWeek !== undefined) params.set('dayOfWeek', data.dayOfWeek);
      if (data.type !== undefined) params.set('type', data.type);
      const qs = params.toString();
      if (qs) url += '?' + qs;
    }

    if (route.method !== 'GET' && route.method !== 'DELETE' && data) {
      if (Array.isArray(data)) {
        body = JSON.stringify(data);
      } else {
        const { id, ...rest } = data;
        if (Object.keys(rest).length) body = JSON.stringify(rest);
      }
    }

    const headers = {};
    if (body) headers['Content-Type'] = 'application/json';

    try {
      const res = await fetch(url, { method: route.method, headers, body, credentials: 'same-origin' });

      if (res.status === 401 && action !== 'login' && action !== 'getMe') {
        showLoginView();
        return { ok: false, status: 401, data: 'Session expired. Please log in again.' };
      }

      let responseData;
      const ct = res.headers.get('Content-Type') || '';
      if (ct.includes('application/json')) {
        responseData = await res.json();
      } else {
        responseData = await res.text();
      }

      return { ok: res.ok, status: res.status, data: responseData };
    } catch (err) {
      return { ok: false, status: 0, data: err.message };
    }
  }

  return { call };
})();
```

Key changes from original:
- Removed `X-Profile-Id` header from all requests
- Added `credentials: 'same-origin'` to `fetch` so the session cookie is sent automatically
- Added `login`, `logout`, `getMe` routes
- Removed `getProfiles`, `createProfile`, `getProfile`, `deleteProfile` (admin-only operations now)
- Added 401 handler: if any non-auth API call returns 401, redirect to login view

- [ ] **Step 1: Replace `api.js` with the content above**

- [ ] **Step 2: Verify syntax by opening the browser DevTools console after the server is running** (deferred until Task 13 smoke test)

- [ ] **Step 3: Commit**

```bash
git add WeightLossTracker/wwwroot/js/api.js
git commit -m "feat: update api.js — remove X-Profile-Id, add auth routes, handle 401"
```

---

## Task 12: Update index.html

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html`

Three changes:
1. Remove profile selectors from mobile header and desktop sidebar
2. Add username display + logout button to sidebar footer
3. Add a `<div id="login-view">` overlay that shows when not authenticated

- [ ] **Step 1: Remove mobile profile selector and goal text from mobile header**

In the mobile header section, remove these two elements:

```html
<select id="mobile-profile-selector" onchange="switchProfile(this.value)"
        class="bg-indigo-800 text-indigo-200 text-xs rounded px-1.5 py-1 border border-indigo-700 ml-auto mr-1 max-w-[100px] hidden sm:block focus:outline-none focus:ring-1 focus:ring-indigo-400"
        aria-label="Switch profile"></select>
<span id="mobile-goal-text" class="text-indigo-400 text-xs mr-2 hidden sm:block"></span>
```

Replace them with:

```html
<span id="mobile-username" class="text-indigo-300 text-xs ml-auto mr-2 hidden sm:block"></span>
```

- [ ] **Step 2: Remove desktop profile selector from sidebar**

Find and remove the entire profile selector `<div>`:

```html
<!-- Profile selector -->
<div class="px-3 mb-4">
  <select id="sidebar-profile-selector" onchange="switchProfile(this.value)"
          class="w-full bg-indigo-800 text-white text-sm rounded-lg px-2 py-1.5 border border-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-400"
          aria-label="Switch profile"></select>
</div>
```

Replace it with:

```html
<!-- User info -->
<div class="px-3 mb-4">
  <p id="sidebar-username" class="text-indigo-300 text-xs font-medium truncate"></p>
</div>
```

- [ ] **Step 3: Add logout button to sidebar footer**

In the sidebar footer, after the `<p id="sidebar-goal-text" ...>` element, add a logout button:

```html
<button onclick="handleLogout()"
        class="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-xs text-indigo-300 hover:text-white hover:bg-red-700 transition-colors focus:outline-none focus:ring-2 focus:ring-red-400 min-h-[44px] mb-1">
  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
       fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
       stroke-linejoin="round" aria-hidden="true">
    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
    <polyline points="16 17 21 12 16 7"/>
    <line x1="21" y1="12" x2="9" y2="12"/>
  </svg>
  Log out
</button>
```

- [ ] **Step 4: Add login view overlay before `</body>`**

Before the two `<script>` tags at the bottom of `<body>`, add:

```html
<!-- Login view (shown when unauthenticated) -->
<div id="login-view" class="hidden fixed inset-0 z-50 bg-gray-100 dark:bg-gray-900 flex items-center justify-center p-4">
  <div class="bg-white dark:bg-gray-800 rounded-xl shadow-lg w-full max-w-sm p-8">
    <div class="flex items-center gap-3 mb-8">
      <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" class="text-indigo-500" aria-hidden="true">
        <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/>
        <line x1="8" y1="12" x2="16" y2="12"/>
      </svg>
      <h1 class="text-xl font-bold text-gray-800 dark:text-gray-100">Weight Loss Tracker</h1>
    </div>

    <div id="login-error"></div>

    <form id="login-form" class="space-y-4" novalidate>
      <div>
        <label for="login-username" class="block text-sm text-gray-600 dark:text-gray-300 mb-1">
          Username
        </label>
        <input id="login-username" type="text" autocomplete="username" required
               class="border dark:border-gray-600 rounded-lg px-3 py-2 w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-400"
               placeholder="your username">
      </div>
      <div>
        <label for="login-password" class="block text-sm text-gray-600 dark:text-gray-300 mb-1">
          Password
        </label>
        <input id="login-password" type="password" autocomplete="current-password" required
               class="border dark:border-gray-600 rounded-lg px-3 py-2 w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-400"
               placeholder="••••••••">
      </div>
      <button type="submit" id="login-btn"
              class="bg-indigo-600 hover:bg-indigo-700 text-white px-5 py-2 rounded-lg font-medium transition-colors w-full min-h-[44px]">
        Log in
      </button>
    </form>
  </div>
</div>
```

- [ ] **Step 5: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat: add login view overlay, remove profile selector, add logout button to sidebar"
```

---

## Task 13: Update app.js

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js`

Three changes:
1. Replace `initProfile()` with `initAuth()` that calls `/api/auth/me`
2. Add `showLoginView()`, `handleLogout()`, login form handler
3. Simplify `renderProfile()` to show only edit form for current user (no create/delete)
4. Update `updateProfileUI()` to show username instead of profile selector

- [ ] **Step 1: Remove the profile state variables and add auth state**

Find and remove:
```javascript
let activeChart = null;
let activeProfile = null;
let allProfiles = [];
let currentView = 'dashboard';
```

Replace with:
```javascript
let activeChart = null;
let activeProfile = null;
let currentUser = null;
let currentView = 'dashboard';
```

- [ ] **Step 2: Replace `initProfile()` and `switchProfile()` with `initAuth()`**

Find and remove the entire `// ─── Profile management` section (lines from `// ─── Profile management` through the end of `updateProfileUI()`).

Replace with:

```javascript
// ─── Auth ──────────────────────────────────────────────────────────────────
function showLoginView() {
  document.getElementById('login-view').classList.remove('hidden');
}

function hideLoginView() {
  document.getElementById('login-view').classList.add('hidden');
}

async function initAuth() {
  const r = await Bridge.call('getMe');
  if (!r.ok) {
    showLoginView();
    return false;
  }
  currentUser = { username: r.data.username };
  activeProfile = r.data.profile;
  updateProfileUI();
  return true;
}

async function handleLogout() {
  await Bridge.call('logout');
  currentUser = null;
  activeProfile = null;
  showLoginView();
}

function updateProfileUI() {
  if (!activeProfile || !currentUser) return;

  const goalText = `Goal: ${activeProfile.startingWeight} → ${activeProfile.goalWeight} lbs`;
  const lossText = `${Math.round(activeProfile.startingWeight - activeProfile.goalWeight)} lbs to lose`;

  const sidebarGoal = document.getElementById('sidebar-goal-text');
  if (sidebarGoal) sidebarGoal.innerHTML = `${escHtml(goalText)}<br><span class="text-indigo-500">${escHtml(lossText)}</span>`;

  const sidebarUsername = document.getElementById('sidebar-username');
  if (sidebarUsername) sidebarUsername.textContent = currentUser.username;

  const mobileUsername = document.getElementById('mobile-username');
  if (mobileUsername) mobileUsername.textContent = currentUser.username;
}

document.getElementById('login-form').addEventListener('submit', async e => {
  e.preventDefault();
  const errEl = document.getElementById('login-error');
  errEl.innerHTML = '';

  const username = document.getElementById('login-username').value.trim();
  const password = document.getElementById('login-password').value;
  const btn = document.getElementById('login-btn');

  if (!username || !password) {
    showError('login-error', 'Username and password are required.');
    return;
  }

  btn.disabled = true;
  btn.textContent = 'Logging in…';

  const r = await Bridge.call('login', { username, password });

  btn.disabled = false;
  btn.textContent = 'Log in';

  if (!r.ok) {
    showError('login-error', 'Invalid username or password.');
    document.getElementById('login-password').value = '';
    document.getElementById('login-username').focus();
    return;
  }

  currentUser = { username: r.data.username };
  hideLoginView();

  const meResult = await Bridge.call('getMe');
  if (meResult.ok) {
    activeProfile = meResult.data.profile;
    updateProfileUI();
  }

  navigate('dashboard');
});
```

- [ ] **Step 3: Simplify `renderProfile()`**

Find and replace the entire `renderProfile()` function with:

```javascript
async function renderProfile() {
  const root = document.getElementById('view-root');

  if (!activeProfile) {
    root.innerHTML = `<p class="text-red-600 dark:text-red-400 p-4">Profile not loaded.</p>`;
    return;
  }

  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="${C.h1}">Profile</h1>

      <div class="${C.card} max-w-lg">
        <h2 class="${C.h2}">Edit your profile</h2>
        <div id="profile-edit-error"></div>
        <form id="profile-edit-form" class="space-y-3" novalidate>
          <div>
            <label for="pe-name" class="${C.label}">Display name</label>
            <input id="pe-name" type="text" required class="${C.input}" value="${escHtml(activeProfile.name || '')}">
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label for="pe-sw" class="${C.label}">Starting weight (lbs)</label>
              <input id="pe-sw" type="number" step="0.1" min="50" max="500" required class="${C.input}" value="${activeProfile.startingWeight || ''}">
            </div>
            <div>
              <label for="pe-gw" class="${C.label}">Goal weight (lbs)</label>
              <input id="pe-gw" type="number" step="0.1" min="50" max="500" required class="${C.input}" value="${activeProfile.goalWeight || ''}">
            </div>
          </div>
          <div>
            <label for="pe-fl" class="${C.label}">Fitness level</label>
            <select id="pe-fl" class="${C.select}">
              ${['Beginner','Intermediate','Advanced'].map(l =>
                `<option ${activeProfile.fitnessLevel === l ? 'selected' : ''}>${l}</option>`
              ).join('')}
            </select>
          </div>
          <div>
            <label for="pe-inj" class="${C.label}">Injuries</label>
            <input id="pe-inj" type="text" class="${C.input}" value="${escHtml(activeProfile.injuries || '')}" placeholder="e.g. Neck injury, lower back injury">
          </div>
          <div>
            <label for="pe-goals" class="${C.label}">Goals</label>
            <input id="pe-goals" type="text" class="${C.input}" value="${escHtml(activeProfile.goals || '')}" placeholder="e.g. Lose weight, build muscle">
          </div>
          <button type="submit" class="${C.btnPrimary} w-full">Save changes</button>
        </form>
      </div>
    </div>`;

  document.getElementById('profile-edit-form').addEventListener('submit', async e => {
    e.preventDefault();
    clearError('profile-edit-error');
    const data = {
      id: activeProfile.id,
      name: document.getElementById('pe-name').value.trim(),
      startingWeight: parseFloat(document.getElementById('pe-sw').value),
      goalWeight: parseFloat(document.getElementById('pe-gw').value),
      startDate: activeProfile.startDate,
      fitnessLevel: document.getElementById('pe-fl').value,
      injuries: document.getElementById('pe-inj').value.trim(),
      goals: document.getElementById('pe-goals').value.trim(),
    };
    if (!data.name) { showError('profile-edit-error', 'Name is required.'); return; }
    if (isNaN(data.startingWeight) || isNaN(data.goalWeight)) {
      showError('profile-edit-error', 'Starting and goal weights are required.');
      return;
    }
    const r = await Bridge.call('updateProfile', data);
    if (!r.ok) { showError('profile-edit-error', r.data?.detail || r.data); return; }
    activeProfile = r.data;
    updateProfileUI();
    renderProfile();
  });
}
```

- [ ] **Step 4: Update the initialization at the bottom of `app.js`**

Find:
```javascript
initTheme();
initProfile().then(() => navigate('dashboard'));
```

Replace with:
```javascript
initTheme();
initAuth().then(authenticated => {
  if (authenticated) navigate('dashboard');
});
```

- [ ] **Step 5: Remove the `deleteProfileById` function** (no longer needed — search for `async function deleteProfileById` and remove the entire function).

- [ ] **Step 6: Verify build**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat: replace initProfile with initAuth, add login/logout flow, simplify profile view"
```

---

## Task 14: Smoke Test

This task verifies the full auth flow end-to-end.

- [ ] **Step 1: Set the admin key for local development**

Set the environment variable before starting the server (or add to `appsettings.Development.json`):

Option A — environment variable (PowerShell):
```powershell
$env:Admin__ApiKey = "dev-secret-key-12345"
```

Option B — add to `WeightLossTracker/appsettings.Development.json`:
```json
{
  "Admin": {
    "ApiKey": "dev-secret-key-12345"
  }
}
```

- [ ] **Step 2: Start the server**

```bash
cd C:/Development/Weightloss/WeightLossTracker
dotnet run
```

Expected: app starts at `http://localhost:5000`. The DB is recreated from scratch (clean slate).

- [ ] **Step 3: Create a test user via the admin endpoint**

```bash
curl -s -X POST http://localhost:5000/api/admin/users \
  -H "Content-Type: application/json" \
  -H "X-Admin-Key: dev-secret-key-12345" \
  -d '{
    "username": "andre",
    "password": "TestPass123!",
    "profile": {
      "name": "Andre",
      "startingWeight": 215.0,
      "goalWeight": 190.0,
      "startDate": "2026-04-17T00:00:00Z",
      "fitnessLevel": "Beginner",
      "injuries": "Neck injury, lower back injury",
      "goals": "Lose 25 lbs"
    }
  }'
```

Expected response:
```json
{"userId":1,"profileId":1,"username":"andre"}
```

- [ ] **Step 4: Verify wrong admin key returns 403**

```bash
curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:5000/api/admin/users \
  -H "X-Admin-Key: wrong-key" \
  -H "Content-Type: application/json" \
  -d '{}'
```

Expected: `403`

- [ ] **Step 5: Open the app in a browser**

Navigate to `http://localhost:5000`. The login view should appear (full-screen overlay).

- [ ] **Step 6: Log in with the test user**

Enter `andre` / `TestPass123!` and click Log in. Expected: login overlay disappears, dashboard renders, sidebar shows "andre" username.

- [ ] **Step 7: Verify session persists on refresh**

Refresh the page. Expected: app loads directly to dashboard (no login screen) — session cookie is still valid.

- [ ] **Step 8: Verify logout works**

Click the Log out button in the sidebar. Expected: login overlay reappears.

- [ ] **Step 9: Verify unauthenticated API calls return 401**

In a new browser tab or curl (no session cookie), call a protected endpoint:

```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/dashboard
```

Expected: `401`

- [ ] **Step 10: Verify the Profile view works**

Log in again, navigate to Profile. Expected: shows only edit form for "Andre" profile (no create/delete/switch UI).

- [ ] **Step 11: Final commit if any tweaks were made**

```bash
git add -p
git commit -m "fix: smoke test corrections"
```

---

## Post-Implementation Notes

**Setting Admin key in production:**
Set the environment variable `Admin__ApiKey` to a strong random secret (e.g., 32+ character random string). Never put the real key in `appsettings.json`.

**HTTPS requirement:**
The cookie is configured with `SecurePolicy = Always`, which requires HTTPS in production. Configure your hosting environment (reverse proxy, Let's Encrypt, etc.) to serve over HTTPS.

**Future social login:**
To add Google OAuth, add `.AddGoogle(opt => { ... })` on the existing auth builder in `Program.cs`. The claims structure, `GetProfileId`, and frontend auth guard need no changes.
