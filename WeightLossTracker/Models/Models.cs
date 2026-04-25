using System.ComponentModel.DataAnnotations;

namespace WeightLossTracker.Models;

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

public class WeightEntry
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public DateTime Date { get; set; }
    public double Weight { get; set; }
    public string? Notes { get; set; }
}

public class MealLog
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public DateTime Date { get; set; }
    public string MealType { get; set; } = "";
    public string Description { get; set; } = "";
    public int? Calories { get; set; }
    public string? Notes { get; set; }
}

public class WorkoutScheduleDay
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public int DayOfWeek { get; set; }
    public string Location { get; set; } = "Rest";
}

public class ExerciseSuggestion
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? DayOfWeek { get; set; }
    public string Location { get; set; } = "";
    public string Category { get; set; } = "";
    public string Content { get; set; } = "";
    public int AiPromptLogId { get; set; }
    public AiPromptLog? AiPromptLog { get; set; }
}

public class AiPromptLog
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PromptType { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string Response { get; set; } = "";
    public string Model { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}

public class BloodPressureEntry
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public int Pulse { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; }
    [MaxLength(20)]
    public string? Category { get; set; }
}
