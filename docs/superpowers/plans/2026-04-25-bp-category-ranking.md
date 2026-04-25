# BP Category Ranking Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add AHA 2017 clinical category labels to blood pressure readings — stored in the DB, returned by the API, and displayed as color-coded badges in the history table.

**Architecture:** A static `BpCategoryService.Classify` method in the Services layer computes the category string from systolic/diastolic values. The POST handler calls it before saving; the GET projection includes it. The JS `bpBadge` helper renders the colored pill inline in `bpRow`.

**Tech Stack:** C# / ASP.NET Core minimal API, Entity Framework Core (SQLite), Vanilla JS / Tailwind CSS

---

## File Map

| Action | File | Purpose |
|---|---|---|
| Create | `WeightLossTracker/Services/BpCategoryService.cs` | AHA 2017 classification logic |
| Modify | `WeightLossTracker/Models/Models.cs:76-86` | Add `Category` property to `BloodPressureEntry` |
| Migration | auto-generated | Add nullable `Category` column to DB |
| Modify | `WeightLossTracker/Program.cs:475-516` | Store category on POST; include in GET + Created projections |
| Modify | `WeightLossTracker/wwwroot/js/app.js:711-727` | Add `bpBadge` helper; update `bpRow` |

---

### Task 1: Create BpCategoryService

**Files:**
- Create: `WeightLossTracker/Services/BpCategoryService.cs`

- [ ] **Step 1: Create the service file**

```csharp
// WeightLossTracker/Services/BpCategoryService.cs
public static class BpCategoryService
{
    public static string Classify(int systolic, int diastolic)
    {
        if (systolic > 180 || diastolic > 120) return "Crisis";
        if (systolic < 90  || diastolic < 60)  return "Hypotension";
        if (systolic < 120 && diastolic < 80)  return "Normal";
        if (systolic < 130 && diastolic < 80)  return "Elevated";
        if (systolic < 140 || diastolic < 90)  return "Stage 1";
        return "Stage 2";
    }
}
```

- [ ] **Step 2: Verify the project builds**

```bash
cd WeightLossTracker && dotnet build
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add WeightLossTracker/Services/BpCategoryService.cs
git commit -m "feat(bp): add BpCategoryService with AHA 2017 classification"
```

---

### Task 2: Add Category property to BloodPressureEntry

**Files:**
- Modify: `WeightLossTracker/Models/Models.cs:76-86`

- [ ] **Step 1: Add the property**

Open `WeightLossTracker/Models/Models.cs`. The `BloodPressureEntry` class currently ends at line 86:

```csharp
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
}
```

Add `Category` as the last property (add `using System.ComponentModel.DataAnnotations;` at the top of the file if not already present):

```csharp
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
```

- [ ] **Step 2: Verify the project builds**

```bash
cd WeightLossTracker && dotnet build
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add WeightLossTracker/Models/Models.cs
git commit -m "feat(bp): add Category property to BloodPressureEntry"
```

---

### Task 3: Generate EF Core migration

**Files:**
- Migration auto-generated under `WeightLossTracker/Migrations/`

- [ ] **Step 1: Add the migration**

```bash
cd WeightLossTracker && dotnet ef migrations add AddBpCategory
```

Expected: output ending with `Done.` and two new files created under `Migrations/`.

- [ ] **Step 2: Verify the generated migration**

Open the generated `Migrations/<timestamp>_AddBpCategory.cs`. The `Up` method should contain exactly one `AddColumn` call:

```csharp
migrationBuilder.AddColumn<string>(
    name: "Category",
    table: "BloodPressureEntries",
    type: "TEXT",
    maxLength: 20,
    nullable: true);
```

> Note: EF Core SQLite generates `type: "TEXT"` — that is correct. The column is nullable so existing rows get `NULL` automatically.

- [ ] **Step 3: Apply the migration**

```bash
dotnet ef database update
```

Expected: `Done.`

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/Migrations/
git commit -m "feat(bp): add EF migration for Category column on BloodPressureEntries"
```

---

### Task 4: Wire Classify into POST and update GET projection

**Files:**
- Modify: `WeightLossTracker/Program.cs:475-516`

- [ ] **Step 1: Update the POST handler**

Find the POST `/api/bp` handler (around line 492). The `entry` initializer currently does not set `Category`. Add it:

```csharp
var entry = new BloodPressureEntry
{
    UserProfileId = profileId,
    Systolic      = req.Systolic,
    Diastolic     = req.Diastolic,
    Pulse         = req.Pulse,
    Notes         = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
    RecordedAt    = req.RecordedAt ?? DateTime.UtcNow,
    Category      = BpCategoryService.Classify(req.Systolic, req.Diastolic)
};
```

Also update the `Results.Created` projection at the bottom of the handler to include `entry.Category`:

```csharp
return Results.Created($"/api/bp/{entry.Id}", new {
    entry.Id, entry.Systolic, entry.Diastolic, entry.Pulse,
    entry.Notes, entry.RecordedAt, entry.Category
});
```

- [ ] **Step 2: Update the GET projection**

Find the GET `/api/bp` handler (around line 475). The `.Select` projection currently omits `Category`. Add it:

```csharp
.Select(b => new {
    b.Id,
    b.Systolic,
    b.Diastolic,
    b.Pulse,
    b.Notes,
    b.RecordedAt,
    b.Category
})
```

- [ ] **Step 3: Verify the project builds**

```bash
cd WeightLossTracker && dotnet build
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 4: Smoke-test with the running app**

Start the app (`dotnet run` from `WeightLossTracker/`) and log a new BP reading. Open the browser dev tools Network tab, submit the form, and confirm the POST response JSON includes a `"category"` field with a non-null value (e.g. `"Normal"`). Also confirm GET `/api/bp` returns `"category"` on each entry.

- [ ] **Step 5: Commit**

```bash
git add WeightLossTracker/Program.cs
git commit -m "feat(bp): store and return category from POST/GET /api/bp"
```

---

### Task 5: Add bpBadge helper and update bpRow in app.js

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js:711-727`

- [ ] **Step 1: Add the bpBadge helper function**

Insert the following function immediately before `function bpRow(e)` (currently at line 711):

```js
function bpBadge(category) {
  const map = {
    'Hypotension': 'background:#93c5fd;color:#1e3a5f',
    'Normal':      'background:#86efac;color:#14532d',
    'Elevated':    'background:#fde68a;color:#78350f',
    'Stage 1':     'background:#fdba74;color:#7c2d12',
    'Stage 2':     'background:#f87171;color:#7f1d1d',
    'Crisis':      'background:#dc2626;color:#fff',
  };
  if (!category || !map[category]) return '';
  return `<span style="${map[category]};display:inline-block;padding:1px 7px;border-radius:999px;font-size:0.68rem;font-weight:600;vertical-align:middle;margin-left:4px">${escHtml(category)}</span>`;
}
```

- [ ] **Step 2: Update bpRow to use the badge**

Find `function bpRow(e)` (currently at line 711). The Sys/Dia cell currently reads:

```js
<td class="py-2 pr-4 font-semibold text-[var(--color-text-primary)]">${e.systolic}/${e.diastolic} <span class="${C.tinyText}">mmHg</span></td>
```

Update it to append the badge:

```js
<td class="py-2 pr-4 font-semibold text-[var(--color-text-primary)]">${e.systolic}/${e.diastolic} <span class="${C.tinyText}">mmHg</span>${bpBadge(e.category)}</td>
```

- [ ] **Step 3: Verify in the browser**

Start the app and navigate to Vitals → Blood Pressure. Confirm:
1. Newly saved readings show a colored badge inline next to the reading (e.g. `118/76 mmHg` + green `Normal` pill).
2. Any existing rows with null category show no badge and no error.
3. The table layout is not broken on narrow viewports (the badge wraps gracefully).

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat(bp): add bpBadge helper and category badge to history table"
```
