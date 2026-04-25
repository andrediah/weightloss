# Blood Pressure Category Ranking — Design Spec

**Date:** 2026-04-25
**Branch:** feature/bp

## Overview

Add a clinical category label to each blood pressure reading, stored in the database and displayed as a color-coded badge inline in the history table.

## Clinical Classification (AHA 2017)

A static `BpCategoryService.Classify(int systolic, int diastolic)` method encodes these rules in order:

| Category | Condition | Badge color |
|---|---|---|
| Hypotension | sys < 90 OR dia < 60 | Blue |
| Normal | sys < 120 AND dia < 80 | Green |
| Elevated | sys 120–129 AND dia < 80 | Yellow |
| Stage 1 | sys 130–139 OR dia 80–89 | Orange |
| Stage 2 | sys ≥ 140 OR dia ≥ 90 | Red |
| Crisis | sys > 180 OR dia > 120 | Dark red |

Rules are evaluated top-to-bottom; the first match wins. Crisis is checked last (it is a subset of Stage 2 by sys, so it must be evaluated before Stage 2 catches it — or checked first, before Stage 2).

**Note:** Crisis must be evaluated before Stage 2 since sys > 180 satisfies the Stage 2 condition (sys ≥ 140). Evaluate Crisis first, then Stage 2.

Corrected evaluation order:

1. Crisis: sys > 180 OR dia > 120
2. Hypotension: sys < 90 OR dia < 60
3. Normal: sys < 120 AND dia < 80
4. Elevated: sys 120–129 AND dia < 80
5. Stage 1: sys 130–139 OR dia 80–89
6. Stage 2: sys ≥ 140 OR dia ≥ 90

## Data Model

Add `Category` (string, nullable, max 20) to `BloodPressureEntry`:

```csharp
public string? Category { get; set; }
```

Generate an EF Core migration to add the nullable column. Existing rows are left null — no backfill.

## Backend

### BpCategoryService

New static class `BpCategoryService` with a single method:

```csharp
public static string Classify(int systolic, int diastolic)
```

Returns one of: `"Crisis"`, `"Hypotension"`, `"Normal"`, `"Elevated"`, `"Stage 1"`, `"Stage 2"`.

### POST /api/bp handler

Call `BpCategoryService.Classify(systolic, diastolic)` before saving the entry, store result in `entry.Category`.

### GET /api/bp

No changes needed — `Category` is serialized automatically as part of the full entity.

## Frontend (app.js)

### bpBadge(category) helper

New function that maps a category string to a colored `<span>` badge. Returns empty string if category is null/undefined (legacy rows show no badge).

Color mapping:
- `"Hypotension"` → blue pill
- `"Normal"` → green pill
- `"Elevated"` → yellow pill
- `"Stage 1"` → orange pill
- `"Stage 2"` → red pill
- `"Crisis"` → dark red pill

### bpRow(e) update

Insert `${bpBadge(e.category)}` inline after the `sys/dia` value in the reading cell. No new column added to the table.

## Error Handling & Edge Cases

- **Existing rows**: `Category` is nullable; null entries render no badge silently.
- **Input validation**: Form already clamps sys (60–250) and dia (40–150), so Classify always receives a number in a valid range.
- **Crisis UI**: Red badge only — no alert or modal in this scope.
- **Tests**: No test suite exists in this project; none added.
