# Weight Tab Trend Chart & UX Parity — Design Spec

**Date:** 2026-04-26
**Status:** Approved

## Problem

The Weight tab and Blood Pressure tab both live under Vitals but look and behave differently:

- The Weight tab has no trend chart; the BP tab does.
- The Weight history table uses hardcoded Tailwind `text-gray-*` classes that ignore the design token system and break in dark mode.
- The BP tab uses the `C` object and CSS variable tokens throughout.

## Goal

Make the Weight tab structurally and visually identical to the BP tab: form → trend chart → history table, all using CSS variable tokens.

## Solution

Three targeted changes to `WeightLossTracker/wwwroot/js/app.js`. No backend changes, no HTML changes, no new API endpoints.

## Architecture

### 1. `renderWeightContent` — restructure to 3-card layout

Current structure: form card + history card.

New structure (mirrors BP tab exactly):
1. Form card — log today's weight (unchanged)
2. Chart card — heading `Trend`, contains `<div id="weight-chart-wrap-tab">` → `<canvas id="weight-chart-tab">`
3. History card — heading `History`, contains `<div id="weight-table-wrap">`

### 2. `renderWeightChart(entries)` — new function

Mirrors `renderBpChart` exactly:

- If no entries: render muted empty-state text inside `#weight-chart-wrap-tab`
- Slice last 14 entries, reverse to chronological order
- X-axis labels: `fmtDate(e.date)`
- Single dataset: weight in lbs, using `--color-accent` as `borderColor`, `backgroundColor: 'transparent'`
- `tension: 0.3`, `pointRadius: 3`
- Destroys and replaces `activeChart` before creating new one
- Same grid/tick color logic as `renderBpChart` (reads CSS variables for dark mode)
- Y-axis title: `'lbs'`
- Legend: hidden (`display: false`) — only one dataset, no legend needed

### 3. `loadWeightData` — renamed from `loadWeightTable`

Fetches `/api/weight` once, then:
1. Calls `renderWeightChart(entries)`
2. Renders the history table (existing logic)

Called from:
- `renderWeightContent` on initial load
- Form submit handler after a successful save

### 4. `weightRow` — CSS variable tokens

Replace hardcoded Tailwind color classes with CSS variable equivalents:

| Old class | New class |
|---|---|
| `text-gray-600 dark:text-gray-300` | `text-[var(--color-text-secondary)]` |
| `text-gray-800 dark:text-gray-100` | `text-[var(--color-text-primary)]` |
| `text-gray-500 dark:text-gray-400` | `text-[var(--color-text-disabled)]` |

## File Changes

| File | Change |
|---|---|
| `WeightLossTracker/wwwroot/js/app.js` | Restructure `renderWeightContent`, add `renderWeightChart`, rename `loadWeightTable` → `loadWeightData`, fix `weightRow` tokens |

No other files change. No migration, no backend changes, no HTML changes.

## Out of Scope

- Goal reference line on the chart
- Moving average / smoothing
- Stat cards (current weight, total lost, to goal)
- Configurable time window (fixed at last 14 entries, matching BP)
- Edit weight entries (existing delete-only pattern is unchanged)
