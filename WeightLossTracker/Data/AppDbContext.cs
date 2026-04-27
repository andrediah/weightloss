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

        // User -> UserProfile 1:1 (User has no inverse navigation — profile is looked up by UserId)
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
