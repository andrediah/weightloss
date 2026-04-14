# Weight Loss Tracker

A personal weight loss companion app with AI-powered workout generation and nutrition advice. Built with ASP.NET Core and vanilla JavaScript, all data stays local on your machine.

## Features

- **Dashboard** -- Track progress with KPI cards (current weight, lost so far, to goal, days logged) and a Chart.js trend line
- **Weight Logging** -- Log daily weigh-ins with optional notes; one entry per day with inline edit/delete
- **Exercise Scheduling** -- 7-day schedule grid (Rest/Home/Gym); generate personalized, injury-safe workouts via Google Gemini
- **Meal Logging** -- Log meals by type with calorie tracking and daily totals
- **AI Nutrition Advice** -- Ask free-text nutrition questions answered by Gemini with your profile context
- **AI History** -- Full audit trail of every AI interaction with prompt, response, model, and token counts
- **Dark Mode** -- OS-aware with manual toggle, persisted in localStorage
- **Responsive Design** -- Desktop sidebar layout with mobile bottom tab navigation

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10 / C# 14, ASP.NET Core Minimal API |
| Database | SQLite via EF Core 10 (local file, auto-migrated) |
| Frontend | Vanilla JS (ES2023), Tailwind CSS (CDN), Chart.js (CDN) |
| AI | Google Gemini 2.5 Flash (free tier) |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A free Google Gemini API key from [Google AI Studio](https://aistudio.google.com/apikey) (optional -- app works in degraded mode without it)

## Getting Started

```bash
# Clone the repo
git clone https://github.com/andrediah/weightloss.git
cd weightloss

# Copy the example config and add your Gemini API key
cp WeightLossTracker/appsettings.example.json WeightLossTracker/appsettings.json
# Edit appsettings.json and replace <your-google-ai-api-key> with your key

# Run
dotnet run --project WeightLossTracker
```

Open **http://localhost:5000** in your browser.

## Configuration

Edit `WeightLossTracker/appsettings.json`:

```json
{
  "Gemini": {
    "ApiKey": "<your-google-ai-api-key>",
    "Model": "gemini-2.5-flash",
    "MaxOutputTokens": 2048,
    "ExerciseMaxOutputTokens": 4096
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `ApiKey` | *(empty)* | Google Gemini API key. Without it, AI features return 503 but everything else works. |
| `Model` | `gemini-2.5-flash` | Gemini model ID |
| `MaxOutputTokens` | `2048` | Token limit for general/meal AI responses |
| `ExerciseMaxOutputTokens` | `4096` | Token limit for workout generation |

## Database

SQLite database is stored at `%LOCALAPPDATA%\WeightLossTracker\tracker.db` and auto-created on first run. Migrations apply automatically at startup -- no manual `dotnet ef` commands needed. If the database is in a partial state (e.g. app was killed mid-migration), it is detected and recreated from scratch.

## Project Structure

```
WeightLossTracker/
  Program.cs                 # App startup + all API endpoints (minimal API)
  Models/Models.cs           # EF Core entity classes
  Data/AppDbContext.cs       # DbContext with seed data
  Services/
    GeminiService.cs         # Gemini REST API client with logging
    GeminiOptions.cs         # Typed configuration for Gemini
    ExerciseService.cs       # Workout generation with injury-aware prompts
    MealService.cs           # Nutrition advice with user profile context
    ServiceCollectionExtensions.cs  # DI registration
  Migrations/                # EF Core migrations
  wwwroot/
    index.html               # App shell (sidebar + mobile tabs)
    js/api.js                # HTTP client (Bridge API)
    js/app.js                # Views, router, and UI logic
docs/
  PRD.md                     # Product requirements
  coding-standards.md        # .NET 10 / C# 14 conventions
  ux-standards.md            # Design system and accessibility guidelines
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/dashboard` | Aggregated stats and chart data |
| GET | `/api/weight` | All weight entries |
| POST | `/api/weight` | Log or update today's weight |
| PUT | `/api/weight/{id}` | Edit a weight entry |
| DELETE | `/api/weight/{id}` | Remove a weight entry |
| GET | `/api/schedule` | Weekly workout schedule |
| PUT | `/api/schedule` | Update schedule locations |
| POST | `/api/exercise/generate-day` | Generate workout for one day |
| POST | `/api/exercise/generate-week` | Generate workouts for all active days |
| GET | `/api/exercise/history` | Past exercise suggestions |
| DELETE | `/api/exercise/history/{id}` | Delete a suggestion |
| GET | `/api/meals/today` | Today's meal log |
| POST | `/api/meals` | Add a meal |
| DELETE | `/api/meals/{id}` | Remove a meal |
| POST | `/api/meals/advice` | Get AI nutrition advice |
| GET | `/api/ai-history` | AI interaction logs |
| DELETE | `/api/ai-history/{id}` | Remove a log entry |

## License

This project is for personal use.
