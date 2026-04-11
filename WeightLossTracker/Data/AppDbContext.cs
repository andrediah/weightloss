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
        // Unique index: one weight entry per calendar day
        modelBuilder.Entity<WeightEntry>()
            .HasIndex(w => w.Date)
            .IsUnique();

        // Unique index: one schedule entry per day of week
        modelBuilder.Entity<WorkoutScheduleDay>()
            .HasIndex(s => s.DayOfWeek)
            .IsUnique();

        // ExerciseSuggestion -> AiPromptLog 1:1, restrict delete
        modelBuilder.Entity<ExerciseSuggestion>()
            .HasIndex(e => e.AiPromptLogId)
            .IsUnique();

        modelBuilder.Entity<ExerciseSuggestion>()
            .HasOne(e => e.AiPromptLog)
            .WithOne()
            .HasForeignKey<ExerciseSuggestion>(e => e.AiPromptLogId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed UserProfile
        modelBuilder.Entity<UserProfile>().HasData(new UserProfile
        {
            Id = 1,
            StartingWeight = 215.0,
            GoalWeight = 190.0,
            StartDate = new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc),
            FitnessLevel = "Beginner",
            Injuries = "Neck injury, lower back injury",
            Goals = "Lose 25 lbs, build muscle"
        });

        // Seed WorkoutSchedule: Mon–Fri = Home, Sat/Sun = Rest
        modelBuilder.Entity<WorkoutScheduleDay>().HasData(
            new WorkoutScheduleDay { Id = 1, DayOfWeek = 0, Location = "Rest" },   // Sunday
            new WorkoutScheduleDay { Id = 2, DayOfWeek = 1, Location = "Home" },   // Monday
            new WorkoutScheduleDay { Id = 3, DayOfWeek = 2, Location = "Home" },   // Tuesday
            new WorkoutScheduleDay { Id = 4, DayOfWeek = 3, Location = "Home" },   // Wednesday
            new WorkoutScheduleDay { Id = 5, DayOfWeek = 4, Location = "Home" },   // Thursday
            new WorkoutScheduleDay { Id = 6, DayOfWeek = 5, Location = "Home" },   // Friday
            new WorkoutScheduleDay { Id = 7, DayOfWeek = 6, Location = "Rest" }    // Saturday
        );
    }
}
