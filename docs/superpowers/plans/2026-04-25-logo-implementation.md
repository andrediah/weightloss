# Logo Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the balance-scale SVG with the new running-figure + trend-path logo in all three placements in `index.html`, with automatic light/dark mode switching via CSS.

**Architecture:** All changes are contained in a single file — `WeightLossTracker/wwwroot/index.html`. Each placement gets two sibling SVG elements (`.logo-light` and `.logo-dark`); CSS rules tied to the existing `[data-theme]` attribute and `prefers-color-scheme` media query toggle which one is visible. No JavaScript changes required.

**Tech Stack:** Inline SVG, Tailwind CSS (CDN), existing CSS custom properties (`--color-accent` etc.)

---

## Files

| Action | Path | Change |
|---|---|---|
| Modify | `WeightLossTracker/wwwroot/index.html` | Add CSS rules + replace 3 SVG instances |

---

### Task 1: Add CSS rules for `.logo-light` / `.logo-dark`

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:165` (end of the `<style>` block, before `</style>`)

- [ ] **Step 1: Add logo class CSS**

  Open `WeightLossTracker/wwwroot/index.html`. Find the line `</style>` that closes the main `<style>` block (around line 173). Insert the following CSS immediately before it:

  ```css
  /* ─── Logo theme switching ─────────────────────────────────────────────── */
  .logo-dark { display: none; }
  @media (prefers-color-scheme: dark) {
    .logo-light { display: none; }
    .logo-dark  { display: inline; }
  }
  [data-theme="light"] .logo-light { display: inline; }
  [data-theme="light"] .logo-dark  { display: none;   }
  [data-theme="dark"]  .logo-light { display: none;   }
  [data-theme="dark"]  .logo-dark  { display: inline; }
  ```

- [ ] **Step 2: Verify CSS is inside the style block**

  Confirm the inserted block appears before `</style>` and after the existing `.prose` rules. The structure should end like:

  ```html
    .prose p  { margin-bottom: 0.5rem; }

    /* ─── Logo theme switching ─────────── */
    .logo-dark { display: none; }
    ...
  </style>
  ```

- [ ] **Step 3: Commit**

  ```bash
  git add WeightLossTracker/wwwroot/index.html
  git commit -m "style: add logo-light/logo-dark CSS toggle rules"
  ```

---

### Task 2: Replace sidebar SVG (desktop, 24×24)

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:225-232`

The current SVG at lines 225–232 inside the sidebar `<!-- App name -->` div looks like:

```html
      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" class="text-[var(--color-accent)] flex-shrink-0" aria-hidden="true">
        <path d="m16 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
        <path d="m2 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
        <path d="M7 21h10"/><path d="M12 3v18"/>
        <path d="M3 7h2c2 0 4-1 6-2 2 1 4 2 6 2h2"/>
      </svg>
```

- [ ] **Step 1: Replace sidebar SVG**

  Replace the entire SVG above with the following two-SVG pair:

  ```html
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 48 48"
             class="logo-light flex-shrink-0" aria-hidden="true">
          <circle cx="32" cy="5" r="4.5" fill="#16A34A"/>
          <line x1="32" y1="9.5" x2="26" y2="21" stroke="#16A34A" stroke-width="4" stroke-linecap="round"/>
          <line x1="28" y1="14" x2="20" y2="11" stroke="#16A34A" stroke-width="3" stroke-linecap="round"/>
          <line x1="28" y1="14" x2="35" y2="17" stroke="#86EFAC" stroke-width="3" stroke-linecap="round"/>
          <line x1="26" y1="21" x2="19" y2="32" stroke="#16A34A" stroke-width="3.5" stroke-linecap="round"/>
          <line x1="26" y1="21" x2="33" y2="30" stroke="#86EFAC" stroke-width="3.5" stroke-linecap="round"/>
          <polyline points="4 44 13 36 22 40 36 27" stroke="#16A34A" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
          <polygon points="30,25 38,25 38,33" fill="#16A34A"/>
        </svg>
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 48 48"
             class="logo-dark flex-shrink-0" aria-hidden="true">
          <circle cx="32" cy="5" r="4.5" fill="#5EEAD4"/>
          <line x1="32" y1="9.5" x2="26" y2="21" stroke="#5EEAD4" stroke-width="4" stroke-linecap="round"/>
          <line x1="28" y1="14" x2="20" y2="11" stroke="#5EEAD4" stroke-width="3" stroke-linecap="round"/>
          <line x1="28" y1="14" x2="35" y2="17" stroke="#0D9488" stroke-width="3" stroke-linecap="round"/>
          <line x1="26" y1="21" x2="19" y2="32" stroke="#5EEAD4" stroke-width="3.5" stroke-linecap="round"/>
          <line x1="26" y1="21" x2="33" y2="30" stroke="#0D9488" stroke-width="3.5" stroke-linecap="round"/>
          <polyline points="4 44 13 36 22 40 36 27" stroke="#5EEAD4" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
          <polygon points="30,25 38,25 38,33" fill="#5EEAD4"/>
        </svg>
  ```

- [ ] **Step 2: Load the app and verify sidebar logo**

  Open `http://localhost:<port>` (run the app with `dotnet run` in `WeightLossTracker/`).
  - On a desktop-width window, confirm the new running-figure icon appears in the top of the left sidebar.
  - Toggle dark mode using the sidebar button — the icon should switch from green (`#16A34A`) to teal (`#5EEAD4`) without a flash or layout shift.

- [ ] **Step 3: Commit**

  ```bash
  git add WeightLossTracker/wwwroot/index.html
  git commit -m "feat: replace sidebar balance-scale with running-figure logo"
  ```

---

### Task 3: Replace mobile header SVG (22×22)

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:185-192`

The current SVG at lines 185–192 inside `<header class="md:hidden ...">`:

```html
    <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24"
         fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
         stroke-linejoin="round" class="text-[var(--color-accent)] flex-shrink-0" aria-hidden="true">
      <path d="m16 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
      <path d="m2 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
      <path d="M7 21h10"/><path d="M12 3v18"/>
      <path d="M3 7h2c2 0 4-1 6-2 2 1 4 2 6 2h2"/>
    </svg>
```

- [ ] **Step 1: Replace mobile header SVG**

  Replace the SVG above with the following pair (note `width="22" height="22"`):

  ```html
      <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 48 48"
           class="logo-light flex-shrink-0" aria-hidden="true">
        <circle cx="32" cy="5" r="4.5" fill="#16A34A"/>
        <line x1="32" y1="9.5" x2="26" y2="21" stroke="#16A34A" stroke-width="4" stroke-linecap="round"/>
        <line x1="28" y1="14" x2="20" y2="11" stroke="#16A34A" stroke-width="3" stroke-linecap="round"/>
        <line x1="28" y1="14" x2="35" y2="17" stroke="#86EFAC" stroke-width="3" stroke-linecap="round"/>
        <line x1="26" y1="21" x2="19" y2="32" stroke="#16A34A" stroke-width="3.5" stroke-linecap="round"/>
        <line x1="26" y1="21" x2="33" y2="30" stroke="#86EFAC" stroke-width="3.5" stroke-linecap="round"/>
        <polyline points="4 44 13 36 22 40 36 27" stroke="#16A34A" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        <polygon points="30,25 38,25 38,33" fill="#16A34A"/>
      </svg>
      <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 48 48"
           class="logo-dark flex-shrink-0" aria-hidden="true">
        <circle cx="32" cy="5" r="4.5" fill="#5EEAD4"/>
        <line x1="32" y1="9.5" x2="26" y2="21" stroke="#5EEAD4" stroke-width="4" stroke-linecap="round"/>
        <line x1="28" y1="14" x2="20" y2="11" stroke="#5EEAD4" stroke-width="3" stroke-linecap="round"/>
        <line x1="28" y1="14" x2="35" y2="17" stroke="#0D9488" stroke-width="3" stroke-linecap="round"/>
        <line x1="26" y1="21" x2="19" y2="32" stroke="#5EEAD4" stroke-width="3.5" stroke-linecap="round"/>
        <line x1="26" y1="21" x2="33" y2="30" stroke="#0D9488" stroke-width="3.5" stroke-linecap="round"/>
        <polyline points="4 44 13 36 22 40 36 27" stroke="#5EEAD4" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        <polygon points="30,25 38,25 38,33" fill="#5EEAD4"/>
      </svg>
  ```

- [ ] **Step 2: Verify mobile header logo**

  In browser DevTools, set the viewport to a mobile width (< 768px) or use the responsive design mode.
  - The new logo should appear in the top-left of the mobile header bar.
  - Toggle dark mode — icon should switch to teal.

- [ ] **Step 3: Commit**

  ```bash
  git add WeightLossTracker/wwwroot/index.html
  git commit -m "feat: replace mobile header balance-scale with running-figure logo"
  ```

---

### Task 4: Replace login screen SVG (28×28)

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:459-466`

The current SVG at lines 459–466 inside `<div id="login-view" ...>`:

```html
        <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" class="text-[var(--color-accent)]" aria-hidden="true">
          <path d="m16 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
          <path d="m2 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
          <path d="M7 21h10"/><path d="M12 3v18"/>
          <path d="M3 7h2c2 0 4-1 6-2 2 1 4 2 6 2h2"/>
        </svg>
```

- [ ] **Step 1: Replace login screen SVG**

  Replace the SVG above with the following pair (note `width="28" height="28"`):

  ```html
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 48 48"
               class="logo-light" aria-hidden="true">
            <circle cx="32" cy="5" r="4.5" fill="#16A34A"/>
            <line x1="32" y1="9.5" x2="26" y2="21" stroke="#16A34A" stroke-width="4" stroke-linecap="round"/>
            <line x1="28" y1="14" x2="20" y2="11" stroke="#16A34A" stroke-width="3" stroke-linecap="round"/>
            <line x1="28" y1="14" x2="35" y2="17" stroke="#86EFAC" stroke-width="3" stroke-linecap="round"/>
            <line x1="26" y1="21" x2="19" y2="32" stroke="#16A34A" stroke-width="3.5" stroke-linecap="round"/>
            <line x1="26" y1="21" x2="33" y2="30" stroke="#86EFAC" stroke-width="3.5" stroke-linecap="round"/>
            <polyline points="4 44 13 36 22 40 36 27" stroke="#16A34A" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
            <polygon points="30,25 38,25 38,33" fill="#16A34A"/>
          </svg>
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 48 48"
               class="logo-dark" aria-hidden="true">
            <circle cx="32" cy="5" r="4.5" fill="#5EEAD4"/>
            <line x1="32" y1="9.5" x2="26" y2="21" stroke="#5EEAD4" stroke-width="4" stroke-linecap="round"/>
            <line x1="28" y1="14" x2="20" y2="11" stroke="#5EEAD4" stroke-width="3" stroke-linecap="round"/>
            <line x1="28" y1="14" x2="35" y2="17" stroke="#0D9488" stroke-width="3" stroke-linecap="round"/>
            <line x1="26" y1="21" x2="19" y2="32" stroke="#5EEAD4" stroke-width="3.5" stroke-linecap="round"/>
            <line x1="26" y1="21" x2="33" y2="30" stroke="#0D9488" stroke-width="3.5" stroke-linecap="round"/>
            <polyline points="4 44 13 36 22 40 36 27" stroke="#5EEAD4" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
            <polygon points="30,25 38,25 38,33" fill="#5EEAD4"/>
          </svg>
  ```

- [ ] **Step 2: Verify login screen logo**

  Log out (or open the app unauthenticated so the login modal shows).
  - The new logo should appear to the left of "Weight Loss Tracker" heading in the login card.
  - Toggle dark mode — the icon should switch to teal.

- [ ] **Step 3: Commit**

  ```bash
  git add WeightLossTracker/wwwroot/index.html
  git commit -m "feat: replace login screen balance-scale with running-figure logo"
  ```

---

### Task 5: Final cross-browser verification

- [ ] **Step 1: Verify all three placements in light mode**

  Load the app in a browser (light OS theme). Check:
  - Desktop sidebar (≥768px): green running-figure logo top-left
  - Mobile header (<768px): green running-figure logo in header bar
  - Login screen: green running-figure logo next to app title

- [ ] **Step 2: Verify dark mode (manual toggle)**

  Click the dark mode toggle button in the sidebar (or mobile header). Check all three placements switch to teal (`#5EEAD4`).

- [ ] **Step 3: Verify dark mode (OS preference)**

  In browser DevTools → Rendering → "Emulate CSS prefers-color-scheme: dark". Reload the page. All three placements should show the teal logo on first load (before any manual toggle).

- [ ] **Step 4: Verify no layout shift**

  The `.logo-dark { display: none }` default means only one SVG occupies space at a time. Confirm the sidebar `gap-2` flex row and mobile header `gap-3` row have the same spacing as before — the icon should not cause any wider or taller layout than the old balance-scale.

- [ ] **Step 5: Commit**

  ```bash
  git add WeightLossTracker/wwwroot/index.html
  git commit -m "feat: logo replacement complete — running-figure icon across all placements"
  ```
