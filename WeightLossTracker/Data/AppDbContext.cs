using Microsoft.EntityFrameworkCore;
using WeightLossTracker.Models;

namespace WeightLossTracker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<WeightEntry> WeightEntries => Set<WeightEntry>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<WorkoutScheduleDay> WorkoutScheduleDays => Set<WorkoutScheduleDay>();
    public DbSet<ExerciseSuggestion> ExerciseSuggestions => Set<ExerciseSuggestion>();
    public DbSet<AiPromptLog> AiPromptLogs => Set<AiPromptLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        // Seed UserProfile
        modelBuilder.Entity<UserProfile>().HasData(new UserProfile
        {
            Id = 1,
            Name = "Default",
            StartingWeight = 215.0,
            GoalWeight = 190.0,
            StartDate = new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc),
            FitnessLevel = "Beginner",
            Injuries = "Neck injury, lower back injury",
            Goals = "Lose 25 lbs, build muscle"
        });

        // Seed WorkoutSchedule: Mon–Fri = Home, Sat/Sun = Rest
        modelBuilder.Entity<WorkoutScheduleDay>().HasData(
            new WorkoutScheduleDay { Id = 1, UserProfileId = 1, DayOfWeek = 0, Location = "Rest" },   // Sunday
            new WorkoutScheduleDay { Id = 2, UserProfileId = 1, DayOfWeek = 1, Location = "Home" },   // Monday
            new WorkoutScheduleDay { Id = 3, UserProfileId = 1, DayOfWeek = 2, Location = "Home" },   // Tuesday
            new WorkoutScheduleDay { Id = 4, UserProfileId = 1, DayOfWeek = 3, Location = "Home" },   // Wednesday
            new WorkoutScheduleDay { Id = 5, UserProfileId = 1, DayOfWeek = 4, Location = "Home" },   // Thursday
            new WorkoutScheduleDay { Id = 6, UserProfileId = 1, DayOfWeek = 5, Location = "Home" },   // Friday
            new WorkoutScheduleDay { Id = 7, UserProfileId = 1, DayOfWeek = 6, Location = "Rest" }    // Saturday
        );
    }
}
