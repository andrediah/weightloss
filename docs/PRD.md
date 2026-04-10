# Product Requirements Document
## Weight Loss Tracker

| Field | Value |
|---|---|
| **Version** | 1.0 |
| **Date** | 2026-04-10 |
| **Status** | Active Development |
| **Stack** | ASP.NET Core 10 · SQLite · Claude API · Tailwind CSS · Chart.js |

---

## 1. Overview

Weight Loss Tracker is a personal, self-hosted web application that helps a single user track their weight-loss journey from **215 lbs to 190 lbs**. The app combines daily weight logging, AI-generated workout plans, meal logging with nutrition advice, and a full audit log of every AI interaction — all in a clean single-page UI served by a local ASP.NET Core server.

---

## 2. Goals & Non-Goals

### Goals
- Provide a frictionless daily check-in loop: log weight → log meals → review workout
- Leverage Claude AI to generate personalised, injury-safe workout plans
- Give AI-powered nutrition guidance in conversational plain language
- Keep all data local (SQLite on-device) with no cloud sync or account required
- Maintain a complete, queryable history of every prompt sent to the AI

### Non-Goals
- Multi-user support
- Mobile native app
- Calorie/macro calculation engine (delegated entirely to Claude)
- Social / sharing features
- Wearable device integrations

---

## 3. User Profile

| Attribute | Value |
|---|---|
| Starting weight | 215 lbs |
| Goal weight | 190 lbs |
| Target loss | 25 lbs |
| Fitness level | Beginner |
| Injuries | Neck injury · Lower back injury |
| Primary goals | Lose weight · Build muscle |
| Start date | 2026-04-09 |

All AI prompts embed this profile to ensure suggestions are safe, relevant, and appropriately scaled.

---

## 4. Architecture

```
┌──────────────────────────────────────────────────────────┐
│  Browser  (localhost)                                     │
│  index.html · js/api.js · js/app.js                      │
│  Tailwind CSS (CDN) · Chart.js (CDN)                     │
└────────────────────┬─────────────────────────────────────┘
                     │  HTTP/REST  (fetch)
┌────────────────────▼─────────────────────────────────────┐
│  ASP.NET Core 10 Minimal API  (Program.cs)                │
│                                                           │
│  Services                                                 │
│  ├─ ClaudeService    — Anthropic API calls + logging      │
│  ├─ ExerciseService  — workout generation & history       │
│  └─ MealService      — meal CRUD + nutrition advice       │
│                                                           │
│  Data                                                     │
│  └─ AppDbContext (EF Core → SQLite)                       │
└────────────────────┬─────────────────────────────────────┘
                     │
┌────────────────────▼─────────────────────────────────────┐
│  SQLite  (%LOCALAPPDATA%\WeightLossTracker\tracker.db)    │
└──────────────────────────────────────────────────────────┘
                     │
┌────────────────────▼─────────────────────────────────────┐
│  Anthropic API  (claude-sonnet-4-6)                       │
└──────────────────────────────────────────────────────────┘
```

### Key Design Decisions
- **No build step**: Tailwind CSS and Chart.js are loaded via CDN; the JS is vanilla ES2023.
- **Single HTML file + two JS files**: keeps the frontend deployable without a bundler.
- **Minimal API**: all endpoints live in `Program.cs` for maximum visibility in a single-developer project.
- **EF Core migrations**: schema is versioned and applied automatically on startup.
- **API key in `appsettings.json`**: configured once locally; excluded from source control via `.gitignore` (see §10).

---

## 5. Data Model

### `UserProfile`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | Seeded as 1 (single user) |
| StartingWeight | double | 215.0 |
| GoalWeight | double | 190.0 |
| StartDate | DateTime | 2026-04-09 |
| FitnessLevel | string | "Beginner" |
| Injuries | string | "Neck injury, lower back injury" |
| Goals | string | "Lose 25 lbs, build muscle" |

### `WeightEntry`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Date | DateTime | Unique per calendar day (upsert enforced at service layer + DB unique index on date) |
| Weight | double | lbs |
| Notes | string? | Free text |

### `MealLog`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Date | DateTime | |
| MealType | string | Breakfast / Lunch / Dinner / Snack |
| Description | string | |
| Calories | int? | Optional |
| Notes | string? | |

### `WorkoutScheduleDay`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| DayOfWeek | int | 0 = Sunday … 6 = Saturday; unique index enforced |
| Location | string | Rest / Home / Gym |

Default seed: Mon–Fri → Home, Sat/Sun → Rest. No FK to `UserProfile` — single-user app by design (Id 1 is always the implicit owner).

### `ExerciseSuggestion`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| CreatedAt | DateTime | UTC |
| DayOfWeek | int? | 0–6 |
| Location | string | Home / Gym |
| Category | string | Cardio / Strength / Flexibility |
| Content | string | Full Claude response |
| AiPromptLogId | int FK → AiPromptLog | 1:1 (unique index enforced). Restrict delete — `AiPromptLog` rows may not be deleted directly while a linked `ExerciseSuggestion` exists; use `DELETE /api/exercise/history/{id}` to remove both in a transaction |

### `AiPromptLog`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| CreatedAt | DateTime | UTC |
| PromptType | string | Exercise / Meal / General |
| Prompt | string | Full prompt sent |
| Response | string | Full Claude response |
| Model | string | e.g. claude-sonnet-4-6 |
| InputTokens | int | From API usage |
| OutputTokens | int | From API usage |

---

## 6. API Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/dashboard` | Aggregated stats + chart data |
| GET | `/api/weight` | All weight entries (desc) |
| POST | `/api/weight` | Upsert today's weight entry |
| PUT | `/api/weight/{id}` | Edit an existing weight entry (weight + notes); same 50–500 lbs validation as POST; returns `404` if not found |
| DELETE | `/api/weight/{id}` | Remove a weight entry |
| GET | `/api/schedule` | Weekly workout schedule |
| PUT | `/api/schedule` | Update schedule locations |
| POST | `/api/exercise/generate-day` | AI workout for one day |
| POST | `/api/exercise/generate-week` | AI workouts for all active days |
| GET | `/api/exercise/history` | Past suggestions; optional `?dayOfWeek=0-6` query param |
| DELETE | `/api/exercise/history/{id}` | Remove an exercise suggestion and its linked `AiPromptLog` in a single transaction; returns `404` if not found |
| GET | `/api/meals/today` | Today's meal log |
| POST | `/api/meals` | Add a meal |
| DELETE | `/api/meals/{id}` | Remove a meal |
| POST | `/api/meals/advice` | AI nutrition advice |
| GET | `/api/ai-history` | All AI prompt logs; optional `?type=Exercise` or `Meal` or `General` query param |
| DELETE | `/api/ai-history/{id}` | Remove an AI prompt log; returns `409 Conflict` if a linked `ExerciseSuggestion` still exists, `404` if not found |

---

## 7. Features

### 7.1 Dashboard
- Four KPI stat cards: Current Weight · Lost So Far · To Goal · Days Logged
- Goal progress bar (percentage toward 25 lb target)
- Weight trend chart (Chart.js line chart) with three datasets:
  - Actual daily weight (indigo)
  - Linear regression trend line (orange, dashed)
  - Goal weight reference line (green, dashed)

### 7.2 Weight Log
- Log today's weight (lbs) with optional free-text notes
- Upsert behaviour: re-logging on the same day updates the existing entry
- Scrollable history table with per-entry edit (inline) and delete
- Validation: weight must be 50–500 lbs

### 7.3 Exercise Schedule
- Seven-day grid (Monday–Sunday display order) with per-day location picker (Rest / Home / Gym)
- "Save Schedule" persists current selections to DB
- "Generate [Day]" button: calls Claude for a single-day workout; auto-saves schedule first
- "Generate Full Week" button: generates all non-Rest days sequentially; the button is disabled and a progress indicator (e.g. "Generating day 2 of 5…") is shown while each day is being generated; each successful day is saved to the DB and rendered immediately so partial results are never lost; on API error the remaining days are skipped and the error is surfaced inline beneath the last successful result
- Workout suggestion panel renders Claude's markdown response
- Per-day history sidebar lists previous suggestions; click to preview any past workout
- Category badge (Cardio / Strength / Flexibility) rotates automatically to balance the week
- All exercise prompts embed the user's injury profile; Claude is instructed to avoid neck/lower-back loading

### 7.4 Meal Log
- Dual-column layout: meal entry form + AI advice panel
- Add meals with type, description, optional calorie count, and notes
- Running daily calorie total displayed in the table header
- Delete individual meals
- Free-text question box → "Ask Claude" → inline nutrition advice rendered in the advice panel

### 7.5 AI History
- Master-detail layout: filter tabs (All / Exercise / Meal / General) + scrollable list + detail pane
- Detail pane shows full prompt text, full response (rendered markdown), model name, and token counts
- Token usage visible for cost awareness

---

## 8. AI Integration

### Model
`claude-sonnet-4-6` via the Anthropic Messages API (`https://api.anthropic.com/v1/messages`).

### Prompt Strategy

| Feature | Prompt Type | Key Context Injected |
|---|---|---|
| Single-day workout | Exercise | Fitness level, injuries, full weekly schedule, day location, suggested category |
| Full-week workout | Exercise | Same as above, iterated per active day |
| Nutrition advice | Meal | User goal (215→190 lbs), fitness level, injuries, free-text question |

### Category Rotation
Workout category (Cardio → Strength → Flexibility → repeat) is determined by the day's index within the active days of the week, ensuring natural variety without user configuration.

### Token Limits
`max_tokens` defaults to **1 024** for all requests (configurable via `appsettings.json`). This is sufficient for a single-day workout or nutrition response while capping per-call cost. If responses are consistently truncated, increase via the `Claude:MaxTokens` setting — no code change required.

### Error Handling

**Startup:** If `Claude:ApiKey` is missing or empty, the app logs a warning and starts in **degraded mode** — all AI-powered endpoints return `503 Service Unavailable` with the message "Claude API key not configured." Non-AI features (weight logging, meal CRUD, schedule editing) remain fully functional.

**Runtime API errors:** `ClaudeService` catches Anthropic API errors (HTTP 4xx/5xx, network timeouts) and throws a typed `ClaudeApiException`. AI endpoints return `502 Bad Gateway` with a plain-text error message. The frontend surfaces errors inline (not as an alert) and re-enables any disabled buttons.

**Input validation:** All AI-triggering endpoints validate inputs before calling the API. For example, `POST /api/exercise/generate-day` returns `400 Bad Request` if `dayOfWeek` is outside 0–6 or the day is set to Rest. Invalid inputs never reach the Anthropic API.

### Logging
Every API call is recorded in `AiPromptLog` with the full prompt, response, model, and token counts before the response is returned to the client.

---

## 9. Frontend Architecture

```
wwwroot/
├── index.html        — App shell, sidebar nav, Tailwind styles, CDN script tags
└── js/
    ├── api.js        — Bridge.call(action, data) → REST fetch wrapper
    └── app.js        — All view functions + router + shared utilities
```

### Router
`navigate(viewName)` swaps `#view-root` innerHTML, destroys any active Chart.js instance, and updates the active nav link. Views: `dashboard`, `weight`, `exercise`, `meals`, `history`.

### Bridge API
`Bridge.call(action, data)` maps named actions to HTTP calls, maintaining a stable interface between the UI logic and the backend transport.

Complete action map:

| Action | Method | Route |
|---|---|---|
| `getDashboard` | GET | `/api/dashboard` |
| `getWeightEntries` | GET | `/api/weight` |
| `saveWeight` | POST | `/api/weight` |
| `updateWeight` | PUT | `/api/weight/{id}` |
| `deleteWeight` | DELETE | `/api/weight/{id}` |
| `getSchedule` | GET | `/api/schedule` |
| `saveSchedule` | PUT | `/api/schedule` |
| `generateDayWorkout` | POST | `/api/exercise/generate-day` |
| `generateWeekWorkouts` | POST | `/api/exercise/generate-week` |
| `getExerciseHistory` | GET | `/api/exercise/history` |
| `deleteExerciseHistory` | DELETE | `/api/exercise/history/{id}` |
| `getTodayMeals` | GET | `/api/meals/today` |
| `addMeal` | POST | `/api/meals` |
| `deleteMeal` | DELETE | `/api/meals/{id}` |
| `getMealAdvice` | POST | `/api/meals/advice` |
| `getAiHistory` | GET | `/api/ai-history` |
| `deleteAiHistory` | DELETE | `/api/ai-history/{id}` |

All Bridge calls return `{ ok, status, data }`. On non-2xx responses, `ok` is `false` and `data` contains the plain-text error message.

### Error Display
A shared `showError(containerId, message)` utility renders an inline error banner (red background, dismiss button) inside the active view's designated error container. No `alert()` or modal dialogs are used for API errors.

### Inline Weight Editing
Clicking the edit icon on a weight-history row replaces the weight and notes cells with input fields pre-filled with current values. "Save" calls `Bridge.call('updateWeight', ...)` and re-renders the row; "Cancel" restores the original display. Only one row may be in edit mode at a time.

### Markdown Renderer
A lightweight `md(text)` function converts Claude's `##`, `###`, `- `, `**bold**` output to HTML without a library dependency.

---

## 10. Configuration

`appsettings.json` (excluded from source control via `.gitignore`). An `appsettings.example.json` is committed as a template:

```json
{
  "Claude": {
    "ApiKey": "<your-anthropic-api-key>",
    "Model": "claude-sonnet-4-6",
    "MaxTokens": 1024
  }
}
```

`Model` and `MaxTokens` are read by `ClaudeService` at startup, making them configurable without a code change.

Database path: `%LOCALAPPDATA%\WeightLossTracker\tracker.db` — created automatically on first run.

---

## 11. Running the App

### First-time setup

```bash
# 1. Copy the example config and add your Anthropic API key
cp appsettings.example.json appsettings.json
# Edit appsettings.json → set Claude:ApiKey

# 2. Restore & run
dotnet run --project WeightLossTracker
```

The app auto-applies EF Core migrations on startup. If a migration adds a unique index (e.g. on `WeightEntry.Date`), existing duplicate rows will cause the migration to fail — resolve duplicates manually before upgrading.

### Subsequent runs

```bash
dotnet run --project WeightLossTracker
```

Default URL: `http://localhost:5000`

---

## 12. Future Enhancements

| Priority | Feature | Notes |
|---|---|---|
| High | User profile editor | Edit starting weight, goal, injuries in-app instead of via DB seed |
| Medium | Historic meal browsing | View past days' meals, not just today |
| Medium | Weekly summary report | AI-generated weekly review of weight trend + workouts completed |
| Medium | Calorie goal setting | Daily calorie budget with over/under indicator |
| Medium | Workout completion tracking | Mark individual workouts as done; track adherence % |
| Low | Export to CSV | Weight and meal data export for external analysis |
| Low | Dark mode | Toggle between light/dark themes |
| Low | PWA / offline support | Service worker so the app loads without the server running |
| Low | Backup & restore | One-click DB backup/restore via the UI |
