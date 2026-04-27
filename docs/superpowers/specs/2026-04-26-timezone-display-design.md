# Timezone-Correct Timestamp Display — Design Spec

**Date:** 2026-04-26
**Status:** Approved

## Problem

The backend stores `DateTime` values in SQLite as ISO 8601 TEXT. EF Core reads them back with `DateTimeKind.Unspecified`. `System.Text.Json` serializes `Unspecified` datetimes without a `Z` suffix (e.g. `"2026-04-25T19:00:00"`). The browser's `new Date("2026-04-25T19:00:00")` treats a no-suffix string as local time — no UTC conversion occurs. Users in any non-UTC timezone therefore see BP reading times (and all other timestamps) shifted by their UTC offset.

## Solution

Register a EF Core value converter that tags every `DateTime` read from SQLite as `DateTimeKind.Utc`. `System.Text.Json` then serializes with `Z` suffix. The browser correctly converts to local time via the existing `fmtDateTime()` / `fmtDate()` calls, which already use `toLocaleString(undefined, {...})`.

## Architecture

Single file change: `WeightLossTracker/Data/AppDbContext.cs`.

### UtcDateTimeConverter

A `ValueConverter<DateTime, DateTime>` that:
- **Write (C# → DB):** passes the value through unchanged — storage format stays the same
- **Read (DB → C#):** calls `DateTime.SpecifyKind(v, DateTimeKind.Utc)` to tag the value as UTC

### ConfigureConventions

Override `ConfigureConventions(ModelConfigurationBuilder)` in `AppDbContext` to apply `UtcDateTimeConverter` to every `DateTime` property across all entities.

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.Properties<DateTime>()
        .HaveConversion<UtcDateTimeConverter>();
}
```

## File Changes

| File | Change |
|---|---|
| `WeightLossTracker/Data/AppDbContext.cs` | Add `UtcDateTimeConverter` class + `ConfigureConventions` override |

No other files change. No migration required.

## Scope

Fixes all `DateTime` fields app-wide in one change:

| Entity | Field | Display location |
|---|---|---|
| `BloodPressureEntry` | `RecordedAt` | BP history table, BP chart labels |
| `AiPromptLog` | `CreatedAt` | AI History view |
| `ExerciseSuggestion` | `CreatedAt` | Exercise view suggestions |
| `MealLog` | `CreatedAt` | Meals view |
| `UserProfile` | `StartDate` | Profile view |
| `User` | `CreatedAt` | Not displayed |

## No Migration Required

The SQLite TEXT storage format is unchanged. The converter's write-side is an identity function — values are stored in exactly the same format as before. Only the `DateTimeKind` tag on the in-memory C# object changes.

## Out of Scope

- Showing timezone labels in the UI (e.g. "EDT") — times display in local time silently
- `DateTime?` nullable fields — none exist on entity models; request DTOs are not persisted directly
- Weight entries — `WeightEntry.Date` stores a date-only value (`DateTime.UtcNow.Date`) with no time component, so no timezone conversion is relevant for display
