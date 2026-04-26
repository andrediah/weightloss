# Timezone-Correct Timestamp Display Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ensure all `DateTime` values read from SQLite are tagged as UTC so `System.Text.Json` serializes them with a `Z` suffix and the browser displays times in the user's local timezone.

**Architecture:** Add a `UtcDateTimeConverter : ValueConverter<DateTime, DateTime>` to `AppDbContext.cs` and register it via `ConfigureConventions` — one change fixes every timestamp in the app. No migration, no frontend changes.

**Tech Stack:** C# / EF Core 9 / SQLite / xUnit

---

## Files

| Action | Path | Change |
|---|---|---|
| Modify | `WeightLossTracker/Data/AppDbContext.cs` | Add `UtcDateTimeConverter` class + `ConfigureConventions` override |
| Modify | `WeightLossTracker.Tests/AppDbContextTests.cs` | New test file for the converter |

---

### Task 1: Add UtcDateTimeConverter and wire it up in AppDbContext

**Files:**
- Modify: `WeightLossTracker/Data/AppDbContext.cs`
- Create: `WeightLossTracker.Tests/AppDbContextTests.cs`

- [ ] **Step 1: Write the failing test**

  Create `WeightLossTracker.Tests/AppDbContextTests.cs` with this content:

  ```csharp
  using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
  using WeightLossTracker.Data;
  using Xunit;

  namespace WeightLossTracker.Tests;

  public class UtcDateTimeConverterTests
  {
      private readonly Func<DateTime, DateTime> _convertFromProvider;

      public UtcDateTimeConverterTests()
      {
          var converter = new UtcDateTimeConverter();
          _convertFromProvider = (Func<DateTime, DateTime>)
              converter.ConvertFromProviderExpression.Compile();
      }

      [Fact]
      public void ConvertFromProvider_SetsKindToUtc()
      {
          var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Unspecified);
          var result = _convertFromProvider(input);
          Assert.Equal(DateTimeKind.Utc, result.Kind);
      }

      [Fact]
      public void ConvertFromProvider_PreservesDateAndTimeValue()
      {
          var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Unspecified);
          var result = _convertFromProvider(input);
          Assert.Equal(input.Ticks, result.Ticks);
      }

      [Fact]
      public void ConvertFromProvider_AlreadyUtc_RemainsUtc()
      {
          var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Utc);
          var result = _convertFromProvider(input);
          Assert.Equal(DateTimeKind.Utc, result.Kind);
      }

      [Fact]
      public void ConvertToProvider_PassesThroughUnchanged()
      {
          var converter = new UtcDateTimeConverter();
          var convertToProvider = (Func<DateTime, DateTime>)
              converter.ConvertToProviderExpression.Compile();
          var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Utc);
          var result = convertToProvider(input);
          Assert.Equal(input, result);
          Assert.Equal(DateTimeKind.Utc, result.Kind);
      }
  }
  ```

- [ ] **Step 2: Run test to verify it fails**

  ```bash
  dotnet test WeightLossTracker.Tests/ --filter "UtcDateTimeConverterTests" -v
  ```

  Expected: compile error — `UtcDateTimeConverter` not found.

- [ ] **Step 3: Add UtcDateTimeConverter and ConfigureConventions to AppDbContext**

  Open `WeightLossTracker/Data/AppDbContext.cs`. Add the following using at the top:

  ```csharp
  using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
  ```

  Add the `ConfigureConventions` override inside the `AppDbContext` class, after the existing `OnModelCreating` method:

  ```csharp
  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
      configurationBuilder.Properties<DateTime>()
          .HaveConversion<UtcDateTimeConverter>();
  }
  ```

  Add the `UtcDateTimeConverter` class at the bottom of the file, outside the `AppDbContext` class but inside the `WeightLossTracker.Data` namespace:

  ```csharp
  public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
  {
      public UtcDateTimeConverter() : base(
          v => v,
          v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
      { }
  }
  ```

  The final file should look like:

  ```csharp
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
      public DbSet<BloodPressureEntry> BloodPressureEntries => Set<BloodPressureEntry>();

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

          modelBuilder.Entity<BloodPressureEntry>()
              .HasOne(b => b.UserProfile)
              .WithMany()
              .HasForeignKey(b => b.UserProfileId)
              .OnDelete(DeleteBehavior.Cascade);
      }

      protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
      {
          configurationBuilder.Properties<DateTime>()
              .HaveConversion<UtcDateTimeConverter>();
      }
  }

  public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
  {
      public UtcDateTimeConverter() : base(
          v => v,
          v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
      { }
  }
  ```

- [ ] **Step 4: Run the new tests to verify they pass**

  ```bash
  dotnet test WeightLossTracker.Tests/ --filter "UtcDateTimeConverterTests" -v
  ```

  Expected output:
  ```
  Passed!  - Failed: 0, Passed: 4, Skipped: 0
  ```

- [ ] **Step 5: Run the full test suite**

  ```bash
  dotnet test WeightLossTracker.Tests/ -v
  ```

  Expected output:
  ```
  Passed!  - Failed: 0, Passed: 9, Skipped: 0
  ```

  (5 existing AuthService tests + 4 new converter tests)

- [ ] **Step 6: Verify the app builds**

  ```bash
  dotnet build WeightLossTracker/
  ```

  Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 7: Commit**

  ```bash
  git add WeightLossTracker/Data/AppDbContext.cs WeightLossTracker.Tests/AppDbContextTests.cs
  git commit -m "fix: tag all DateTime values as UTC on EF Core read to fix timezone display"
  ```
