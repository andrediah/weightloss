# UX Redesign — Visual Language v2

**Date:** 2026-04-18
**Branch:** feature/visualv2
**Scope:** Full redesign of all four views to establish a new visual language

---

## 1. Design Direction

**Warm & Motivational.** The visual language is built around encouragement, warmth, and progress celebration. Every screen communicates forward momentum. The tone is that of a supportive coach — positive, human, and personal.

---

## 2. Colour System

### Base palette

| Token | Light value | Dark value |
|---|---|---|
| `--color-surface-primary` | `#fffbf0` (warm off-white) | `#1c1008` (warm near-black) |
| `--color-surface-secondary` | `#fff8ed` | `#2a1a0a` |
| `--color-text-primary` | `#1c0800` | `#fef3c7` |
| `--color-text-secondary` | `#92400e` | `#fde68a` |
| `--color-border-default` | `#fde68a` | `#3d2008` |

### Accent presets (user-selectable, 5 options)

All presets are warm or warm-adjacent so they harmonise with the fixed warm base at all times. The warm base (`--color-surface-primary`) never changes regardless of accent choice.

| Name | Primary | Dark variant |
|---|---|---|
| **Amber** (default) | `#f59e0b` / `#d97706` | same — amber reads well on dark |
| **Rose** | `#f472b6` / `#db2777` | `#f9a8d4` / `#ec4899` |
| **Coral** | `#fb923c` / `#ea580c` | `#fdba74` / `#f97316` |
| **Gold** | `#eab308` / `#ca8a04` | `#fde047` / `#facc15` |
| **Violet** | `#a78bfa` / `#7c3aed` | `#c4b5fd` / `#8b5cf6` |

Cool accents (Teal, Blue) are excluded — they create temperature contrast against the warm base surfaces that cannot be resolved without changing the base, which is out of scope.

Secondary accent (violet) is always present alongside the user's chosen primary accent, serving as a counter-colour on data callouts and the log entry header.

### Dark mode

Dark mode uses a **warm brown-black base** (`#1c1008`), not a neutral or cool dark. Amber is preserved at full warmth. This is a deliberate palette — not an inversion of light mode.

---

## 3. Typography

- **Font family:** System UI stack (`system-ui, -apple-system, sans-serif`) — no custom font loading
- **Hero numbers:** `font-weight: 900`, rendered at 32–48px via `clamp()`
- **Section labels:** 10–11px, `font-weight: 700`, `text-transform: uppercase`, `letter-spacing: 0.08em`
- **Body / list items:** 12–13px, `font-weight: 400–600`
- **Button labels:** Sentence case, 14–15px, `font-weight: 800`
- All sizes use `rem` / `clamp()` — no hardcoded `px` for font sizes

---

## 4. Component Tokens

| Property | Value |
|---|---|
| Card border-radius | `16px` |
| Button border-radius | `14px` (full-width CTA), `99px` (pill / segmented) |
| Card shadow | `0 2px 8px rgba(0,0,0,0.06)` |
| Header gradient | `linear-gradient(160deg, var(--color-accent), var(--color-accent-dark))` |
| Spacing grid | 4px base — all gaps/padding are multiples of 4 |

---

## 5. Navigation

Fixed bottom tab bar, 4 items, present on all screens.

| Tab | Icon | Label |
|---|---|---|
| Home | 🏠 | Home |
| Log | ➕ | Log |
| Trends | 📈 | Trends |
| Settings | ⚙️ | Settings |

- Active tab: `border-top: 2px solid var(--color-accent)` + accent-coloured label
- Inactive tabs: muted warm tone
- iOS safe area: `padding-bottom: env(safe-area-inset-bottom)`
- Desktop: sidebar nav replaces bottom bar at `md` breakpoint (768px+)

---

## 6. Screens

### 6.1 Dashboard (Home)

**Hero:** SVG progress ring showing % to goal — centred inside the amber gradient header.

**Layout (top → bottom):**
1. Gradient header — greeting text, ring, ring label (e.g. "92% to goal 🎯")
2. Stat card row — Current weight · Lost lbs · Goal weight (3 equal columns)
3. Recent entries card — last 3 log entries, date + weight, no pagination needed

**Greeting copy:** Time-aware ("Good morning 🌅", "Good evening 🌙"). Rotates through motivational sub-lines.

### 6.2 Log Entry

**Trigger:** Tapping the ➕ tab navigates to a dedicated full screen.

**Layout:**
1. Violet gradient header — "Log your weight" + motivational sub-line
2. Date strip — horizontal scroll of the last 7 days; selected day highlighted in violet
3. Weight input card — large `font-weight: 900` number display, − and + buttons (44×44px), manual text input also supported
4. Notes card — optional free-text field, placeholder hint only (not a label substitute)
5. Full-width CTA — "Save Entry ✓" amber gradient button

**Validation:** Weight must be a positive number between 50–999 lbs (or 23–453 kg). Inline error displayed below input on blur.

### 6.3 Trends

**Layout:**
1. Amber gradient header — "Your trends 📈" + period tabs (1W · 1M · 3M · All)
2. Chart card — Chart.js line chart with amber fill gradient below the line
3. Summary stat row — "Avg this period" and "Best week loss" (2 columns)

**Chart style:** Amber line, amber-to-transparent area fill, no gridlines (minimal), x-axis labels only (day/date abbreviations).

### 6.4 Settings

**Layout — grouped cards:**
1. 🎨 Accent colour — 5 circular swatches; active swatch has a ring border
2. 🌙 Dark mode — toggle switch (pill style); default follows OS `prefers-color-scheme`; persisted to `localStorage`
3. 🏁 Goal weight — displays current goal with an "Edit" pill button; tapping "Edit" transforms the display into an inline number input with "Save" / "Cancel" pill buttons — no modal or navigation
4. ⚖️ Units — segmented control: lbs / kg

**Accent colour change** applies instantly via a CSS custom property swap on `<html>`. No page reload.

---

## 7. Theming Architecture

Accent colours are stored as two CSS custom properties on `<html>`:

```css
html {
  --color-accent: #f59e0b;       /* primary accent */
  --color-accent-dark: #d97706;  /* darker variant for gradients */
}
```

JavaScript writes these on page load (from `localStorage`) and on user selection. All gradient headers, active nav indicators, buttons, and ring strokes reference these two variables — swapping them changes the whole UI instantly.

User's chosen accent is persisted as `wlt-accent` in `localStorage`. Dark/light mode preference is persisted as `wlt-theme`.

---

## 8. Responsive Behaviour

| Breakpoint | Navigation | Layout |
|---|---|---|
| Default (< 768px) | Fixed bottom tab bar | Single column, full-width cards |
| md (768px+) | Left sidebar (collapsible) | Two-column dashboard: ring + stats left, chart right |
| lg (1024px+) | Left sidebar expanded | Three-column trends grid |

Mobile is the primary design target. Desktop is an enhancement.

---

## 9. Accessibility

All requirements from `docs/ux-standards.md` apply. Specific commitments for this redesign:

- Progress ring: `role="progressbar"` with `aria-valuenow`, `aria-valuemin`, `aria-valuemax`
- Emoji icons in nav: wrapped in `<span aria-hidden="true">`, text label provides accessible name
- Accent colour swatches: `aria-label="Amber accent"` etc., `aria-pressed` for selected state
- Dark mode toggle: `role="switch"` with `aria-checked`
- All interactive elements meet 44×44px touch target minimum
- `prefers-reduced-motion` disables all transitions and animations

---

## 10. Out of Scope

- Push notifications / reminders
- Social / sharing features
- Body measurements beyond weight
- Calorie / nutrition tracking
- Export / data backup UI

These are explicitly excluded from this redesign cycle.
