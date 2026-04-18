# User Authentication Design

**Date:** 2026-04-17
**Branch:** feature/security
**Status:** Approved

## Overview

Add username/password authentication to the WeightLossTracker app. The app will be hosted for multiple users; each authenticated user is associated with exactly one `UserProfile`. Accounts are admin-created only (no public registration). Sessions use ASP.NET Core Cookie Authentication. The design is intentionally extensible for social login (e.g., Google OAuth) as a future enhancement.

## Data Model

### New `User` table

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
```

### `UserProfile` changes

- Add `UserId` (int, FK to `User`, unique) — 1:1 relationship
- The `X-Profile-Id` header approach is removed; profile is resolved server-side from the authenticated user's claims

### Migration strategy

- **Clean slate:** drop and recreate the SQLite database
- One new EF Core migration adds the `User` table and `UserId` column on `UserProfile`
- The existing auto-reset logic in `Program.cs` handles clean slate on startup

## Dependencies

- `BCrypt.Net-Next` NuGet package — password hashing

## Authentication Middleware

Added to `Program.cs`:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt => {
        opt.LoginPath = null;                                    // return 401, not redirect
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.Strict;
        opt.ExpireTimeSpan = TimeSpan.FromDays(14);
        opt.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
```

`app.UseAuthentication()` and `app.UseAuthorization()` added before route registration.

### Claims

On login, the following claims are stored in the cookie:
- `ClaimTypes.NameIdentifier` → `User.Id`
- `ClaimTypes.Name` → `User.Username`

### Profile resolution

`GetProfileId()` is replaced by a helper that looks up `UserProfile.UserId` from the authenticated user's `NameIdentifier` claim. All existing endpoints use this new helper unchanged.

All existing API routes get `.RequireAuthorization()`.

## API Endpoints

### Auth (unauthenticated)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/login` | Accepts `{ username, password }`. Verifies BCrypt hash. Signs in via `HttpContext.SignInAsync()`. Returns 200 or 401. |
| POST | `/api/auth/logout` | Calls `HttpContext.SignOutAsync()`. Returns 200. |
| GET | `/api/auth/me` | Returns `{ username, profileId }` for logged-in user, or 401. |

### Admin (unauthenticated, key-protected)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/admin/users` | Creates a `User` + linked `UserProfile` in one transaction. Protected by `X-Admin-Key` header checked against `Admin:ApiKey` in app config. Returns 403 if key doesn't match. |

**Request body for `/api/admin/users`:**
```json
{
  "username": "andre",
  "password": "plaintext-hashed-on-server",
  "profile": {
    "name": "Andre",
    "startingWeight": 215.0,
    "goalWeight": 190.0,
    "startDate": "2026-04-17",
    "fitnessLevel": "beginner",
    "injuries": "neck, lower back",
    "goals": "lose 25 lbs"
  }
}
```

**`appsettings.json` addition:**
```json
"Admin": {
  "ApiKey": ""
}
```
The actual key is set via environment variable (`Admin__ApiKey`) and must never be committed.

## Frontend Changes

### Login page

- A login view is shown when the user is unauthenticated; it replaces the main app content inside `index.html` (no separate page — the JS auth guard swaps views)
- Fields: username, password, submit button
- On success (200) → shows the main app
- On failure (401) → displays "Invalid username or password"

### Auth guard (`app.js`)

- On startup, calls `GET /api/auth/me`
- 401 → show login view
- 200 → store `profileId` from response, show main app

### API calls (`api.js`)

- Remove all `X-Profile-Id` header usage (profile resolved server-side)
- Any 401 response → redirect to login view

### Logout

- Logout button in the app header
- Calls `POST /api/auth/logout` → shows login view

### Profile switcher

- Removed entirely; each authenticated user has exactly one profile

## Security Notes

- Passwords are hashed with BCrypt before storage; plaintext is never persisted
- Cookie is `HttpOnly`, `Secure`, `SameSite=Strict`
- Admin key is read from environment variable only; never committed to source control
- HTTPS must be configured in production (required for `SecurePolicy.Always`)

## Future: Social Login

Adding Google OAuth later requires only:
1. `.AddGoogle(opt => { ... })` on the existing auth builder
2. A new `POST /api/auth/google-callback` endpoint
3. An optional `ExternalLoginId` column on `User`

No restructuring of sessions, profile resolution, or frontend auth guard needed.
