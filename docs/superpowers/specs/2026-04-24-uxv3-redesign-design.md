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
--color-border-strong:     #86EFAC;   /* green-300 — visible edges on secondary surfaces */
--color-border-subtle:     #E5E7EB;
--color-row-alt:           #F9FAFB;   /* alternating table rows */
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
--color-border-default:    #334155;   /* slate-700 — visible against card surfaces */
--color-border-strong:     #475569;   /* slate-600 — use for hard edges (sidebar, tab bar) */
--color-border-subtle:     #334155;
--color-row-alt:           #172033;   /* alternating table rows (between 950 and 800) */
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
- **Right border:** `1px solid --color-border-strong`
- **App logo/icon:** Replace the current `+` circle SVG with a scale/weight SVG icon. Color: `--color-accent`
- **Brand text:** Primary text color; accent color for "Tracker" sub-label
- **Nav items (inactive):** `--color-text-secondary`, no background
- **Nav items (active):** Background `--color-nav-active-bg`, text `--color-nav-active-text`, `border-left: 2px solid --color-nav-active-border`, left padding reduced by 2px to compensate
- **Nav item hover:** `--color-accent-subtle` background
- **Goal progress strip** (new, bottom of sidebar above logout + toggle):
  - Label: "X% to goal" in `--color-text-disabled`
  - Track: `--color-border-subtle`, height 3px, border-radius 2px
  - Fill: `--color-accent`, width computed as
    `clamp(0%, (startWeight - currentWeight) / (startWeight - goalWeight) * 100%, 100%)`

### Keep (behavior preserved)
- Dark mode toggle stays in the sidebar bottom on desktop AND in the mobile header. Restyle it to the new palette (see §10); do not remove it — desktop users have no other way to switch themes.
- The goal progress strip is inserted **above** the existing logout + toggle cluster, not as a replacement.

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
- Bar color: `--color-accent` with per-bar opacity gradient (oldest = 30%, newest = 100%). Pass `backgroundColor` as an array of rgba strings, one per bar — Chart.js applies a single value uniformly otherwise. Read the accent token from `getComputedStyle(document.documentElement).getPropertyValue('--color-accent')` at chart build time so the color tracks light/dark mode.
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
| Warning | `color-mix(in srgb, var(--color-feedback-warning) 15%, transparent)` | `--color-feedback-warning` |
| Error/missed | `color-mix(in srgb, var(--color-feedback-error) 12%, transparent)` | `--color-feedback-error` |

> Note: `rgba(var(--token), α)` does **not** work when the token is a hex literal. Use `color-mix()` as shown, or define `-rgb` channel tokens (e.g. `--color-feedback-warning-rgb: 138 109 0;` + `rgba(var(--color-feedback-warning-rgb) / 0.15)`). The `color-mix` form is simpler and covered by all current evergreen browsers.

### Log table rows
- Alternating row background: transparent / `--color-row-alt`
- Row border-bottom: `1px solid --color-border-subtle`
- Date: `--color-text-secondary`
- Value: `--color-text-primary`, weight 600
- Delta (positive = loss): `--color-accent`

---

## 7. Mobile Bottom Tab Bar

- Background: `--color-surface-sidebar`
- Border-top: `1px solid --color-border-strong`
- Active tab: top border `2px solid --color-accent`, icon + label `--color-accent`
- Inactive: `--color-text-secondary`

Remove the `bg-indigo-900` / `dark:bg-gray-950` hardcoded values and replace with token-based classes or inline CSS var references. The existing `.mobile-tab-active { border-top-color: #a5b4fc }` rule in `index.html` must change to `var(--color-accent)` — see §11.

---

## 8. Typography & `C` constants in `app.js`

No font family change — keep the system font stack (`-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif`).

**Tailwind version note:** `index.html` loads the Tailwind **v3** CDN (`tailwind.config = { darkMode: 'class' }`). The `<style type="text/tailwindcss">` v4 block is tolerated but not canonical. v3 arbitrary-value syntax requires `bg-[var(--color-accent)]`, **not** `bg-[--color-accent]` (that's v4-only). Use the `var()` form below throughout.

Update the `C` constants object in `app.js`:

| Key | Current | Updated |
|---|---|---|
| `h1` | `text-2xl font-bold text-gray-800 dark:text-gray-100` | `text-2xl font-bold text-[var(--color-text-primary)]` |
| `h2` | `text-lg font-semibold text-gray-700 dark:text-gray-200 mb-4` | `text-lg font-semibold text-[var(--color-text-secondary)] mb-4` |
| `h3` | `font-semibold text-gray-800 dark:text-gray-100` | `font-semibold text-[var(--color-text-primary)]` |
| `label` | `block text-sm text-gray-600 dark:text-gray-300 mb-1` | `block text-sm text-[var(--color-text-secondary)] mb-1` |
| `bodyText` | `text-sm text-gray-700 dark:text-gray-200` | `text-sm text-[var(--color-text-primary)]` |
| `mutedText` | `text-sm text-gray-500 dark:text-gray-400` | `text-sm text-[var(--color-text-secondary)]` |
| `tinyText` | `text-xs text-gray-400 dark:text-gray-500` | `text-xs text-[var(--color-text-disabled)]` |
| `card` | `bg-white dark:bg-gray-800 rounded-xl shadow p-5` | `bg-[var(--color-surface-card)] border border-[var(--color-border-subtle)] rounded-xl shadow-sm p-5` |
| `input` | `border dark:border-gray-600 rounded-lg px-3 py-2 w-full bg-white dark:bg-gray-700 ...` | `border border-[var(--color-border-subtle)] rounded-lg px-3 py-2 w-full bg-[var(--color-surface-card)] text-[var(--color-text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]` |
| `inputSm` | same family as `input`, smaller | same substitutions, smaller padding |
| `select` | same as `input` | same substitutions as `input` |
| `btnPrimary` | `bg-indigo-600 hover:bg-indigo-700 text-white ...` | `bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] px-5 py-2 rounded-lg font-medium transition-colors min-h-[44px]` |
| `btnSuccess` | `bg-green-600 hover:bg-green-700 ...` | same as `btnPrimary` (accent is already green/teal — collapse the variant) |
| `btnSecondary` | `bg-gray-100 ... dark:bg-gray-700 ...` | `bg-[var(--color-accent-subtle)] text-[var(--color-accent-text)] border border-[var(--color-accent)] hover:bg-[var(--color-accent)] hover:text-[var(--color-text-inverted)] px-4 py-2 rounded-lg text-sm font-medium transition-colors` |
| `btnSmPrimary` | `text-xs bg-indigo-600 hover:bg-indigo-700 ...` | `text-xs bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] rounded px-2 py-1 transition-colors` |
| `trow` | `border-b dark:border-gray-700 ... hover:bg-gray-50 dark:hover:bg-gray-700/50` | `border-b border-[var(--color-border-subtle)] last:border-0 hover:bg-[var(--color-accent-subtle)]` |
| `divider` | `border-b dark:border-gray-700` | `border-b border-[var(--color-border-subtle)]` |
| `badge` | `text-xs font-semibold px-2 py-0.5 rounded-full` | unchanged (color applied per-variant via §6 Badges table) |

---

## 9. Login View (`index.html` lines ~394–432)

The login view is rendered directly in `index.html`, not through the `C` constants. Apply these swaps:

| Element | Current | New |
|---|---|---|
| Outer backdrop | `bg-gray-100 dark:bg-gray-900` | `bg-[var(--color-surface-primary)]` |
| Card | `bg-white dark:bg-gray-800` | `bg-[var(--color-surface-card)] border border-[var(--color-border-subtle)]` |
| Logo icon `stroke` | `text-indigo-500` | `text-[var(--color-accent)]` |
| Heading | `text-gray-800 dark:text-gray-100` | `text-[var(--color-text-primary)]` |
| Label | `text-gray-600 dark:text-gray-300` | `text-[var(--color-text-secondary)]` |
| Input | `border dark:border-gray-600 ... bg-white dark:bg-gray-700 ... focus:ring-indigo-400` | match §6 Inputs (token border, token bg, `focus:ring-[var(--color-accent)]`) |
| Submit button | `bg-indigo-600 hover:bg-indigo-700 text-white` | match §6 Primary button |

Replace the logo SVG with the scale/weight SVG used in the sidebar (per §4) so login and app share brand.

---

## 10. Stray `<style>` rules in `index.html` to update

These rules live in the inline `<style>` block (lines ~20–121) and hardcode colors outside the token system. They must be updated:

| Rule (approx. line) | Current | New |
|---|---|---|
| `.mobile-tab-active` (line 90) | `border-top-color: #a5b4fc; color: #ffffff` | `border-top-color: var(--color-accent); color: var(--color-accent)` |
| `.prose h2` (line 116) | `color: #4f46e5` | `color: var(--color-accent)` |
| `.dark .prose h2` (line 120) | `color: #818cf8` | remove the rule — `.prose h2` already picks up the dark-mode accent via the token |
| Scrollbar track (108, 111) | `#f1f5f9` / `#1e293b` | `var(--color-surface-secondary)` for both (drop the `.dark` override) |
| Scrollbar thumb (110, 112) | `#cbd5e1` / `#475569` | `var(--color-border-subtle)` for both (drop the `.dark` override) |
| Skip-nav link (`focus:bg-indigo-600`, line 127) | indigo | `focus:bg-[var(--color-accent)]` |

Also audit the sidebar `<button>` markup (lines ~190–261) — every `text-indigo-100`, `hover:bg-indigo-700`, `dark:hover:bg-indigo-800`, `focus:ring-indigo-400`, and `focus:ring-offset-indigo-900` must be replaced with token-based equivalents. Same for the mobile header (lines ~132–164) and mobile tab bar (lines ~316–392).

---

## 11. What Is NOT Changing

- HTML structure, routing logic, view render functions — no JS refactoring
- Backend API contracts
- Chart.js version or configuration beyond colors
- Mobile layout pattern (top header + bottom tabs)
- Accessibility attributes (aria-labels, roles, skip nav link)
- Dark mode toggle mechanism (localStorage + `.dark` class on `<html>`) — behavior unchanged; styling re-tokenized per §4 and §10

---

## 12. Files Affected

| File | Change |
|---|---|
| `wwwroot/index.html` | CSS token values, inline `<style>` rules (§10), sidebar HTML, mobile header, mobile tab bar, login view (§9) |
| `wwwroot/js/app.js` | `C` constants object (§8), dashboard view renderer (§5), Chart.js color config (§5) |

No new files. No backend changes.
