# Blood Pressure Logging — Design Spec

| Field | Value |
|---|---|
| **Date** | 2026-04-24 |
| **Branch** | feature/blood-pressure |
| **Status** | Approved |

---

## 1. Overview

Add blood pressure logging to the Weight Loss Tracker. Readings (systolic, diastolic, pulse, optional notes, timestamp) are stored per user profile and viewable historically with a trend chart and table. The existing "Weight Log" nav section is renamed "Vitals" and gains an internal tab bar — Weight and Blood Pressure — keeping the mobile tab bar at 6 items. The dashboard gains a 4th stat card showing the latest BP reading.

---

## 2. Scope

**In scope:**
- `BloodPressureEntry` model + EF Core migration
- Three new API endpoints (list, create, delete)
- Dashboard endpoint updated to include latest BP
- Frontend: Vitals tab router, Blood Pressure tab (form + chart + table)
- Dashboard: 4th stat card (Latest BP)

**Not in scope:**
- AI analysis of BP readings
- BP targets / threshold alerts
- Export or sharing
- Edit existing entries (delete + re-enter is sufficient for now)

---

## 3. Data Model

### `BloodPressureEntry`

| Column | Type | Constraints |
|---|---|---|
| `Id` | int | PK, auto-increment |
| `ProfileId` | int | FK → `UserProfile.Id`, cascade delete |
| `Systolic` | int | required, 60–250 mmHg |
| `Diastolic` | int | required, 40–150 mmHg |
| `Pulse` | int | required, 30–200 bpm |
| `Notes` | string? | nullable, max 500 chars |
| `RecordedAt` | DateTime | required, UTC; defaults to `DateTime.UtcNow` on server if not supplied |

`ProfileId` foreign key ensures entries are scoped per profile and cascade-deleted when the profile is removed.

---

## 4. API Endpoints

All endpoints require authentication (same cookie-based auth as existing routes). Profile is resolved from the authenticated user's active profile.

### `GET /api/bp`
Returns all blood pressure entries for the active profile, ordered by `RecordedAt` descending.

**Response:**
```json
[
  {
    "id": 1,
    "systolic": 128,
    "diastolic": 84,
    "pulse": 72,
    "notes": "Morning reading",
    "recordedAt": "2026-04-24T08:30:00Z"
  }
]
```

Returns `[]` when no entries exist. Returns `404` if no active profile found.

### `POST /api/bp`
Creates a new blood pressure entry.

**Request body:**
```json
{
  "systolic": 128,
  "diastolic": 84,
  "pulse": 72,
  "notes": "Morning reading",
  "recordedAt": "2026-04-24T08:30:00Z"
}
```

- `systolic`, `diastolic`, `pulse` are required; returns `400` if missing or out of range
- `notes` is optional; omit or pass `null`
- `recordedAt` is optional; defaults to `DateTime.UtcNow` on server if not provided. When the client sends it (from the `datetime-local` input), the value is the user's local time — the frontend appends the local UTC offset before sending so the server receives a full ISO 8601 timestamp (e.g. `2026-04-24T08:30:00-05:00`). The server stores as UTC.
- Returns `201` with the created entry on success

### `DELETE /api/bp/{id}`
Deletes an entry. Returns `404` if not found or not owned by the active profile. Returns `204` on success.

### Dashboard endpoint update (`GET /api/dashboard`)
Response gains a `latestBp` field:

```json
{
  "latestBp": {
    "systolic": 128,
    "diastolic": 84,
    "pulse": 72,
    "recordedAt": "2026-04-24T08:30:00Z"
  }
}
```

`latestBp` is `null` when no entries exist. All existing dashboard fields are unchanged.

---

## 5. Frontend — Navigation

### `index.html`
- Sidebar nav button: `data-nav="vitals"`, label "Vitals", keep existing bar-chart SVG icon
- Mobile tab bar button: `data-mobile-nav="vitals"`, label "Vitals"
- Both replace the current `data-nav="weight"` / `data-mobile-nav="weight"` buttons

Mobile tab count stays at 6.

---

## 6. Frontend — Vitals Router

### `renderVitals(activeTab = 'weight')`
Thin tab-router function registered in the views map as `vitals: renderVitals`.

Renders a 2-tab bar at the top of the view:
- **Weight** tab — calls `renderWeightContent(container)`
- **Blood Pressure** tab — calls `renderBloodPressureContent(container)`

Tab switching re-renders only the inner content div; does not call `navigate()`. Active tab highlighted with `--color-accent` bottom border and text.

### `renderWeightContent(container)`
The existing `renderWeight` function body, refactored to accept a container element and render into it. No behaviour changes.

---

## 7. Frontend — Blood Pressure Tab

### Log form
Rendered as a `C.card` at the top of the BP tab.

Fields:
| Field | Input type | Required | Validation |
|---|---|---|---|
| Systolic | `number` | Yes | 60–250 |
| Diastolic | `number` | Yes | 40–150 |
| Pulse | `number` | Yes | 30–200 |
| Notes | `text` | No | max 500 chars |
| Date/time | `datetime-local` | Yes | defaults to current local datetime |

- Submit button disabled until all 3 required number fields are non-empty and in range
- Inline error message shown on out-of-range or API failure
- On success: form resets, chart and table refresh

### Trend chart
Chart.js line chart showing last 14 readings, oldest → newest (left → right).

- Two datasets:
  - Systolic: `--color-accent`, solid line, `tension: 0.3`, `pointRadius: 3`
  - Diastolic: `--color-accent-hover`, solid line, `tension: 0.3`, `pointRadius: 3`
- Grid lines: `--color-border-subtle`, `lineWidth: 0.5`
- Axis tick color: `--color-text-secondary`, `font.size: 11`
- Y-axis label: `mmHg`
- Legend: top, shows "Systolic" and "Diastolic" labels
- Transparent background
- Colors read from `getComputedStyle` at chart build time (tracks light/dark mode)
- Hidden and replaced by empty-state message when no entries exist

### History table
Scrollable table below the chart, newest entry first.

Columns: Date/Time · Sys/Dia · Pulse · Notes · Delete

- **Date/Time**: formatted as `Apr 24, 8:30 AM` using `fmtDateTime()`
- **Sys/Dia**: rendered as `128/84 mmHg`, weight 600, `--color-text-primary`
- **Pulse**: `72 bpm`, `--color-text-secondary`
- **Notes**: truncated at 40 chars if long, `--color-text-disabled`
- **Delete**: small danger-style button (`×`), calls DELETE endpoint, refreshes view on success
- Alternating row backgrounds: transparent / `--color-row-alt`
- Row border-bottom: `--color-border-subtle`
- Empty state: `"No blood pressure readings yet. Log your first reading above."`

---

## 8. Frontend — Dashboard Update

The 3-card stats banner becomes a **4-card grid**:

| Card | Value | Label | Notes |
|---|---|---|---|
| Current weight | `205.4 lbs` | `CURRENT` | unchanged |
| Lost | `−9.6 lbs` | `LOST` | unchanged |
| To go | `15.4 lbs` | `TO GO` | unchanged |
| Latest BP | `128/84` | `BLOOD PRESSURE` | pulse shown below in `--color-text-disabled` |

- Grid: `grid-cols-2 md:grid-cols-4`
- BP value color: `--color-text-primary`
- Pulse secondary line: `72 bpm` in `--color-text-disabled`, smaller font
- If `d.latestBp` is `null`: value shows `—`, no pulse line
- `recordedAt` not shown on the card (keep it compact)

---

## 9. Files Affected

| File | Change |
|---|---|
| `WeightLossTracker/Models/Models.cs` | Add `BloodPressureEntry` class |
| `WeightLossTracker/Data/AppDbContext.cs` | Add `DbSet<BloodPressureEntry>`, configure FK |
| `WeightLossTracker/Program.cs` | Add 3 BP endpoints, update dashboard response |
| `WeightLossTracker/Migrations/` | New migration for `BloodPressureEntries` table |
| `WeightLossTracker/wwwroot/index.html` | Rename weight→vitals in sidebar + mobile tab bar |
| `WeightLossTracker/wwwroot/js/app.js` | `renderVitals`, `renderWeightContent`, `renderBloodPressureContent`, dashboard 4-card grid |

No new files outside of the migration pair.
