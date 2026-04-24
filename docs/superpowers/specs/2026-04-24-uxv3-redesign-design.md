# UX v3 Redesign — Design Spec

| Field | Value |
|---|---|
| **Date** | 2026-04-24 |
| **Branch** | feature/uxv3 |
| **Status** | Approved |

---

## 1. Overview

A full visual redesign of the Weight Loss Tracker frontend (`wwwroot/index.html`, `wwwroot/js/app.js`). No backend changes. The goal is to replace the current indigo-heavy palette with a Dual-Mode Hybrid design system: a clean green/white health palette in light mode and a slate/teal premium dark palette in dark mode. Layout structure (sidebar + main content) is preserved.

---

## 2. Design Direction

**Dual-Mode Hybrid** — the two themes share the same layout, typography, and component shapes. Only color tokens change between modes.

- **Light mode:** Health & Wellness — white surfaces, green accent, card borders, airy feel
- **Dark mode:** Dark Premium — slate-950 base, teal-green accent, subtle depth through layered surfaces
- **Accent color (unified):** teal-green — `#16a34a` light / `#0d9488` dark

This replaces the current indigo-900 sidebar + indigo-600 accent throughout the app.

---

## 3. Color Tokens

Replace all existing CSS custom property values in `index.html` with the following:

### Light mode (`:root` / `[data-theme="light"]`)

```css
--color-surface-primary:   #FFFFFF;
--color-surface-secondary: #F0FDF4;   /* green-50 */
--color-surface-card:      #FFFFFF;
--color-surface-sidebar:   #F0FDF4;
--color-border-default:    #BBF7D0;   /* green-200 */
--color-border-subtle:     #E5E7EB;
--color-text-primary:      #111827;
--color-text-secondary:    #6B7280;
--color-text-disabled:     #9CA3AF;
--color-text-inverted:     #FFFFFF;
--color-accent:            #16A34A;   /* green-600 */
--color-accent-hover:      #15803D;   /* green-700 */
--color-accent-subtle:     #DCFCE7;   /* green-100 */
--color-accent-text:       #166534;   /* green-800 */
--color-nav-active-bg:     #DCFCE7;
--color-nav-active-border: #16A34A;
--color-nav-active-text:   #166534;
--color-feedback-error:    #B71C1C;
--color-feedback-success:  #166534;
--color-feedback-warning:  #8A6D00;
```

### Dark mode (`[data-theme="dark"]` / `@media (prefers-color-scheme: dark)`)

```css
--color-surface-primary:   #0F172A;   /* slate-950 */
--color-surface-secondary: #1E293B;   /* slate-800 */
--color-surface-card:      #1E293B;
--color-surface-sidebar:   #0F172A;
--color-border-default:    #1E293B;
--color-border-subtle:     #334155;   /* slate-700 */
--color-text-primary:      #F1F5F9;
--color-text-secondary:    #94A3B8;
--color-text-disabled:     #475569;
--color-text-inverted:     #0F172A;
--color-accent:            #0D9488;   /* teal-600 */
--color-accent-hover:      #0F766E;   /* teal-700 */
--color-accent-subtle:     #134E4A;   /* teal-950 */
--color-accent-text:       #5EEAD4;   /* teal-300 */
--color-nav-active-bg:     #134E4A;
--color-nav-active-border: #0D9488;
--color-nav-active-text:   #5EEAD4;
--color-feedback-error:    #FCA5A5;
--color-feedback-success:  #6EE7B7;
--color-feedback-warning:  #FDE68A;
```

---

## 4. Sidebar Redesign

### Structure (unchanged)
- Desktop: fixed left sidebar, `w-56`, full height
- Mobile: hidden; replaced by bottom tab bar

### Visual changes
- **Background:** `--color-surface-sidebar` (green-50 light / slate-950 dark)
- **Right border:** `1px solid --color-border-default`
- **App logo/icon:** Replace the current `+` circle SVG with a scale/weight SVG icon. Color: `--color-accent`
- **Brand text:** Primary text color; accent color for "Tracker" sub-label
- **Nav items (inactive):** `--color-text-secondary`, no background
- **Nav items (active):** Background `--color-nav-active-bg`, text `--color-nav-active-text`, `border-left: 2px solid --color-nav-active-border`, left padding reduced by 2px to compensate
- **Nav item hover:** `--color-accent-subtle` background
- **Goal progress strip** (new, bottom of sidebar above logout):
  - Label: "X% to goal" in `--color-text-disabled`
  - Track: `--color-border-subtle`, height 3px, border-radius 2px
  - Fill: `--color-accent`, width = `(startWeight - currentWeight) / (startWeight - goalWeight) * 100%`

### Remove
- Dark mode toggle from sidebar bottom (keep only in mobile header)
- Replace with goal progress strip

---

## 5. Dashboard View

### Stats banner
Three equal-width cards in a row:

| Card | Value | Label | Color |
|---|---|---|---|
| Current weight | e.g. `205.4` | `CURRENT` | `--color-text-primary` |
| Lost | e.g. `−9.6` | `LOST` | `--color-accent` |
| To go | e.g. `15.4` | `TO GO` | `--color-text-primary` |

- Card background: `--color-surface-card`
- Card border: `1px solid --color-border-default` (light) / none (dark)
- Value font: 1.5rem, weight 800
- Label font: 0.6rem, uppercase, letter-spacing 0.8px, `--color-text-disabled`
- Unit ("lbs") shown as small superscript in `--color-text-secondary`

### Progress bar
Below the stats banner, full width:
- Left label: start weight (`215 lbs`), right label: goal weight (`190 lbs`), center: percentage
- Track height: 6px, background `--color-accent-subtle`, border-radius 3px
- Fill: `linear-gradient(90deg, var(--color-accent), #22c55e)` light / `linear-gradient(90deg, #0d9488, #22c55e)` dark, width = goal %

### Weight trend chart (Chart.js)
- Bar chart, last 7 entries
- Bar color: `--color-accent` with opacity gradient (oldest = 30%, newest = 100%)
- Grid lines: `--color-border-subtle`, thin (0.5px)
- Axis labels: `--color-text-secondary`, 11px
- No chart border/box — transparent background

### Quick-log panel
Next to the chart:
- Primary button "＋ Weight" — `--color-accent` background, white text
- Secondary button "＋ Meal" — `--color-accent-subtle` background, `--color-accent-text` text, `--color-accent` border

---

## 6. Component Updates

### Buttons
| Variant | Background | Text | Border |
|---|---|---|---|
| Primary | `--color-accent` | `--color-text-inverted` | none |
| Secondary | `--color-accent-subtle` | `--color-accent-text` | `1px solid --color-accent` |
| Danger | transparent | `--color-feedback-error` | `1px solid` error color at 40% opacity |
| Disabled | `--color-surface-secondary` | `--color-text-disabled` | none |

All buttons: border-radius 8px, min-height 44px (touch target), font-weight 600.

### Inputs & selects
- Background: `--color-surface-card`
- Border: `1px solid --color-border-subtle`
- Focus ring: `2px solid --color-accent`, border transparent
- Text: `--color-text-primary`
- Placeholder: `--color-text-disabled`
- Border-radius: 8px

### Cards
- Background: `--color-surface-card`
- Border: `1px solid --color-border-subtle` (light mode only; dark mode uses layered bg)
- Border-radius: 12px
- Shadow: `0 1px 3px rgba(0,0,0,0.06)` light / none dark

### AI insight card (special variant)
- Background: `--color-accent-subtle`
- Border: `1px solid --color-accent` at 50% opacity
- Label "AI Insight": `--color-accent`, 0.6rem, uppercase, weight 700
- Body text: `--color-accent-text`

### Badges
| Type | Background | Text |
|---|---|---|
| Success/on-track | `--color-accent-subtle` | `--color-accent-text` |
| Neutral | `--color-surface-secondary` | `--color-text-secondary` |
| Warning | `rgba(--color-feedback-warning, 0.15)` / `#FEF9C3` light | `--color-feedback-warning` |
| Error/missed | `rgba(--color-feedback-error, 0.12)` / `#FEE2E2` light | `--color-feedback-error` |

### Log table rows
- Alternating: transparent / `--color-surface-secondary` (5% opacity)
- Row border-bottom: `1px solid --color-border-subtle`
- Date: `--color-text-secondary`
- Value: `--color-text-primary`, weight 600
- Delta (positive = loss): `--color-accent`

---

## 7. Mobile Bottom Tab Bar

- Background: `--color-surface-sidebar`
- Border-top: `1px solid --color-border-default`
- Active tab: top border `2px solid --color-accent`, icon + label `--color-accent`
- Inactive: `--color-text-secondary`

Remove the `bg-indigo-900` / `dark:bg-gray-950` hardcoded values and replace with token-based classes or inline CSS var references.

---

## 8. Typography

No font family change — keep the system font stack (`-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif`). Update the `C` constants object in `app.js`:

| Key | Current | Updated |
|---|---|---|
| `h1` | `text-2xl font-bold text-gray-800 dark:text-gray-100` | `text-2xl font-bold` + `color: var(--color-text-primary)` |
| `h2` | `text-lg font-semibold text-gray-700 dark:text-gray-200 mb-4` | same size + `color: var(--color-text-secondary)` |
| `card` | `bg-white dark:bg-gray-800 rounded-xl shadow p-5` | token bg, updated radius/shadow |
| `btnPrimary` | `bg-indigo-600 hover:bg-indigo-700 ...` | `bg-[--color-accent] hover:bg-[--color-accent-hover] ...` |
| `input` | hardcoded gray classes | token-based |

---

## 9. What Is NOT Changing

- HTML structure, routing logic, view render functions — no JS refactoring
- Backend API contracts
- Chart.js version or configuration beyond colors
- Mobile layout pattern (top header + bottom tabs)
- Accessibility attributes (aria-labels, roles, skip nav link)
- Dark mode toggle mechanism (localStorage + `.dark` class on `<html>`)

---

## 10. Files Affected

| File | Change |
|---|---|
| `wwwroot/index.html` | CSS token values, sidebar HTML, mobile nav colors, login view |
| `wwwroot/js/app.js` | `C` constants object, dashboard view renderer, chart color config |

No new files. No backend changes.
