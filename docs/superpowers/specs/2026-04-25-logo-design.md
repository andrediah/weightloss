# Logo Design Spec — Weight Loss Tracker

**Date:** 2026-04-25
**Status:** Approved

## Overview

An icon-only logo for the Weight Loss Tracker web app. The logo replaces the existing balance-scale SVG used in the sidebar, mobile header, and login screen. It is implemented as an inline SVG — no external image files.

## Concept

**Active Figure on Trend Path.** A running person leaning forward, with a downward-trending arrow path beneath them as their ground. The figure conveys energy and aspiration; the trend path conveys measurable progress downward over time. Together they communicate: *an active person on a successful weight-loss journey.*

## Visual Specification

### Shape & Structure

- **Head:** filled circle, centered-right in the viewBox
- **Torso:** single bold line, leaning forward ~30° from vertical
- **Arms:** two lines from mid-torso — back arm raised, front arm lowered
- **Legs:** two lines from base of torso — back leg and front leg in mid-stride
- **Trend path:** polyline from bottom-left to mid-right, descending in two steps, terminated with a filled arrowhead triangle
- **ViewBox:** `0 0 48 48`

### Two-Tone Color

The figure uses two greens to give depth and separate the limbs visually:

| Element | Light mode | Dark mode |
|---|---|---|
| Head, torso, back arm, back leg, trend path, arrowhead | `#16A34A` | `#5EEAD4` |
| Front arm, front leg | `#86EFAC` | `#0D9488` |

These map directly to the existing CSS design tokens (`--color-accent` and `--color-accent-subtle`/`--color-border-strong`).

### Stroke Weights (at viewBox 48×48)

| Element | `stroke-width` |
|---|---|
| Torso | `4` |
| Arms | `3` |
| Legs | `3.5` |
| Trend path | `2.8` |

All strokes use `stroke-linecap="round"` and `stroke-linejoin="round"`.

### Reference SVG (48×48, light mode)

```svg
<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48">
  <circle cx="32" cy="5" r="4.5" fill="#16A34A"/>
  <line x1="32" y1="9.5" x2="26" y2="21" stroke="#16A34A" stroke-width="4" stroke-linecap="round"/>
  <line x1="28" y1="14" x2="20" y2="11" stroke="#16A34A" stroke-width="3" stroke-linecap="round"/>
  <line x1="28" y1="14" x2="35" y2="17" stroke="#86EFAC" stroke-width="3" stroke-linecap="round"/>
  <line x1="26" y1="21" x2="19" y2="32" stroke="#16A34A" stroke-width="3.5" stroke-linecap="round"/>
  <line x1="26" y1="21" x2="33" y2="30" stroke="#86EFAC" stroke-width="3.5" stroke-linecap="round"/>
  <polyline points="4 44 13 36 22 40 36 27" stroke="#16A34A" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
  <polygon points="30,25 38,25 38,33" fill="#16A34A"/>
</svg>
```

For dark mode, swap `#16A34A` → `#5EEAD4` and `#86EFAC` → `#0D9488`.

## Usage Locations

The logo replaces the existing balance-scale SVG in three places in `index.html`:

1. **Desktop sidebar** — `width="24" height="24"` alongside "Weight Loss Tracker" app name
2. **Mobile top header** — `width="22" height="22"` alongside app name
3. **Login screen** — `width="28" height="28"` above the login form

The SVG markup is identical at all three locations; only the `width`/`height` attributes differ. The `viewBox` stays `0 0 48 48` so scaling is handled by the browser.

## Dark Mode Handling

The existing app uses a `[data-theme="dark"]` attribute toggled by `toggleTheme()` and a `prefers-color-scheme: dark` media query for the initial state. Because the logo uses two distinct fill/stroke colors (not a single `currentColor`), the implementation uses two sibling SVG elements at each placement site:

- One with class `logo-light` (hardcoded light-mode colors, hidden in dark mode)
- One with class `logo-dark` (hardcoded dark-mode colors, hidden in light mode)

CSS rules in the `<style>` block handle visibility:

```css
[data-theme="dark"] .logo-light,
@media (prefers-color-scheme: dark) { .logo-light } { display: none; }
[data-theme="dark"] .logo-dark,
@media (prefers-color-scheme: dark) { .logo-dark } { display: inline; }
.logo-dark { display: none; }
```

Both SVGs carry `aria-hidden="true"` since the surrounding text already labels the icon.

## PNG Representations

256×256 PNG exports are stored alongside this spec:

| File | Description |
|---|---|
| [`logo-light-256.png`](logo-light-256.png) | Light mode — transparent background, green figure |
| [`logo-dark-256.png`](logo-dark-256.png) | Dark mode — `#0F172A` background, teal figure |

These are for reference and documentation only. The production logo is always rendered as inline SVG.

## Out of Scope

- Animated version (loading spinner, intro splash)
- Wordmark / text lockup
- External image files (.png, .svg file)
- Favicon (separate task if needed)
