# UX Redesign v2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the current indigo/neutral UI with a Warm & Motivational visual language — amber gradient headers, SVG progress ring dashboard, 5 warm accent presets, warm dark mode, and emoji bottom nav across 4 redesigned screens.

**Architecture:** All UI lives in two files — `index.html` (HTML skeleton, CSS tokens, nav chrome, login overlay) and `wwwroot/js/app.js` (router + all view renderers). The redesign updates both files in sequence: tokens and shell first, then each view renderer. No new files are created. The existing Exercise/Meals/History/Profile views keep working but are demoted from the primary 4-tab nav; they remain reachable via the desktop sidebar.

**Tech Stack:** Vanilla JS, Tailwind CSS (CDN, class-based dark mode), Chart.js (CDN), ASP.NET Core static file serving.

---

## File Map

| File | What changes |
|---|---|
| `WeightLossTracker/wwwroot/index.html` | CSS tokens (warm palette + accent vars), nav HTML (4-tab mobile bar, sidebar), login overlay styling |
| `WeightLossTracker/wwwroot/js/app.js` | `C` style constants, accent/theme JS, router (add `trends` + `settings` views), `renderDashboard`, `renderWeight` → Log screen, new `renderTrends`, new `renderSettings` |

---

## Task 1: Replace CSS design tokens with warm palette

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:21-121`

- [ ] **Step 1: Replace the entire `<style>` block token section**

In `index.html`, find the `/* ─── Design tokens */` block (lines ~21–120) and replace the `:root`, `@media (prefers-color-scheme: dark)`, `[data-theme="dark"]`, and `[data-theme="light"]` blocks with:

```css
/* ─── Design tokens — Warm & Motivational (ux-redesign-v2 spec) ─── */
:root {
  /* Base — warm off-white (never changes regardless of accent) */
  --color-surface-primary:   #fffbf0;
  --color-surface-secondary: #fff8ed;
  --color-text-primary:      #1c0800;
  --color-text-secondary:    #92400e;
  --color-text-disabled:     #c4a86a;
  --color-border-default:    #fde68a;
  --color-border-focus:      #d97706;
  --color-feedback-error:    #b91c1c;
  --color-feedback-success:  #15803d;
  --color-feedback-warning:  #b45309;

  /* Accent — Amber default; overridden by applyAccent() */
  --color-accent:      #f59e0b;
  --color-accent-dark: #d97706;

  /* Secondary accent — always violet */
  --color-accent2:      #a78bfa;
  --color-accent2-dark: #7c3aed;

  --spacing-xs:  4px;
  --spacing-sm:  8px;
  --spacing-md:  16px;
  --spacing-lg:  24px;
  --spacing-xl:  32px;
  --spacing-2xl: 48px;

  --radius-sm:   4px;
  --radius-md:   8px;
  --radius-lg:   16px;
  --radius-full: 9999px;
}

/* Warm dark mode — brown-black base, amber preserved */
@media (prefers-color-scheme: dark) {
  :root {
    --color-surface-primary:   #1c1008;
    --color-surface-secondary: #2a1a0a;
    --color-text-primary:      #fef3c7;
    --color-text-secondary:    #fde68a;
    --color-text-disabled:     #a08040;
    --color-border-default:    #3d2008;
    --color-border-focus:      #fbbf24;
    --color-feedback-error:    #fca5a5;
    --color-feedback-success:  #86efac;
    --color-feedback-warning:  #fde047;
  }
}

[data-theme="dark"] {
  --color-surface-primary:   #1c1008;
  --color-surface-secondary: #2a1a0a;
  --color-text-primary:      #fef3c7;
  --color-text-secondary:    #fde68a;
  --color-text-disabled:     #a08040;
  --color-border-default:    #3d2008;
  --color-border-focus:      #fbbf24;
  --color-feedback-error:    #fca5a5;
  --color-feedback-success:  #86efac;
  --color-feedback-warning:  #fde047;
}

[data-theme="light"] {
  --color-surface-primary:   #fffbf0;
  --color-surface-secondary: #fff8ed;
  --color-text-primary:      #1c0800;
  --color-text-secondary:    #92400e;
  --color-text-disabled:     #c4a86a;
  --color-border-default:    #fde68a;
  --color-border-focus:      #d97706;
  --color-feedback-error:    #b91c1c;
  --color-feedback-success:  #15803d;
  --color-feedback-warning:  #b45309;
}
```

- [ ] **Step 2: Replace mobile bottom-tab active state rule**

Find `[data-mobile-nav].mobile-tab-active` and replace with:

```css
/* ─── Mobile bottom-tab active state ─── */
[data-mobile-nav] { border-top: 2px solid transparent; }
[data-mobile-nav].mobile-tab-active {
  border-top-color: var(--color-accent);
  color: var(--color-accent);
}
```

- [ ] **Step 3: Replace scrollbar colours**

Find the `::-webkit-scrollbar` rules and replace with:

```css
::-webkit-scrollbar { width: 5px; height: 5px; }
::-webkit-scrollbar-track { background: #fff8ed; }
::-webkit-scrollbar-thumb { background: #fde68a; border-radius: 4px; }
.dark ::-webkit-scrollbar-track { background: #2a1a0a; }
.dark ::-webkit-scrollbar-thumb { background: #3d2008; }
```

- [ ] **Step 4: Update `<body>` classes**

Find the `<body>` tag and change:
```html
<body class="h-full bg-gray-100 dark:bg-gray-900 flex flex-col md:flex-row transition-colors duration-200">
```
to:
```html
<body class="h-full flex flex-col md:flex-row transition-colors duration-200" style="background-color: var(--color-surface-primary); color: var(--color-text-primary);">
```

- [ ] **Step 5: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "style: replace design tokens with warm palette (ux-redesign-v2)"
```

---

## Task 2: Replace mobile top header and desktop sidebar

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:132-306`

- [ ] **Step 1: Replace mobile top header**

Find the `<!-- Mobile top header -->` block and replace with:

```html
<!-- Mobile top header (hidden on md+) -->
<header class="md:hidden flex items-center gap-3 flex-shrink-0 min-h-[56px] px-4"
        style="background: linear-gradient(135deg, var(--color-accent), var(--color-accent-dark));">
  <span style="font-size:1.1rem; font-weight:800; color:#fff; letter-spacing:0.02em;">
    🏋️ Weight Loss Tracker
  </span>
  <span id="mobile-username" class="text-xs ml-auto mr-2 hidden sm:block" style="color:rgba(255,255,255,0.8);"></span>
  <button data-theme-toggle onclick="toggleTheme()"
          aria-label="Switch to dark mode"
          class="ml-auto flex items-center justify-center w-11 h-11 rounded-lg transition-colors focus:outline-none focus:ring-2"
          style="color:rgba(255,255,255,0.9); focus-ring-color: white;">
    <svg data-moon xmlns="http://www.w3.org/2000/svg" width="18" height="18"
         viewBox="0 0 24 24" fill="none" stroke="currentColor"
         stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
      <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
    </svg>
    <svg data-sun class="hidden" xmlns="http://www.w3.org/2000/svg" width="18" height="18"
         viewBox="0 0 24 24" fill="none" stroke="currentColor"
         stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
      <circle cx="12" cy="12" r="5"/>
      <line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/>
      <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/>
      <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
      <line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/>
      <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/>
      <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
    </svg>
  </button>
</header>
```

- [ ] **Step 2: Replace desktop sidebar**

Find the `<!-- Sidebar navigation (desktop only) -->` block and replace the entire `<nav>` element with:

```html
<!-- Sidebar navigation (desktop only, md+) -->
<nav aria-label="Main navigation"
     class="hidden md:flex md:flex-col w-56 py-6 px-3 flex-shrink-0 min-h-screen"
     style="background: linear-gradient(180deg, var(--color-accent) 0%, var(--color-accent-dark) 100%);">

  <div class="font-extrabold text-lg px-3 mb-8 flex items-center gap-2" style="color:#fff;">
    <span aria-hidden="true">🏋️</span>
    <span class="leading-tight">Weight Loss<br>
      <span class="font-normal text-sm" style="color:rgba(255,255,255,0.8);">Tracker</span>
    </span>
  </div>

  <div class="px-3 mb-4">
    <p id="sidebar-username" class="text-xs font-medium truncate" style="color:rgba(255,255,255,0.8);"></p>
  </div>

  <div class="space-y-1 flex-1" role="list">
    <button data-nav="dashboard" onclick="navigate('dashboard')" role="listitem"
            aria-label="Home"
            class="w-full text-left px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.9);">
      <span aria-hidden="true" style="font-size:1.1rem;">🏠</span> Home
    </button>
    <button data-nav="weight" onclick="navigate('weight')" role="listitem"
            aria-label="Log weight"
            class="w-full text-left px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.9);">
      <span aria-hidden="true" style="font-size:1.1rem;">➕</span> Log
    </button>
    <button data-nav="trends" onclick="navigate('trends')" role="listitem"
            aria-label="Trends"
            class="w-full text-left px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.9);">
      <span aria-hidden="true" style="font-size:1.1rem;">📈</span> Trends
    </button>
    <button data-nav="settings" onclick="navigate('settings')" role="listitem"
            aria-label="Settings"
            class="w-full text-left px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.9);">
      <span aria-hidden="true" style="font-size:1.1rem;">⚙️</span> Settings
    </button>

    <!-- Divider -->
    <div class="my-3 mx-2" style="border-top:1px solid rgba(255,255,255,0.2);"></div>

    <!-- Secondary nav (demoted views) -->
    <button data-nav="exercise" onclick="navigate('exercise')" role="listitem"
            aria-label="Exercise"
            class="w-full text-left px-4 py-2 rounded-lg text-xs font-medium transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.6);">
      <span aria-hidden="true">💪</span> Exercise
    </button>
    <button data-nav="meals" onclick="navigate('meals')" role="listitem"
            aria-label="Meals"
            class="w-full text-left px-4 py-2 rounded-lg text-xs font-medium transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.6);">
      <span aria-hidden="true">🥗</span> Meals
    </button>
    <button data-nav="history" onclick="navigate('history')" role="listitem"
            aria-label="AI History"
            class="w-full text-left px-4 py-2 rounded-lg text-xs font-medium transition-colors flex items-center gap-3 min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.6);">
      <span aria-hidden="true">🕐</span> AI History
    </button>
  </div>

  <div class="mt-4 px-3">
    <p id="sidebar-goal-text" class="text-xs mb-3" style="color:rgba(255,255,255,0.7);"></p>
    <button onclick="handleLogout()"
            class="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-xs font-medium transition-colors min-h-[44px] mb-1 focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.8);">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
        <polyline points="16 17 21 12 16 7"/>
        <line x1="21" y1="12" x2="9" y2="12"/>
      </svg>
      Log out
    </button>
    <button data-theme-toggle onclick="toggleTheme()"
            aria-label="Switch to dark mode"
            class="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-xs font-medium transition-colors min-h-[44px] focus:outline-none focus:ring-2 focus:ring-white"
            style="color:rgba(255,255,255,0.8);">
      <svg data-moon xmlns="http://www.w3.org/2000/svg" width="16" height="16"
           viewBox="0 0 24 24" fill="none" stroke="currentColor"
           stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
      </svg>
      <svg data-sun class="hidden" xmlns="http://www.w3.org/2000/svg" width="16" height="16"
           viewBox="0 0 24 24" fill="none" stroke="currentColor"
           stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <circle cx="12" cy="12" r="5"/>
        <line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/>
        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/>
        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
        <line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/>
        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/>
        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
      </svg>
      <span data-theme-label>Dark mode</span>
    </button>
  </div>
</nav>
```

- [ ] **Step 3: Replace mobile bottom tab bar**

Find `<!-- Mobile bottom tab bar -->` and replace the entire `<nav>` with:

```html
<!-- Mobile bottom tab bar (hidden on md+) -->
<nav aria-label="Mobile navigation"
     class="md:hidden fixed bottom-0 left-0 right-0 flex z-50"
     style="background: var(--color-surface-secondary); border-top: 2px solid var(--color-border-default); padding-bottom: env(safe-area-inset-bottom);">

  <button data-mobile-nav="dashboard" onclick="navigate('dashboard')"
          class="flex-1 flex flex-col items-center justify-center py-2 min-h-[56px] transition-colors focus:outline-none"
          style="color: var(--color-text-disabled);"
          aria-label="Home">
    <span aria-hidden="true" style="font-size:1.25rem;">🏠</span>
    <span class="text-xs mt-0.5 font-semibold uppercase tracking-wide" style="font-size:0.6rem;">Home</span>
  </button>

  <button data-mobile-nav="weight" onclick="navigate('weight')"
          class="flex-1 flex flex-col items-center justify-center py-2 min-h-[56px] transition-colors focus:outline-none"
          style="color: var(--color-text-disabled);"
          aria-label="Log weight">
    <span aria-hidden="true" style="font-size:1.25rem;">➕</span>
    <span class="text-xs mt-0.5 font-semibold uppercase tracking-wide" style="font-size:0.6rem;">Log</span>
  </button>

  <button data-mobile-nav="trends" onclick="navigate('trends')"
          class="flex-1 flex flex-col items-center justify-center py-2 min-h-[56px] transition-colors focus:outline-none"
          style="color: var(--color-text-disabled);"
          aria-label="Trends">
    <span aria-hidden="true" style="font-size:1.25rem;">📈</span>
    <span class="text-xs mt-0.5 font-semibold uppercase tracking-wide" style="font-size:0.6rem;">Trends</span>
  </button>

  <button data-mobile-nav="settings" onclick="navigate('settings')"
          class="flex-1 flex flex-col items-center justify-center py-2 min-h-[56px] transition-colors focus:outline-none"
          style="color: var(--color-text-disabled);"
          aria-label="Settings">
    <span aria-hidden="true" style="font-size:1.25rem;">⚙️</span>
    <span class="text-xs mt-0.5 font-semibold uppercase tracking-wide" style="font-size:0.6rem;">Settings</span>
  </button>
</nav>
```

- [ ] **Step 4: Update main content area background**

Find `<main id="main-content"` and change the class to:
```html
<main id="main-content" class="flex-1 overflow-y-auto p-4 md:p-6 lg:p-8 pb-24 md:pb-8" tabindex="-1" style="background: var(--color-surface-primary);">
```

- [ ] **Step 5: Update login overlay**

Find `<div id="login-view"` and replace with:

```html
<!-- Login view (shown when unauthenticated) -->
<div id="login-view" class="hidden fixed inset-0 z-50 flex items-center justify-center p-4"
     style="background: var(--color-surface-primary);">
  <div class="rounded-2xl w-full max-w-sm p-8 shadow-lg"
       style="background: var(--color-surface-secondary); border: 1px solid var(--color-border-default);">
    <div class="flex items-center gap-3 mb-8">
      <span style="font-size:2rem;">🏋️</span>
      <h1 class="font-extrabold" style="font-size:1.25rem; color: var(--color-text-primary);">
        Weight Loss Tracker
      </h1>
    </div>

    <div id="login-error"></div>

    <form id="login-form" class="space-y-4" novalidate>
      <div>
        <label for="login-username" class="block text-sm mb-1 font-semibold"
               style="color: var(--color-text-secondary);">Username</label>
        <input id="login-username" type="text" autocomplete="username" required
               class="rounded-xl px-3 py-2 w-full focus:outline-none focus:ring-2 min-h-[44px]"
               style="border: 1.5px solid var(--color-border-default); background: var(--color-surface-primary); color: var(--color-text-primary); focus-ring-color: var(--color-accent);"
               placeholder="your username">
      </div>
      <div>
        <label for="login-password" class="block text-sm mb-1 font-semibold"
               style="color: var(--color-text-secondary);">Password</label>
        <input id="login-password" type="password" autocomplete="current-password" required
               class="rounded-xl px-3 py-2 w-full focus:outline-none focus:ring-2 min-h-[44px]"
               style="border: 1.5px solid var(--color-border-default); background: var(--color-surface-primary); color: var(--color-text-primary);"
               placeholder="••••••••">
      </div>
      <button type="submit" id="login-btn"
              class="w-full rounded-xl font-extrabold text-white min-h-[44px] transition-opacity"
              style="background: linear-gradient(135deg, var(--color-accent), var(--color-accent-dark)); font-size:0.95rem;">
        Log in
      </button>
    </form>
  </div>
</div>
```

- [ ] **Step 6: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "style: replace nav chrome and login overlay with warm design (ux-redesign-v2)"
```

---

## Task 3: Add accent theming and update C constants in app.js

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js:1-31` (constants section)
- Modify: `WeightLossTracker/wwwroot/js/app.js:93-132` (dark mode section)

- [ ] **Step 1: Replace the `C` constants object**

Find `const C = {` (line ~7) and replace the entire object with:

```js
const C = {
  card:        'rounded-2xl p-5 shadow-sm',
  cardStyle:   'background: var(--color-surface-secondary); border: 1px solid var(--color-border-default);',
  h1:          'font-extrabold',
  h1Style:     'font-size: clamp(1.5rem,4vw,2rem); color: var(--color-text-primary);',
  h2:          'font-bold mb-4',
  h2Style:     'font-size:1rem; color: var(--color-text-secondary); text-transform:uppercase; letter-spacing:0.08em;',
  label:       'block text-sm mb-1 font-semibold',
  labelStyle:  'color: var(--color-text-secondary);',
  bodyText:    'text-sm',
  bodyStyle:   'color: var(--color-text-primary);',
  mutedText:   'text-sm',
  mutedStyle:  'color: var(--color-text-secondary);',
  tinyText:    'text-xs',
  tinyStyle:   'color: var(--color-text-disabled);',
  input:       'rounded-xl px-3 py-2 w-full focus:outline-none focus:ring-2 min-h-[44px]',
  inputStyle:  'border: 1.5px solid var(--color-border-default); background: var(--color-surface-primary); color: var(--color-text-primary);',
  inputSm:     'rounded-lg px-2 py-1 text-sm focus:outline-none focus:ring-1',
  inputSmStyle:'border: 1px solid var(--color-border-default); background: var(--color-surface-primary); color: var(--color-text-primary);',
  select:      'rounded-xl px-3 py-2 w-full focus:outline-none focus:ring-2 min-h-[44px]',
  selectStyle: 'border: 1.5px solid var(--color-border-default); background: var(--color-surface-primary); color: var(--color-text-primary);',
  btnPrimary:  'rounded-xl font-extrabold text-white px-5 py-2 transition-opacity min-h-[44px]',
  btnPrimaryStyle: 'background: linear-gradient(135deg, var(--color-accent), var(--color-accent-dark));',
  btnSuccess:  'rounded-xl font-extrabold text-white py-2 transition-opacity min-h-[44px]',
  btnSuccessStyle: 'background: linear-gradient(135deg, var(--color-accent), var(--color-accent-dark));',
  btnSecondary:'rounded-xl px-4 py-2 text-sm font-semibold transition-colors min-h-[44px]',
  btnSecStyle: 'background: var(--color-surface-secondary); color: var(--color-text-secondary); border: 1px solid var(--color-border-default);',
  btnSmPrimary:'text-xs rounded-lg px-2 py-1 font-bold text-white transition-opacity',
  btnSmStyle:  'background: var(--color-accent);',
  trow:        'border-b last:border-0',
  trowStyle:   'border-color: var(--color-border-default);',
  divider:     'border-b',
  dividerStyle:'border-color: var(--color-border-default);',
  badge:       'text-xs font-semibold px-2 py-0.5 rounded-full',
};
```

> Note: The new `C` separates class strings from inline styles. When rendering HTML, pair `class="${C.card}"` with `style="${C.cardStyle}"`. Existing views (exercise, meals, history, profile) will still compile — the old Tailwind classes remain valid fallbacks. Update them opportunistically when touching those views in future.

- [ ] **Step 2: Add accent preset data and `initAccent` / `applyAccent` functions**

After the `C` constants block, add:

```js
// ─── Accent presets ────────────────────────────────────────────────────────────
const ACCENTS = {
  amber:  { accent: '#f59e0b', accentDark: '#d97706', label: 'Amber'  },
  rose:   { accent: '#f472b6', accentDark: '#db2777', label: 'Rose'   },
  coral:  { accent: '#fb923c', accentDark: '#ea580c', label: 'Coral'  },
  gold:   { accent: '#eab308', accentDark: '#ca8a04', label: 'Gold'   },
  violet: { accent: '#a78bfa', accentDark: '#7c3aed', label: 'Violet' },
};

function initAccent() {
  const saved = localStorage.getItem('wlt-accent') || 'amber';
  applyAccent(saved);
}

function applyAccent(name) {
  const preset = ACCENTS[name] || ACCENTS.amber;
  const root = document.documentElement;
  root.style.setProperty('--color-accent',      preset.accent);
  root.style.setProperty('--color-accent-dark', preset.accentDark);
  root.setAttribute('data-accent', name);
  localStorage.setItem('wlt-accent', name);
  // Refresh active nav indicator colour
  document.querySelectorAll('[data-mobile-nav].mobile-tab-active').forEach(el => {
    el.style.borderTopColor = preset.accent;
    el.style.color = preset.accent;
  });
}
```

- [ ] **Step 3: Update `initTheme` to use `wlt-theme` key and call `initAccent`**

Find the `initTheme` function and replace:
```js
function initTheme() {
  const saved = localStorage.getItem('theme');
```
with:
```js
function initTheme() {
  const saved = localStorage.getItem('wlt-theme');
```

Find `localStorage.getItem('theme')` in the `change` event listener inside `initTheme` and replace with `localStorage.getItem('wlt-theme')`.

- [ ] **Step 4: Update `toggleTheme` to persist to `wlt-theme`**

Find `toggleTheme` and replace:
```js
function toggleTheme() {
  const isDark = !document.documentElement.classList.contains('dark');
  localStorage.setItem('theme', isDark ? 'dark' : 'light');
  applyTheme(isDark);
}
```
with:
```js
function toggleTheme() {
  const isDark = !document.documentElement.classList.contains('dark');
  localStorage.setItem('wlt-theme', isDark ? 'dark' : 'light');
  applyTheme(isDark);
}
```

- [ ] **Step 5: Add `trends` and `settings` to the router and update `navigate` active state**

Find the `navigate` function. Replace the `const views = {` block with:

```js
const views = {
  dashboard: renderDashboard,
  weight:    renderLog,
  trends:    renderTrends,
  settings:  renderSettings,
  exercise:  renderExercise,
  meals:     renderMeals,
  history:   renderHistory,
  profile:   renderProfile,
};
```

Find the desktop sidebar active state block inside `navigate`:
```js
document.querySelectorAll('[data-nav]').forEach(el => {
  const active = el.dataset.nav === viewName;
  el.classList.toggle('bg-indigo-700',            active);
  el.classList.toggle('dark:bg-indigo-800',       active);
  el.classList.toggle('text-white',               active);
  el.classList.toggle('text-indigo-100',         !active);
});
```
Replace with:
```js
document.querySelectorAll('[data-nav]').forEach(el => {
  const active = el.dataset.nav === viewName;
  el.style.background = active ? 'rgba(255,255,255,0.25)' : '';
  el.style.color = active ? '#fff' : '';
});
```

- [ ] **Step 6: Update the loading spinner to use accent colour**

Find `border-b-2 border-indigo-600` in the loading spinner and replace with `border-b-2` and add `style="border-color: var(--color-accent);"`:

```js
root.innerHTML = `
  <div class="flex justify-center py-16" role="status" aria-label="Loading">
    <div class="animate-spin rounded-full h-10 w-10 border-b-2"
         style="border-color: var(--color-accent);"></div>
  </div>`;
```

- [ ] **Step 7: Call `initAccent` at the bottom of the file**

Find the last two lines:
```js
initTheme();
initAuth().then(authenticated => {
```
Replace with:
```js
initTheme();
initAccent();
initAuth().then(authenticated => {
```

- [ ] **Step 8: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat: add accent preset system and update C constants (ux-redesign-v2)"
```

---

## Task 4: Rewrite Dashboard view

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js` — `renderDashboard` function

- [ ] **Step 1: Replace `renderDashboard` entirely**

Find `async function renderDashboard()` and replace it and its closing `}` with:

```js
// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function renderDashboard() {
  const root = document.getElementById('view-root');
  const r = await Bridge.call('getDashboard');
  if (!r.ok) {
    root.innerHTML = `<p style="color:var(--color-feedback-error);padding:1rem;">
      Failed to load dashboard: ${escHtml(r.data?.detail || r.data)}</p>`;
    return;
  }
  const d = r.data;
  const pct = Math.min(100, Math.max(0, d.progressPct || 0));
  const circumference = 2 * Math.PI * 46; // r=46 in viewBox 110x110
  const offset = circumference * (1 - pct / 100);
  const greeting = timeGreeting();
  const cwDisplay = d.currentWeight != null ? d.currentWeight.toFixed(1) : '—';

  root.innerHTML = `
    <div>
      <!-- Gradient header with progress ring -->
      <div class="rounded-2xl p-5 mb-4 text-white"
           style="background: linear-gradient(160deg, var(--color-accent) 0%, var(--color-accent-dark) 100%);">
        <div class="flex justify-between items-start mb-4">
          <div>
            <div class="font-bold text-xs uppercase tracking-widest" style="color:rgba(255,255,255,0.75);">
              ${greeting.time}
            </div>
            <div class="font-extrabold mt-1" style="font-size:clamp(1.1rem,3vw,1.4rem);">
              ${escHtml(greeting.line)}
            </div>
          </div>
          <div class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0"
               style="background:rgba(255,255,255,0.2); font-size:1.25rem;">
            ${greeting.emoji}
          </div>
        </div>

        <!-- SVG progress ring -->
        <div class="flex justify-center">
          <div class="relative inline-block">
            <svg width="110" height="110" viewBox="0 0 110 110"
                 role="progressbar"
                 aria-valuenow="${pct}"
                 aria-valuemin="0"
                 aria-valuemax="100"
                 aria-label="Weight loss progress: ${pct}%">
              <circle cx="55" cy="55" r="46" fill="none"
                      stroke="rgba(255,255,255,0.25)" stroke-width="10"/>
              <circle cx="55" cy="55" r="46" fill="none"
                      stroke="#ffffff" stroke-width="10"
                      stroke-dasharray="${circumference.toFixed(1)}"
                      stroke-dashoffset="${offset.toFixed(1)}"
                      stroke-linecap="round"
                      transform="rotate(-90 55 55)"
                      style="transition: stroke-dashoffset 0.7s ease;"/>
            </svg>
            <div class="absolute inset-0 flex flex-col items-center justify-center text-center">
              <div class="font-black text-white" style="font-size:1.5rem; line-height:1;">${pct}%</div>
              <div class="font-semibold mt-1" style="font-size:0.65rem; color:rgba(255,255,255,0.8);">to goal 🎯</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Stat cards -->
      <div class="grid grid-cols-3 gap-3 mb-4">
        ${statCard(cwDisplay, 'lbs now', 'var(--color-accent)')}
        ${statCard(d.lostSoFar != null ? '−' + d.lostSoFar : '—', 'lost 🔥', 'var(--color-accent2)')}
        ${statCard(d.goalWeight != null ? d.goalWeight.toFixed(1) : '—', 'goal 🏁', '#15803d')}
      </div>

      <!-- Recent entries -->
      <div class="rounded-2xl p-4" style="${C.cardStyle}">
        <div class="font-bold mb-3 text-xs uppercase tracking-widest"
             style="color: var(--color-text-secondary);">Recent entries</div>
        <div id="recent-entries-list">
          <div class="text-sm" style="color:var(--color-text-disabled);">Loading…</div>
        </div>
      </div>
    </div>`;

  // Load recent entries
  const wr = await Bridge.call('getWeightEntries');
  const recentEl = document.getElementById('recent-entries-list');
  if (recentEl) {
    if (!wr.ok || !wr.data.length) {
      recentEl.innerHTML = `<p class="text-sm" style="color:var(--color-text-disabled);">
        No entries yet. Tap ➕ to log your first weight.</p>`;
    } else {
      recentEl.innerHTML = wr.data.slice(0, 3).map((e, i) => `
        <div class="flex justify-between items-center py-2 ${i < 2 ? 'border-b' : ''}"
             style="border-color: var(--color-border-default);">
          <span class="text-sm" style="color: var(--color-text-${i === 0 ? 'primary' : 'secondary'});">
            ${i === 0 ? 'Today' : i === 1 ? 'Yesterday' : fmtDate(e.date)}
          </span>
          <span class="font-bold text-sm" style="color: ${i === 0 ? 'var(--color-accent)' : 'var(--color-text-disabled)'};">
            ${e.weight.toFixed(1)} lbs
          </span>
        </div>`).join('');
    }
  }
}

function timeGreeting() {
  const h = new Date().getHours();
  const lines = [
    "You're doing amazing! 💪",
    "Keep pushing forward!",
    "Every step counts! 🌟",
    "Progress, not perfection!",
    "You've got this! 🎯",
  ];
  const line = lines[Math.floor(Math.random() * lines.length)];
  if (h < 12) return { time: 'Good morning 🌅', emoji: '😊', line };
  if (h < 17) return { time: 'Good afternoon ☀️', emoji: '💪', line };
  return { time: 'Good evening 🌙', emoji: '🌟', line };
}

function statCard(value, label, color) {
  return `
    <div class="rounded-2xl p-3 text-center" style="${C.cardStyle}">
      <div class="font-black" style="font-size:1.2rem; color:${color};">${escHtml(String(value))}</div>
      <div class="font-semibold mt-1" style="font-size:0.7rem; color:var(--color-text-secondary);">${label}</div>
    </div>`;
}
```

- [ ] **Step 2: Delete the old `kpiCard` function** (it is replaced by `statCard`)

Find `function kpiCard(label, value, colorClass, icon) {` and delete it and its closing `}`.

- [ ] **Step 3: Manually verify in browser**

Start the app (`dotnet run` in `WeightLossTracker/`) and open `http://localhost:<port>`. Log in, confirm:
- Gradient amber header visible
- SVG ring shows the correct percentage
- 3 stat cards below the ring
- Recent entries list loads

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat: redesign dashboard — progress ring hero, stat cards, recent entries (ux-redesign-v2)"
```

---

## Task 5: Rewrite Log Entry view

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js` — rename `renderWeight` → `renderLog`, full rewrite

- [ ] **Step 1: Replace `renderWeight` with `renderLog`**

Find `async function renderWeight()` and replace the entire function (through its closing `}`) with:

```js
// ─── LOG ENTRY ─────────────────────────────────────────────────────────────────
async function renderLog() {
  const root = document.getElementById('view-root');

  // Build date strip: last 7 days, today selected
  const today = new Date();
  const dayAbbr = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
  const dates = Array.from({ length: 7 }, (_, i) => {
    const d = new Date(today);
    d.setDate(today.getDate() - (6 - i));
    return d;
  });
  const selectedIdx = 6; // today

  root.innerHTML = `
    <div>
      <!-- Violet gradient header -->
      <div class="rounded-2xl p-5 mb-4 text-white"
           style="background: linear-gradient(160deg, var(--color-accent2) 0%, var(--color-accent2-dark) 100%);">
        <div class="font-bold text-xs uppercase tracking-widest mb-1"
             style="color:rgba(255,255,255,0.75);">Log your weight</div>
        <div class="font-extrabold" style="font-size:clamp(1.1rem,3vw,1.4rem);">
          How did you do today? 💪
        </div>
      </div>

      <!-- Date strip -->
      <div class="flex gap-2 overflow-x-auto pb-1 mb-4" style="scrollbar-width:none;">
        ${dates.map((d, i) => {
          const isSelected = i === selectedIdx;
          return `
          <button data-date-btn="${d.toISOString().slice(0,10)}"
                  onclick="selectLogDate(this, '${d.toISOString().slice(0,10)}')"
                  class="flex-shrink-0 flex flex-col items-center justify-center rounded-xl min-w-[48px] py-2 transition-colors focus:outline-none focus:ring-2"
                  style="${isSelected
                    ? 'background:var(--color-accent2-dark);color:#fff;'
                    : 'background:var(--color-surface-secondary);color:var(--color-text-secondary);border:1px solid var(--color-border-default);'}"
                  aria-pressed="${isSelected}"
                  aria-label="${d.toLocaleDateString(undefined, {weekday:'long', month:'short', day:'numeric'})}">
            <span style="font-size:0.6rem;font-weight:700;text-transform:uppercase;letter-spacing:0.06em;opacity:0.75;">
              ${dayAbbr[d.getDay()]}
            </span>
            <span style="font-size:1rem;font-weight:800;">${d.getDate()}</span>
          </button>`;
        }).join('')}
      </div>

      <!-- Weight input card -->
      <div class="rounded-2xl p-5 mb-4 text-center" style="${C.cardStyle}">
        <div class="font-bold text-xs uppercase tracking-widest mb-4"
             style="color:var(--color-text-secondary);">Weight (lbs)</div>
        <div id="log-error"></div>
        <div id="weight-display"
             class="font-black mb-4"
             style="font-size:clamp(2.5rem,10vw,4rem); color:var(--color-accent); line-height:1;">
          <span id="weight-int">—</span><span id="weight-dec"
            style="font-size:0.5em; color:var(--color-accent2);">.0</span>
        </div>
        <div class="flex justify-center items-center gap-6 mb-4">
          <button onclick="adjustWeight(-0.1)" aria-label="Decrease by 0.1"
                  class="flex items-center justify-center rounded-full font-black text-2xl transition-colors focus:outline-none focus:ring-2"
                  style="width:52px;height:52px;background:var(--color-surface-secondary);color:var(--color-accent);border:1.5px solid var(--color-border-default);">
            −
          </button>
          <input id="wt-weight" type="number" step="0.1" min="50" max="999"
                 oninput="syncWeightDisplay(this.value)"
                 onblur="validateWeightInline()"
                 aria-label="Weight in lbs"
                 class="text-center font-bold rounded-xl px-2 py-1 w-24 focus:outline-none focus:ring-2"
                 style="font-size:0.9rem;border:1.5px solid var(--color-border-default);background:var(--color-surface-primary);color:var(--color-text-primary);"
                 placeholder="e.g. 192.0">
          <button onclick="adjustWeight(0.1)" aria-label="Increase by 0.1"
                  class="flex items-center justify-center rounded-full font-black text-2xl transition-colors focus:outline-none focus:ring-2"
                  style="width:52px;height:52px;background:var(--color-surface-secondary);color:var(--color-accent);border:1.5px solid var(--color-border-default);">
            +
          </button>
        </div>
        <p id="wt-weight-error" class="text-sm mb-2 hidden"
           style="color:var(--color-feedback-error);" role="alert"></p>
      </div>

      <!-- Notes card -->
      <div class="rounded-2xl p-4 mb-4" style="${C.cardStyle}">
        <label for="wt-notes" class="block font-bold text-xs uppercase tracking-widest mb-2"
               style="color:var(--color-text-secondary);">📝 Note (optional)</label>
        <input id="wt-notes" type="text"
               class="rounded-xl px-3 py-2 w-full focus:outline-none focus:ring-2 min-h-[44px]"
               style="border:1.5px solid var(--color-border-default);background:var(--color-surface-primary);color:var(--color-text-primary);"
               placeholder="Morning weigh-in, after workout…">
      </div>

      <!-- Save button -->
      <button id="log-save-btn" onclick="saveLogEntry()"
              class="w-full rounded-2xl font-extrabold text-white min-h-[52px] transition-opacity"
              style="background:linear-gradient(135deg,var(--color-accent),var(--color-accent-dark));font-size:1rem;">
        Save Entry ✓
      </button>

      <!-- History section -->
      <div class="mt-6 rounded-2xl p-4" style="${C.cardStyle}">
        <div class="font-bold text-xs uppercase tracking-widest mb-3"
             style="color:var(--color-text-secondary);">All entries</div>
        <div id="weight-error"></div>
        <div id="weight-table-wrap"></div>
      </div>
    </div>`;

  // Pre-fill today's weight if already logged
  const r = await Bridge.call('getWeightEntries');
  if (r.ok && r.data.length) {
    const todayStr = today.toISOString().slice(0,10);
    const todayEntry = r.data.find(e => e.date?.slice(0,10) === todayStr);
    if (todayEntry) {
      document.getElementById('wt-weight').value = todayEntry.weight.toFixed(1);
      syncWeightDisplay(todayEntry.weight.toFixed(1));
    }
  }

  await loadWeightTable();
}

let _logSelectedDate = new Date().toISOString().slice(0,10);

function selectLogDate(btn, dateStr) {
  _logSelectedDate = dateStr;
  document.querySelectorAll('[data-date-btn]').forEach(b => {
    const sel = b.dataset.dateBtn === dateStr;
    b.style.background = sel ? 'var(--color-accent2-dark)' : 'var(--color-surface-secondary)';
    b.style.color = sel ? '#fff' : 'var(--color-text-secondary)';
    b.style.border = sel ? 'none' : '1px solid var(--color-border-default)';
    b.setAttribute('aria-pressed', sel);
  });
}

function adjustWeight(delta) {
  const input = document.getElementById('wt-weight');
  const current = parseFloat(input.value) || 0;
  const next = Math.round((current + delta) * 10) / 10;
  if (next >= 50 && next <= 999) {
    input.value = next.toFixed(1);
    syncWeightDisplay(input.value);
  }
}

function syncWeightDisplay(val) {
  const num = parseFloat(val);
  const intEl = document.getElementById('weight-int');
  const decEl = document.getElementById('weight-dec');
  if (!intEl || !decEl) return;
  if (isNaN(num)) {
    intEl.textContent = '—';
    decEl.textContent = '';
    return;
  }
  const parts = num.toFixed(1).split('.');
  intEl.textContent = parts[0];
  decEl.textContent = '.' + (parts[1] || '0');
}

function validateWeightInline() {
  const val = parseFloat(document.getElementById('wt-weight')?.value ?? '');
  const errEl = document.getElementById('wt-weight-error');
  if (!errEl) return true;
  if (isNaN(val) || val < 50 || val > 999) {
    errEl.textContent = 'Weight must be between 50 and 999 lbs.';
    errEl.classList.remove('hidden');
    return false;
  }
  errEl.classList.add('hidden');
  return true;
}

async function saveLogEntry() {
  if (!validateWeightInline()) return;
  const weight = parseFloat(document.getElementById('wt-weight').value);
  const notes  = document.getElementById('wt-notes').value.trim() || null;
  const btn    = document.getElementById('log-save-btn');

  btn.disabled = true;
  btn.textContent = 'Saving…';

  const r = await Bridge.call('saveWeight', { weight, notes });

  btn.disabled = false;
  btn.textContent = 'Save Entry ✓';

  if (!r.ok) {
    showError('log-error', r.data?.detail || r.data);
    return;
  }
  document.getElementById('wt-notes').value = '';
  await loadWeightTable();

  // Brief success indicator
  btn.textContent = 'Saved! ✓';
  btn.style.background = 'linear-gradient(135deg,#15803d,#166534)';
  setTimeout(() => {
    btn.textContent = 'Save Entry ✓';
    btn.style.background = '';
  }, 1500);
}
```

- [ ] **Step 2: Update router alias**

In the `navigate` function, the router already maps `weight → renderLog` from Task 3 Step 5. Confirm the alias is present. No change needed if Task 3 is complete.

- [ ] **Step 3: Verify in browser**

Navigate to the Log tab. Confirm:
- Violet gradient header
- 7-day date strip scrolls, tapping highlights selected day
- Weight display updates as ± buttons are tapped
- Validation error appears below input on blur with invalid value
- Save clears notes, refreshes history table, shows green confirmation

- [ ] **Step 4: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat: redesign log entry — date strip, big weight input, violet header (ux-redesign-v2)"
```

---

## Task 6: Add Trends view

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js` — add `renderTrends` function after `renderLog`

- [ ] **Step 1: Add `renderTrends` function**

After the `saveLogEntry` function, add:

```js
// ─── TRENDS ────────────────────────────────────────────────────────────────────
let _trendsPeriod = '1W';

async function renderTrends() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div>
      <!-- Amber gradient header -->
      <div class="rounded-2xl p-5 mb-4 text-white"
           style="background: linear-gradient(160deg, var(--color-accent) 0%, var(--color-accent-dark) 100%);">
        <div class="font-bold text-xs uppercase tracking-widest mb-1"
             style="color:rgba(255,255,255,0.75);">Your trends 📈</div>
        <div class="font-extrabold mb-4" style="font-size:clamp(1.1rem,3vw,1.4rem);">
          See your journey
        </div>
        <!-- Period tabs -->
        <div class="flex gap-2">
          ${['1W','1M','3M','All'].map(p => `
            <button data-period="${p}" onclick="setTrendsPeriod('${p}')"
                    class="rounded-full font-bold text-xs px-3 py-1.5 transition-colors focus:outline-none focus:ring-2 focus:ring-white min-h-[32px]"
                    style="${p === _trendsPeriod
                      ? 'background:#fff;color:var(--color-accent-dark);'
                      : 'background:rgba(255,255,255,0.25);color:#fff;'}"
                    aria-pressed="${p === _trendsPeriod}">
              ${p}
            </button>`).join('')}
        </div>
      </div>

      <!-- Chart card -->
      <div class="rounded-2xl p-4 mb-4" style="${C.cardStyle}">
        <div id="trends-chart-wrap">
          <canvas id="trends-chart" height="120"></canvas>
        </div>
      </div>

      <!-- Summary stats -->
      <div class="grid grid-cols-2 gap-3 mb-4" id="trends-stats"></div>
    </div>`;

  await loadTrendsChart();
}

async function setTrendsPeriod(period) {
  _trendsPeriod = period;
  document.querySelectorAll('[data-period]').forEach(btn => {
    const active = btn.dataset.period === period;
    btn.style.background = active ? '#fff' : 'rgba(255,255,255,0.25)';
    btn.style.color = active ? 'var(--color-accent-dark)' : '#fff';
    btn.setAttribute('aria-pressed', active);
  });
  await loadTrendsChart();
}

async function loadTrendsChart() {
  const r = await Bridge.call('getWeightEntries');
  if (!r.ok) return;

  const all = r.data.slice().sort((a, b) => new Date(a.date) - new Date(b.date));

  const cutoff = new Date();
  const cutoffs = { '1W': 7, '1M': 30, '3M': 90 };
  const days = cutoffs[_trendsPeriod];
  const entries = days
    ? all.filter(e => (Date.now() - new Date(e.date)) / 86400000 <= days)
    : all;

  const statsEl = document.getElementById('trends-stats');

  if (!entries.length) {
    document.getElementById('trends-chart-wrap').innerHTML =
      `<p class="text-center py-8 text-sm" style="color:var(--color-text-disabled);">
         No entries in this period. Log some weights first.</p>`;
    if (statsEl) statsEl.innerHTML = '';
    return;
  }

  const labels  = entries.map(e => {
    const d = new Date(e.date);
    return _trendsPeriod === '1W'
      ? ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'][d.getDay()]
      : d.toLocaleDateString(undefined, { month:'short', day:'numeric' });
  });
  const weights = entries.map(e => e.weight);
  const avg     = (weights.reduce((s, w) => s + w, 0) / weights.length).toFixed(1);
  const total   = (weights[0] - weights[weights.length - 1]).toFixed(1);

  // Destroy previous chart if present
  if (activeChart) { activeChart.destroy(); activeChart = null; }

  const isDark   = document.documentElement.classList.contains('dark');
  const tickColor = isDark ? '#fde68a' : '#92400e';
  const accent   = getComputedStyle(document.documentElement)
                     .getPropertyValue('--color-accent').trim() || '#f59e0b';
  const accentDark = getComputedStyle(document.documentElement)
                     .getPropertyValue('--color-accent-dark').trim() || '#d97706';

  const ctx = document.getElementById('trends-chart')?.getContext('2d');
  if (!ctx) return;

  const gradient = ctx.createLinearGradient(0, 0, 0, 200);
  gradient.addColorStop(0, accent + '66');
  gradient.addColorStop(1, accent + '00');

  activeChart = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: 'Weight (lbs)',
        data: weights,
        borderColor: accent,
        backgroundColor: gradient,
        tension: 0.35,
        pointRadius: entries.length <= 14 ? 4 : 2,
        pointBackgroundColor: accent,
        fill: true,
        borderWidth: 2.5,
      }]
    },
    options: {
      responsive: true,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: isDark ? '#2a1a0a' : '#fff8ed',
          titleColor: accentDark,
          bodyColor: isDark ? '#fde68a' : '#92400e',
          borderColor: accent,
          borderWidth: 1,
        }
      },
      scales: {
        x: {
          ticks: { color: tickColor, font: { size: 10 } },
          grid: { display: false },
        },
        y: {
          ticks: { color: tickColor, font: { size: 10 } },
          grid: { color: isDark ? 'rgba(253,230,138,0.08)' : 'rgba(146,64,14,0.08)' },
          title: { display: false },
        }
      }
    }
  });

  // Stat cards
  if (statsEl) {
    const weekLosses = [];
    for (let i = 7; i < weights.length; i++) {
      weekLosses.push(weights[i - 7] - weights[i]);
    }
    const bestWeek = weekLosses.length
      ? Math.max(...weekLosses).toFixed(1)
      : '—';

    statsEl.innerHTML = `
      ${trendsStatCard('Avg this period', avg + ' lbs', 'var(--color-accent)')}
      ${trendsStatCard('Best week loss', bestWeek !== '—' ? '−' + bestWeek + ' lbs' : '—', 'var(--color-accent2)')}`;
  }
}

function trendsStatCard(label, value, color) {
  return `
    <div class="rounded-2xl p-4" style="${C.cardStyle}">
      <div class="font-bold text-xs uppercase tracking-widest mb-2"
           style="color:var(--color-text-secondary);">${label}</div>
      <div class="font-black" style="font-size:1.3rem; color:${color};">${escHtml(value)}</div>
    </div>`;
}
```

- [ ] **Step 2: Verify in browser**

Navigate to Trends tab. Confirm:
- Amber gradient header with period tabs
- Chart renders with amber line and amber fill gradient
- Switching period tabs re-renders the chart
- Summary stat cards show avg and best week loss

- [ ] **Step 3: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat: add Trends view — Chart.js amber styling, period tabs, stat cards (ux-redesign-v2)"
```

---

## Task 7: Add Settings view

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js` — add `renderSettings` function

- [ ] **Step 1: Add `renderSettings` function** after `trendsStatCard`:

```js
// ─── SETTINGS ──────────────────────────────────────────────────────────────────
async function renderSettings() {
  const root = document.getElementById('view-root');
  const currentAccent = localStorage.getItem('wlt-accent') || 'amber';
  const isDark = document.documentElement.classList.contains('dark');
  const units = localStorage.getItem('wlt-units') || 'lbs';
  const goal  = activeProfile?.goalWeight ?? '';

  root.innerHTML = `
    <div>
      <!-- Amber gradient header -->
      <div class="rounded-2xl p-5 mb-4 text-white"
           style="background: linear-gradient(160deg, var(--color-accent) 0%, var(--color-accent-dark) 100%);">
        <div class="font-bold text-xs uppercase tracking-widest mb-1"
             style="color:rgba(255,255,255,0.75);">Settings ⚙️</div>
        <div class="font-extrabold" style="font-size:clamp(1.1rem,3vw,1.4rem);">Make it yours</div>
      </div>

      <!-- Accent colour -->
      <div class="rounded-2xl p-4 mb-3" style="${C.cardStyle}">
        <div class="font-bold text-xs uppercase tracking-widest mb-3"
             style="color:var(--color-text-secondary);">🎨 Accent colour</div>
        <div class="flex gap-3 flex-wrap">
          ${Object.entries(ACCENTS).map(([key, preset]) => `
            <button data-accent-swatch="${key}"
                    onclick="applyAccent('${key}'); renderSettingsAccentState('${key}')"
                    aria-label="${preset.label} accent"
                    aria-pressed="${key === currentAccent}"
                    class="rounded-full transition-transform focus:outline-none focus:ring-2 focus:ring-offset-2"
                    style="width:36px;height:36px;background:linear-gradient(135deg,${preset.accent},${preset.accentDark});
                           ${key === currentAccent ? 'box-shadow:0 0 0 3px var(--color-surface-primary),0 0 0 5px ' + preset.accent + ';' : ''}">
            </button>`).join('')}
        </div>
      </div>

      <!-- Dark mode toggle -->
      <div class="rounded-2xl p-4 mb-3 flex justify-between items-center" style="${C.cardStyle}">
        <div>
          <div class="font-bold text-sm" style="color:var(--color-text-primary);">🌙 Dark mode</div>
          <div class="text-xs mt-0.5" style="color:var(--color-text-secondary);">Warm dark theme</div>
        </div>
        <button id="settings-dark-toggle"
                role="switch"
                aria-checked="${isDark}"
                onclick="settingsToggleDark()"
                class="relative flex-shrink-0 rounded-full transition-colors focus:outline-none focus:ring-2"
                style="width:48px;height:28px;background:${isDark ? 'var(--color-accent)' : 'var(--color-border-default)'};">
          <span id="settings-dark-knob"
                class="absolute top-0.5 rounded-full transition-transform"
                style="width:24px;height:24px;background:#fff;box-shadow:0 1px 4px rgba(0,0,0,0.2);
                       transform: translateX(${isDark ? '22px' : '2px'});"></span>
        </button>
      </div>

      <!-- Goal weight inline edit -->
      <div class="rounded-2xl p-4 mb-3" style="${C.cardStyle}" id="settings-goal-card">
        <div class="font-bold text-xs uppercase tracking-widest mb-2"
             style="color:var(--color-text-secondary);">🏁 Goal weight</div>
        <div class="flex justify-between items-center" id="settings-goal-display">
          <div class="font-black" style="font-size:1.5rem; color:var(--color-accent);">
            ${goal ? goal + ' <span style="font-size:0.6em;color:var(--color-text-secondary);">' + units + '</span>' : '—'}
          </div>
          <button onclick="startGoalEdit()"
                  class="rounded-full font-bold text-xs px-3 py-1.5 min-h-[36px] focus:outline-none focus:ring-2"
                  style="background:var(--color-surface-secondary);color:var(--color-accent);border:1.5px solid var(--color-accent);">
            Edit
          </button>
        </div>
        <div id="settings-goal-edit" class="hidden">
          <div id="settings-goal-error"></div>
          <div class="flex gap-2 items-center mt-2">
            <input id="settings-goal-input" type="number" step="0.1" min="50" max="999"
                   value="${goal}"
                   aria-label="Goal weight"
                   class="rounded-xl px-3 py-2 w-32 font-bold text-center focus:outline-none focus:ring-2 min-h-[44px]"
                   style="border:1.5px solid var(--color-accent);background:var(--color-surface-primary);color:var(--color-text-primary);font-size:1.1rem;">
            <button onclick="saveGoalEdit()"
                    class="rounded-full font-bold text-xs px-4 py-1.5 min-h-[44px] focus:outline-none focus:ring-2"
                    style="background:var(--color-accent);color:#fff;">Save</button>
            <button onclick="cancelGoalEdit()"
                    class="rounded-full font-bold text-xs px-4 py-1.5 min-h-[44px] focus:outline-none focus:ring-2"
                    style="background:var(--color-surface-secondary);color:var(--color-text-secondary);border:1px solid var(--color-border-default);">
              Cancel
            </button>
          </div>
        </div>
      </div>

      <!-- Units segmented control -->
      <div class="rounded-2xl p-4 mb-3 flex justify-between items-center" style="${C.cardStyle}">
        <div class="font-bold text-sm" style="color:var(--color-text-primary);">⚖️ Units</div>
        <div class="flex overflow-hidden rounded-full"
             style="border:1.5px solid var(--color-border-default);">
          <button id="units-lbs" onclick="setUnits('lbs')"
                  class="font-bold text-xs px-4 py-2 min-h-[36px] focus:outline-none transition-colors"
                  style="${units === 'lbs'
                    ? 'background:var(--color-accent);color:#fff;'
                    : 'background:var(--color-surface-secondary);color:var(--color-text-secondary);'}">
            lbs
          </button>
          <button id="units-kg" onclick="setUnits('kg')"
                  class="font-bold text-xs px-4 py-2 min-h-[36px] focus:outline-none transition-colors"
                  style="${units === 'kg'
                    ? 'background:var(--color-accent);color:#fff;'
                    : 'background:var(--color-surface-secondary);color:var(--color-text-secondary);'}">
            kg
          </button>
        </div>
      </div>
    </div>`;
}

function renderSettingsAccentState(activeKey) {
  document.querySelectorAll('[data-accent-swatch]').forEach(btn => {
    const key = btn.dataset.accentSwatch;
    const preset = ACCENTS[key];
    btn.setAttribute('aria-pressed', key === activeKey);
    btn.style.boxShadow = key === activeKey
      ? `0 0 0 3px var(--color-surface-primary),0 0 0 5px ${preset.accent}`
      : 'none';
  });
}

function settingsToggleDark() {
  toggleTheme();
  const isDark = document.documentElement.classList.contains('dark');
  const toggle = document.getElementById('settings-dark-toggle');
  const knob   = document.getElementById('settings-dark-knob');
  if (toggle) {
    toggle.setAttribute('aria-checked', isDark);
    toggle.style.background = isDark ? 'var(--color-accent)' : 'var(--color-border-default)';
  }
  if (knob) knob.style.transform = `translateX(${isDark ? '22px' : '2px'})`;
}

function startGoalEdit() {
  document.getElementById('settings-goal-display').classList.add('hidden');
  document.getElementById('settings-goal-edit').classList.remove('hidden');
  document.getElementById('settings-goal-input').focus();
}

function cancelGoalEdit() {
  document.getElementById('settings-goal-display').classList.remove('hidden');
  document.getElementById('settings-goal-edit').classList.add('hidden');
}

async function saveGoalEdit() {
  const val = parseFloat(document.getElementById('settings-goal-input').value);
  if (isNaN(val) || val < 50 || val > 999) {
    showError('settings-goal-error', 'Goal must be between 50 and 999 lbs.');
    return;
  }
  if (!activeProfile) { showError('settings-goal-error', 'Profile not loaded.'); return; }

  const updated = { ...activeProfile, goalWeight: val };
  const r = await Bridge.call('updateProfile', updated);
  if (!r.ok) { showError('settings-goal-error', r.data?.detail || r.data); return; }

  activeProfile = r.data;
  updateProfileUI();
  // Re-render settings to reflect new goal
  renderSettings();
}

function setUnits(unit) {
  localStorage.setItem('wlt-units', unit);
  ['lbs','kg'].forEach(u => {
    const btn = document.getElementById(`units-${u}`);
    if (!btn) return;
    btn.style.background = u === unit ? 'var(--color-accent)' : 'var(--color-surface-secondary)';
    btn.style.color = u === unit ? '#fff' : 'var(--color-text-secondary)';
  });
}
```

- [ ] **Step 2: Verify in browser**

Navigate to Settings tab. Confirm:
- 5 accent swatches render; tapping one changes the whole app accent instantly
- Dark mode toggle animates and changes theme
- Goal weight shows current value; Edit reveals inline input; Save calls API and re-renders
- Units segmented control highlights active selection

- [ ] **Step 3: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat: add Settings view — accent swatches, dark toggle, goal edit, units (ux-redesign-v2)"
```

---

## Task 8: Final polish and accessibility pass

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html` — `<html>` tag
- Modify: `WeightLossTracker/wwwroot/js/app.js` — `showError` function

- [ ] **Step 1: Update `showError` to use warm token colours**

Find `function showError(containerId, message)` and replace the template string:

```js
function showError(containerId, message) {
  const el = document.getElementById(containerId);
  if (!el) return;
  el.innerHTML = `
    <div role="alert" class="rounded-xl p-3 flex items-start gap-2 mt-2 mb-2"
         style="background:rgba(185,28,28,0.08);border:1px solid var(--color-feedback-error);color:var(--color-feedback-error);">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none"
           stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
           class="flex-shrink-0 mt-0.5" aria-hidden="true">
        <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <span class="flex-1 text-sm">${escHtml(message)}</span>
      <button onclick="this.closest('[role=alert]').remove()"
              aria-label="Dismiss error"
              class="font-bold text-lg leading-none ml-1"
              style="color:var(--color-feedback-error);">
        &times;
      </button>
    </div>`;
}
```

- [ ] **Step 2: Add `lang` attribute guard and update skip-nav link**

In `index.html`, confirm `<html lang="en"` is present (it already is).

Find the skip navigation `<a>` tag and update its style to use tokens:
```html
<a href="#main-content"
   class="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:px-4 focus:py-2 focus:rounded-lg focus:text-sm focus:font-medium"
   style="background:var(--color-accent);color:#fff;">
  Skip to main content
</a>
```

- [ ] **Step 3: Verify full flow in browser**

Open the app, log in, and check each tab:
1. Dashboard — ring, stats, recent entries
2. Log — date strip, weight input, save, history table
3. Trends — chart renders, period tabs work, no console errors
4. Settings — accent swap, dark toggle, goal inline edit, units toggle
5. Switch to dark mode — warm brown-black base, no cold grey surfaces
6. Switch accent to Rose — all headers, ring, and buttons update

- [ ] **Step 4: Final commit**

```bash
git add WeightLossTracker/wwwroot/index.html WeightLossTracker/wwwroot/js/app.js
git commit -m "style: accessibility polish, warm error messages, skip-nav token update (ux-redesign-v2)"
```

---

## Self-Review Notes

**Spec coverage check:**

| Spec requirement | Task |
|---|---|
| Warm off-white base + warm dark base | Task 1 |
| 5 warm accent presets, instant swap | Task 3 |
| `wlt-accent` / `wlt-theme` localStorage | Task 3 |
| Emoji bottom nav, 4 tabs | Task 2 |
| Desktop sidebar 4 primary + 3 secondary | Task 2 |
| SVG progress ring, `role="progressbar"` | Task 4 |
| Time-aware greeting | Task 4 |
| Stat cards (current, lost, goal) | Task 4 |
| Recent entries list | Task 4 |
| Violet header on Log | Task 5 |
| 7-day date strip | Task 5 |
| ±0.1 weight buttons, 44×44px | Task 5 |
| Inline validation on blur | Task 5 |
| Amber gradient header on Trends | Task 6 |
| Period tabs (1W/1M/3M/All) | Task 6 |
| Chart.js amber line + fill gradient | Task 6 |
| Avg + best week stat cards | Task 6 |
| Accent swatch picker | Task 7 |
| Dark mode toggle (pill, `role="switch"`) | Task 7 |
| Goal inline edit (no modal) | Task 7 |
| Units segmented control | Task 7 |
| `prefers-reduced-motion` | Task 1 (existing rule retained) |
| Emoji nav `aria-hidden`, label accessible name | Task 2 |
| Accent swatch `aria-label` + `aria-pressed` | Task 7 |
| Warm error messages | Task 8 |
