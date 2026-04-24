# UX v3 Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the indigo-heavy color palette with a Dual-Mode Hybrid green/teal design system across `index.html` and `app.js` without touching any backend or JS logic.

**Architecture:** Two agents work in parallel — Agent A owns all `index.html` changes (Tasks 1–6), Agent B owns all `app.js` changes (Tasks 7–10). Tasks within each agent must run sequentially; the two agents are fully independent since they edit different files.

**Tech Stack:** Vanilla JS, Tailwind CSS v3 CDN (class-based dark mode via `.dark`), CSS custom properties (design tokens), Chart.js

---

## Parallelization Map

```
Phase 1 ─── Agent A: Tasks 1 → 2 → 3 → 4 → 5 → 6   (index.html)
         └── Agent B: Tasks 7 → 8 → 9 → 10            (app.js)
```

Both agents start simultaneously. No cross-file dependencies.

---

## Files Modified

| File | Tasks |
|---|---|
| `WeightLossTracker/wwwroot/index.html` | 1–6 |
| `WeightLossTracker/wwwroot/js/app.js` | 7–10 |

---

## Task 1: Replace CSS Design Tokens

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:22-86`

Replace the entire token block (`:root`, `@media prefers-color-scheme: dark`, `[data-theme="dark"]`, `[data-theme="light"]`) with the new Dual-Mode Hybrid palette. The existing spacing and radius variables are preserved unchanged.

- [ ] **Step 1: Replace the `:root` block**

In `index.html`, find and replace the `:root { ... }` block (lines 22–45) with:

```css
    :root {
      --color-surface-primary:   #FFFFFF;
      --color-surface-secondary: #F0FDF4;
      --color-surface-card:      #FFFFFF;
      --color-surface-sidebar:   #F0FDF4;
      --color-border-default:    #BBF7D0;
      --color-border-strong:     #86EFAC;
      --color-border-subtle:     #E5E7EB;
      --color-row-alt:           #F9FAFB;
      --color-text-primary:      #111827;
      --color-text-secondary:    #6B7280;
      --color-text-disabled:     #9CA3AF;
      --color-text-inverted:     #FFFFFF;
      --color-accent:            #16A34A;
      --color-accent-hover:      #15803D;
      --color-accent-subtle:     #DCFCE7;
      --color-accent-text:       #166534;
      --color-nav-active-bg:     #DCFCE7;
      --color-nav-active-border: #16A34A;
      --color-nav-active-text:   #166534;
      --color-feedback-error:    #B71C1C;
      --color-feedback-success:  #166534;
      --color-feedback-warning:  #8A6D00;

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
```

- [ ] **Step 2: Replace the `@media (prefers-color-scheme: dark)` block**

Find and replace lines 47–60:

```css
    @media (prefers-color-scheme: dark) {
      :root {
        --color-surface-primary:   #0F172A;
        --color-surface-secondary: #1E293B;
        --color-surface-card:      #1E293B;
        --color-surface-sidebar:   #0F172A;
        --color-border-default:    #334155;
        --color-border-strong:     #475569;
        --color-border-subtle:     #334155;
        --color-row-alt:           #172033;
        --color-text-primary:      #F1F5F9;
        --color-text-secondary:    #94A3B8;
        --color-text-disabled:     #475569;
        --color-text-inverted:     #0F172A;
        --color-accent:            #0D9488;
        --color-accent-hover:      #0F766E;
        --color-accent-subtle:     #134E4A;
        --color-accent-text:       #5EEAD4;
        --color-nav-active-bg:     #134E4A;
        --color-nav-active-border: #0D9488;
        --color-nav-active-text:   #5EEAD4;
        --color-feedback-error:    #FCA5A5;
        --color-feedback-success:  #6EE7B7;
        --color-feedback-warning:  #FDE68A;
      }
    }
```

- [ ] **Step 3: Replace the `[data-theme="dark"]` block**

Find and replace lines 62–73:

```css
    [data-theme="dark"] {
      --color-surface-primary:   #0F172A;
      --color-surface-secondary: #1E293B;
      --color-surface-card:      #1E293B;
      --color-surface-sidebar:   #0F172A;
      --color-border-default:    #334155;
      --color-border-strong:     #475569;
      --color-border-subtle:     #334155;
      --color-row-alt:           #172033;
      --color-text-primary:      #F1F5F9;
      --color-text-secondary:    #94A3B8;
      --color-text-disabled:     #475569;
      --color-text-inverted:     #0F172A;
      --color-accent:            #0D9488;
      --color-accent-hover:      #0F766E;
      --color-accent-subtle:     #134E4A;
      --color-accent-text:       #5EEAD4;
      --color-nav-active-bg:     #134E4A;
      --color-nav-active-border: #0D9488;
      --color-nav-active-text:   #5EEAD4;
      --color-feedback-error:    #FCA5A5;
      --color-feedback-success:  #6EE7B7;
      --color-feedback-warning:  #FDE68A;
    }
```

- [ ] **Step 4: Replace the `[data-theme="light"]` block**

Find and replace lines 75–86:

```css
    [data-theme="light"] {
      --color-surface-primary:   #FFFFFF;
      --color-surface-secondary: #F0FDF4;
      --color-surface-card:      #FFFFFF;
      --color-surface-sidebar:   #F0FDF4;
      --color-border-default:    #BBF7D0;
      --color-border-strong:     #86EFAC;
      --color-border-subtle:     #E5E7EB;
      --color-row-alt:           #F9FAFB;
      --color-text-primary:      #111827;
      --color-text-secondary:    #6B7280;
      --color-text-disabled:     #9CA3AF;
      --color-text-inverted:     #FFFFFF;
      --color-accent:            #16A34A;
      --color-accent-hover:      #15803D;
      --color-accent-subtle:     #DCFCE7;
      --color-accent-text:       #166534;
      --color-nav-active-bg:     #DCFCE7;
      --color-nav-active-border: #16A34A;
      --color-nav-active-text:   #166534;
      --color-feedback-error:    #B71C1C;
      --color-feedback-success:  #166534;
      --color-feedback-warning:  #8A6D00;
    }
```

- [ ] **Step 5: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat(uxv3): replace CSS design tokens with Dual-Mode Hybrid palette"
```

---

## Task 2: Update Stray Style Rules & Body Class

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:88-127`

Update hardcoded hex/Tailwind values in the inline `<style>` block and the `<body>` opening tag.

- [ ] **Step 1: Update `.mobile-tab-active` rule (line 90)**

Find:
```css
    [data-mobile-nav].mobile-tab-active { border-top-color: #a5b4fc; color: #ffffff; }
```

Replace with:
```css
    [data-mobile-nav].mobile-tab-active { border-top-color: var(--color-accent); color: var(--color-accent); }
```

- [ ] **Step 2: Add `.nav-active` CSS class for sidebar active state**

After the `.mobile-tab-active` line, insert:

```css
    .nav-active {
      background: var(--color-nav-active-bg) !important;
      color: var(--color-nav-active-text) !important;
      border-left: 2px solid var(--color-nav-active-border);
      padding-left: calc(1rem - 2px) !important;
    }
    .nav-item:hover:not(.nav-active) { background: var(--color-accent-subtle); }
```

- [ ] **Step 3: Update scrollbar rules (lines 108–112)**

Find:
```css
    ::-webkit-scrollbar-track { background: #f1f5f9; }
    ::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
    .dark ::-webkit-scrollbar-track { background: #1e293b; }
    .dark ::-webkit-scrollbar-thumb { background: #475569; }
```

Replace with:
```css
    ::-webkit-scrollbar-track { background: var(--color-surface-secondary); }
    ::-webkit-scrollbar-thumb { background: var(--color-border-subtle); border-radius: 4px; }
```

- [ ] **Step 4: Update `.prose h2` and remove `.dark .prose h2` (lines 116–120)**

Find:
```css
    .prose h2 { color: #4f46e5; font-weight: 700; font-size: 1.1rem; margin-top: 1rem; }
    .prose h3 { font-weight: 600; margin-top: 0.75rem; }
    .prose li { margin-bottom: 0.25rem; }
    .prose p  { margin-bottom: 0.5rem; }
    .dark .prose h2 { color: #818cf8; }
```

Replace with:
```css
    .prose h2 { color: var(--color-accent); font-weight: 700; font-size: 1.1rem; margin-top: 1rem; }
    .prose h3 { font-weight: 600; margin-top: 0.75rem; }
    .prose li { margin-bottom: 0.25rem; }
    .prose p  { margin-bottom: 0.5rem; }
```

- [ ] **Step 5: Update skip-nav link focus color (line 127)**

Find:
```html
     class="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:bg-indigo-600 focus:text-white focus:px-4 focus:py-2 focus:rounded-lg focus:text-sm focus:font-medium">
```

Replace with:
```html
     class="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:bg-[var(--color-accent)] focus:text-[var(--color-text-inverted)] focus:px-4 focus:py-2 focus:rounded-lg focus:text-sm focus:font-medium">
```

- [ ] **Step 6: Update `<body>` opening tag (line 123)**

Find:
```html
<body class="h-full bg-gray-100 dark:bg-gray-900 flex flex-col md:flex-row transition-colors duration-200">
```

Replace with:
```html
<body class="h-full bg-[var(--color-surface-primary)] flex flex-col md:flex-row transition-colors duration-200">
```

- [ ] **Step 7: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat(uxv3): update stray style rules, scrollbars, prose, body, skip-nav to tokens"
```

---

## Task 3: Retokenize Mobile Header

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:131-164`

Replace all `indigo-*` classes and the `+` circle SVG in the mobile top header.

- [ ] **Step 1: Replace entire mobile header block**

Find the `<header class="md:hidden ...">` block (lines 131–164) and replace it entirely with:

```html
  <!-- Mobile top header (hidden on md+) -->
  <header class="md:hidden bg-[var(--color-surface-sidebar)] border-b border-[var(--color-border-strong)] px-4 flex items-center gap-3 flex-shrink-0 min-h-[56px]">
    <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24"
         fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
         stroke-linejoin="round" class="text-[var(--color-accent)] flex-shrink-0" aria-hidden="true">
      <path d="m16 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
      <path d="m2 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
      <path d="M7 21h10"/><path d="M12 3v18"/>
      <path d="M3 7h2c2 0 4-1 6-2 2 1 4 2 6 2h2"/>
    </svg>
    <span class="text-[var(--color-text-primary)] font-bold text-base leading-tight">
      Weight Loss <span class="text-[var(--color-accent)] font-normal text-sm">Tracker</span></span>
    <span id="mobile-username" class="text-[var(--color-text-secondary)] text-xs ml-auto mr-2 hidden sm:block"></span>
    <!-- Dark mode toggle for mobile -->
    <button data-theme-toggle onclick="toggleTheme()"
            aria-label="Switch to dark mode"
            class="ml-auto flex items-center justify-center w-11 h-11 rounded-lg text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)] hover:bg-[var(--color-accent-subtle)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]">
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

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat(uxv3): retokenize mobile header, replace + SVG with scale icon"
```

---

## Task 4: Redesign Sidebar Navigation

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:166-306`

Replace indigo classes, new scale SVG logo, add goal progress strip, apply `nav-item` class to all nav buttons.

- [ ] **Step 1: Replace entire sidebar `<nav>` block**

Find the `<!-- Sidebar navigation (desktop only) -->` `<nav>` block (lines 166–306) and replace it entirely with:

```html
  <!-- Sidebar navigation (desktop only) -->
  <nav aria-label="Main navigation"
       class="hidden md:flex md:flex-col w-56 bg-[var(--color-surface-sidebar)] border-r border-[var(--color-border-strong)] py-6 px-3 flex-shrink-0 min-h-screen">

    <!-- App name -->
    <div class="text-[var(--color-text-primary)] font-bold text-lg px-3 mb-8 flex items-center gap-2" aria-hidden="true">
      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" class="text-[var(--color-accent)] flex-shrink-0" aria-hidden="true">
        <path d="m16 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
        <path d="m2 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
        <path d="M7 21h10"/><path d="M12 3v18"/>
        <path d="M3 7h2c2 0 4-1 6-2 2 1 4 2 6 2h2"/>
      </svg>
      <span class="leading-tight">Weight Loss<br>
        <span class="text-[var(--color-accent)] font-normal text-sm">Tracker</span>
      </span>
    </div>

    <!-- User info -->
    <div class="px-3 mb-4">
      <p id="sidebar-username" class="text-[var(--color-text-secondary)] text-xs font-medium truncate"></p>
    </div>

    <!-- Nav links -->
    <div class="space-y-1 flex-1" role="list">
      <button data-nav="dashboard" onclick="navigate('dashboard')" role="listitem"
              aria-label="Dashboard"
              class="nav-item w-full text-left px-4 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 text-[var(--color-text-secondary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:ring-offset-1 min-h-[44px]">
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
          <rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>
        </svg>
        Dashboard
      </button>

      <button data-nav="weight" onclick="navigate('weight')" role="listitem"
              aria-label="Weight log"
              class="nav-item w-full text-left px-4 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 text-[var(--color-text-secondary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:ring-offset-1 min-h-[44px]">
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/>
          <line x1="6" y1="20" x2="6" y2="14"/>
        </svg>
        Weight Log
      </button>

      <button data-nav="exercise" onclick="navigate('exercise')" role="listitem"
              aria-label="Exercise schedule"
              class="nav-item w-full text-left px-4 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 text-[var(--color-text-secondary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:ring-offset-1 min-h-[44px]">
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <path d="M18 8h1a4 4 0 0 1 0 8h-1"/><path d="M2 8h16v9a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8z"/>
          <line x1="6" y1="1" x2="6" y2="4"/><line x1="10" y1="1" x2="10" y2="4"/>
          <line x1="14" y1="1" x2="14" y2="4"/>
        </svg>
        Exercise
      </button>

      <button data-nav="meals" onclick="navigate('meals')" role="listitem"
              aria-label="Meal log"
              class="nav-item w-full text-left px-4 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 text-[var(--color-text-secondary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:ring-offset-1 min-h-[44px]">
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <path d="M3 2v7c0 1.1.9 2 2 2h4a2 2 0 0 0 2-2V2"/>
          <path d="M7 2v20"/><path d="M21 15V2v0a5 5 0 0 0-5 5v6c0 1.1.9 2 2 2h3zm0 0v7"/>
        </svg>
        Meals
      </button>

      <button data-nav="history" onclick="navigate('history')" role="listitem"
              aria-label="AI history"
              class="nav-item w-full text-left px-4 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 text-[var(--color-text-secondary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:ring-offset-1 min-h-[44px]">
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <circle cx="12" cy="12" r="10"/>
          <path d="M12 8v4l3 3"/>
        </svg>
        AI History
      </button>

      <button data-nav="profile" onclick="navigate('profile')" role="listitem"
              aria-label="Profile"
              class="nav-item w-full text-left px-4 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 text-[var(--color-text-secondary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:ring-offset-1 min-h-[44px]">
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
          <circle cx="12" cy="7" r="4"/>
        </svg>
        Profile
      </button>
    </div>

    <!-- Goal progress strip + logout + dark mode toggle -->
    <div class="mt-4 px-3">
      <!-- Goal progress strip (new) -->
      <div id="sidebar-progress" class="mb-3 px-1">
        <p class="progress-label text-[var(--color-text-disabled)] text-xs mb-1">0% to goal</p>
        <div style="background: var(--color-border-subtle); height: 3px; border-radius: 2px;">
          <div class="progress-fill" style="background: var(--color-accent); height: 3px; width: 0%; border-radius: 2px; transition: width 0.5s ease;"></div>
        </div>
      </div>

      <button onclick="handleLogout()"
              class="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-xs text-[var(--color-text-secondary)] hover:text-[var(--color-feedback-error)] hover:bg-[color-mix(in_srgb,var(--color-feedback-error)_10%,transparent)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--color-feedback-error)] min-h-[44px] mb-1">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" aria-hidden="true">
          <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
          <polyline points="16 17 21 12 16 7"/>
          <line x1="21" y1="12" x2="9" y2="12"/>
        </svg>
        Log out
      </button>

      <!-- Dark mode toggle -->
      <button data-theme-toggle onclick="toggleTheme()"
              aria-label="Switch to dark mode"
              title="Toggle dark mode"
              class="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-xs text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)] hover:bg-[var(--color-accent-subtle)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] min-h-[44px]">
        <!-- Moon icon (shown in light mode) -->
        <svg data-moon xmlns="http://www.w3.org/2000/svg" width="16" height="16"
             viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
        </svg>
        <!-- Sun icon (shown in dark mode) -->
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

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat(uxv3): redesign sidebar — tokens, scale SVG, nav-item class, goal progress strip"
```

---

## Task 5: Retokenize Mobile Bottom Tab Bar

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:315-392`

Replace `bg-indigo-900`, `border-indigo-800`, and `text-indigo-400`/`focus:bg-indigo-800` classes with token-based equivalents.

- [ ] **Step 1: Replace the entire mobile `<nav>` tab bar block**

Find the `<!-- Mobile bottom tab bar (hidden on md+) -->` block (lines 315–392) and replace it entirely with:

```html
  <!-- Mobile bottom tab bar (hidden on md+) -->
  <nav aria-label="Mobile navigation"
       class="md:hidden fixed bottom-0 left-0 right-0 bg-[var(--color-surface-sidebar)] border-t border-[var(--color-border-strong)] flex z-50"
       style="padding-bottom: env(safe-area-inset-bottom)">

    <button data-mobile-nav="dashboard" onclick="navigate('dashboard')"
            class="flex-1 flex flex-col items-center justify-center py-2 text-[var(--color-text-secondary)] min-h-[56px] transition-colors focus:outline-none focus:bg-[var(--color-accent-subtle)]"
            aria-label="Dashboard">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
        <rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>
      </svg>
      <span class="text-xs mt-0.5">Dashboard</span>
    </button>

    <button data-mobile-nav="weight" onclick="navigate('weight')"
            class="flex-1 flex flex-col items-center justify-center py-2 text-[var(--color-text-secondary)] min-h-[56px] transition-colors focus:outline-none focus:bg-[var(--color-accent-subtle)]"
            aria-label="Weight log">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/>
        <line x1="6" y1="20" x2="6" y2="14"/>
      </svg>
      <span class="text-xs mt-0.5">Weight</span>
    </button>

    <button data-mobile-nav="exercise" onclick="navigate('exercise')"
            class="flex-1 flex flex-col items-center justify-center py-2 text-[var(--color-text-secondary)] min-h-[56px] transition-colors focus:outline-none focus:bg-[var(--color-accent-subtle)]"
            aria-label="Exercise">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <path d="M18 8h1a4 4 0 0 1 0 8h-1"/><path d="M2 8h16v9a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8z"/>
        <line x1="6" y1="1" x2="6" y2="4"/><line x1="10" y1="1" x2="10" y2="4"/>
        <line x1="14" y1="1" x2="14" y2="4"/>
      </svg>
      <span class="text-xs mt-0.5">Exercise</span>
    </button>

    <button data-mobile-nav="meals" onclick="navigate('meals')"
            class="flex-1 flex flex-col items-center justify-center py-2 text-[var(--color-text-secondary)] min-h-[56px] transition-colors focus:outline-none focus:bg-[var(--color-accent-subtle)]"
            aria-label="Meals">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <path d="M3 2v7c0 1.1.9 2 2 2h4a2 2 0 0 0 2-2V2"/>
        <path d="M7 2v20"/><path d="M21 15V2v0a5 5 0 0 0-5 5v6c0 1.1.9 2 2 2h3zm0 0v7"/>
      </svg>
      <span class="text-xs mt-0.5">Meals</span>
    </button>

    <button data-mobile-nav="history" onclick="navigate('history')"
            class="flex-1 flex flex-col items-center justify-center py-2 text-[var(--color-text-secondary)] min-h-[56px] transition-colors focus:outline-none focus:bg-[var(--color-accent-subtle)]"
            aria-label="AI history">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <circle cx="12" cy="12" r="10"/>
        <path d="M12 8v4l3 3"/>
      </svg>
      <span class="text-xs mt-0.5">History</span>
    </button>

    <button data-mobile-nav="profile" onclick="navigate('profile')"
            class="flex-1 flex flex-col items-center justify-center py-2 text-[var(--color-text-secondary)] min-h-[56px] transition-colors focus:outline-none focus:bg-[var(--color-accent-subtle)]"
            aria-label="Profile">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" aria-hidden="true">
        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
        <circle cx="12" cy="7" r="4"/>
      </svg>
      <span class="text-xs mt-0.5">Profile</span>
    </button>
  </nav>
```

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat(uxv3): retokenize mobile bottom tab bar"
```

---

## Task 6: Update Login View

**Files:**
- Modify: `WeightLossTracker/wwwroot/index.html:394-432`

Update login view per spec §9: outer backdrop, card, logo SVG, heading, labels, inputs, submit button.

- [ ] **Step 1: Replace entire login view block**

Find the `<!-- Login view (shown when unauthenticated) -->` block (lines 394–432) and replace it entirely with:

```html
  <!-- Login view (shown when unauthenticated) -->
  <div id="login-view" class="hidden fixed inset-0 z-50 bg-[var(--color-surface-primary)] flex items-center justify-center p-4">
    <div class="bg-[var(--color-surface-card)] border border-[var(--color-border-subtle)] rounded-xl shadow-lg w-full max-w-sm p-8">
      <div class="flex items-center gap-3 mb-8">
        <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
             stroke-linejoin="round" class="text-[var(--color-accent)]" aria-hidden="true">
          <path d="m16 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
          <path d="m2 16 3-8 3 8c-.87.65-1.92 1-3 1s-2.13-.35-3-1Z"/>
          <path d="M7 21h10"/><path d="M12 3v18"/>
          <path d="M3 7h2c2 0 4-1 6-2 2 1 4 2 6 2h2"/>
        </svg>
        <h1 class="text-xl font-bold text-[var(--color-text-primary)]">Weight Loss Tracker</h1>
      </div>

      <div id="login-error"></div>

      <form id="login-form" class="space-y-4" novalidate>
        <div>
          <label for="login-username" class="block text-sm text-[var(--color-text-secondary)] mb-1">
            Username
          </label>
          <input id="login-username" type="text" autocomplete="username" required
                 class="border border-[var(--color-border-subtle)] rounded-lg px-3 py-2 w-full bg-[var(--color-surface-card)] text-[var(--color-text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] placeholder:text-[var(--color-text-disabled)]"
                 placeholder="your username">
        </div>
        <div>
          <label for="login-password" class="block text-sm text-[var(--color-text-secondary)] mb-1">
            Password
          </label>
          <input id="login-password" type="password" autocomplete="current-password" required
                 class="border border-[var(--color-border-subtle)] rounded-lg px-3 py-2 w-full bg-[var(--color-surface-card)] text-[var(--color-text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] placeholder:text-[var(--color-text-disabled)]"
                 placeholder="••••••••">
        </div>
        <button type="submit" id="login-btn"
                class="bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] px-5 py-2 rounded-lg font-semibold transition-colors w-full min-h-[44px]">
          Log in
        </button>
      </form>
    </div>
  </div>
```

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/index.html
git commit -m "feat(uxv3): update login view to token-based palette, scale SVG logo"
```

---

## Task 7: Update C Constants in app.js

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js:6-25`

Replace all Tailwind class strings in the `C` constants object with token-based equivalents per spec §8.

- [ ] **Step 1: Replace the entire `C` object**

Find the `const C = {` block (lines 6–25) and replace it entirely with:

```js
const C = {
  card:        'bg-[var(--color-surface-card)] border border-[var(--color-border-subtle)] rounded-xl shadow-sm p-5',
  h1:          'text-2xl font-bold text-[var(--color-text-primary)]',
  h2:          'text-lg font-semibold text-[var(--color-text-secondary)] mb-4',
  h3:          'font-semibold text-[var(--color-text-primary)]',
  label:       'block text-sm text-[var(--color-text-secondary)] mb-1',
  bodyText:    'text-sm text-[var(--color-text-primary)]',
  mutedText:   'text-sm text-[var(--color-text-secondary)]',
  tinyText:    'text-xs text-[var(--color-text-disabled)]',
  input:       'border border-[var(--color-border-subtle)] rounded-lg px-3 py-2 w-full bg-[var(--color-surface-card)] text-[var(--color-text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:border-transparent placeholder:text-[var(--color-text-disabled)]',
  inputSm:     'border border-[var(--color-border-subtle)] rounded px-2 py-1 bg-[var(--color-surface-card)] text-[var(--color-text-primary)] text-sm focus:outline-none focus:ring-1 focus:ring-[var(--color-accent)] focus:border-transparent',
  select:      'border border-[var(--color-border-subtle)] rounded-lg px-3 py-2 w-full bg-[var(--color-surface-card)] text-[var(--color-text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)] focus:border-transparent',
  btnPrimary:  'bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] px-5 py-2 rounded-lg font-semibold transition-colors min-h-[44px]',
  btnSuccess:  'bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] py-2 rounded-lg font-semibold transition-colors min-h-[44px]',
  btnSecondary:'bg-[var(--color-accent-subtle)] text-[var(--color-accent-text)] border border-[var(--color-accent)] hover:bg-[var(--color-accent)] hover:text-[var(--color-text-inverted)] px-4 py-2 rounded-lg text-sm font-semibold transition-colors',
  btnSmPrimary:'text-xs bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] rounded px-2 py-1 transition-colors',
  trow:        'border-b border-[var(--color-border-subtle)] last:border-0 hover:bg-[var(--color-accent-subtle)]',
  divider:     'border-b border-[var(--color-border-subtle)]',
  badge:       'text-xs font-semibold px-2 py-0.5 rounded-full',
};
```

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat(uxv3): update C constants to token-based Tailwind classes"
```

---

## Task 8: Update navigate(), updateProfileUI(), and add lastCurrentWeight global

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js:27-30` (globals)
- Modify: `WeightLossTracker/wwwroot/js/app.js:162-176` (updateProfileUI)
- Modify: `WeightLossTracker/wwwroot/js/app.js:225-241` (navigate active state + loading spinner)

- [ ] **Step 1: Add `lastCurrentWeight` global (after line 28)**

Find:
```js
let activeChart = null;
let activeProfile = null;
let currentUser = null;
let currentView = 'dashboard';
```

Replace with:
```js
let activeChart = null;
let activeProfile = null;
let currentUser = null;
let currentView = 'dashboard';
let lastCurrentWeight = null;
```

- [ ] **Step 2: Update `updateProfileUI` function (lines 162–176)**

Find:
```js
function updateProfileUI() {
  if (!activeProfile || !currentUser) return;

  const goalText = `Goal: ${activeProfile.startingWeight} → ${activeProfile.goalWeight} lbs`;
  const lossText = `${Math.round(activeProfile.startingWeight - activeProfile.goalWeight)} lbs to lose`;

  const sidebarGoal = document.getElementById('sidebar-goal-text');
  if (sidebarGoal) sidebarGoal.innerHTML = `${escHtml(goalText)}<br><span class="text-indigo-500">${escHtml(lossText)}</span>`;

  const sidebarUsername = document.getElementById('sidebar-username');
  if (sidebarUsername) sidebarUsername.textContent = currentUser.username;

  const mobileUsername = document.getElementById('mobile-username');
  if (mobileUsername) mobileUsername.textContent = currentUser.username;
}
```

Replace with:
```js
function updateProfileUI() {
  if (!activeProfile || !currentUser) return;

  const sw = activeProfile.startingWeight ?? 0;
  const gw = activeProfile.goalWeight ?? 0;
  const cw = lastCurrentWeight ?? sw;
  const pct = sw === gw ? 0 : Math.min(100, Math.max(0, ((sw - cw) / (sw - gw)) * 100));

  const progress = document.getElementById('sidebar-progress');
  if (progress) {
    const fill = progress.querySelector('.progress-fill');
    const label = progress.querySelector('.progress-label');
    if (fill) fill.style.width = `${pct.toFixed(1)}%`;
    if (label) label.textContent = `${Math.round(pct)}% to goal`;
  }

  const sidebarUsername = document.getElementById('sidebar-username');
  if (sidebarUsername) sidebarUsername.textContent = currentUser.username;

  const mobileUsername = document.getElementById('mobile-username');
  if (mobileUsername) mobileUsername.textContent = currentUser.username;
}
```

- [ ] **Step 3: Update `navigate()` active state and loading spinner (lines 225–241)**

Find:
```js
  // Desktop sidebar active state
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
  // Desktop sidebar active state
  document.querySelectorAll('[data-nav]').forEach(el => {
    const active = el.dataset.nav === viewName;
    el.classList.toggle('nav-active', active);
  });
```

- [ ] **Step 4: Update loading spinner in `navigate()` (line 240)**

Find:
```js
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div>
```

Replace with:
```js
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-[var(--color-accent)]"></div>
```

- [ ] **Step 5: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat(uxv3): update navigate() active state, loading spinner, sidebar progress strip"
```

---

## Task 9: Redesign Dashboard Renderer & Chart.js

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js:255-371`

Replace the dashboard renderer with the new 3-card stats banner, redesigned progress bar, bar chart with opacity gradient, and quick-log panel per spec §5.

- [ ] **Step 1: Replace the `renderDashboard` function (lines 255–352)**

Find:
```js
async function renderDashboard() {
```

Find the entire function through its closing `}` (line 352) and replace it with:

```js
async function renderDashboard() {
  const root = document.getElementById('view-root');
  const r = await Bridge.call('getDashboard');
  if (!r.ok) {
    root.innerHTML = `<p class="text-[var(--color-feedback-error)] p-4">
      Failed to load dashboard: ${escHtml(r.data?.detail || r.data)}</p>`;
    return;
  }
  const d = r.data;

  lastCurrentWeight = d.currentWeight;
  updateProfileUI();

  const cw = d.currentWeight != null ? d.currentWeight.toFixed(1) : '—';
  const lost = d.lostSoFar != null ? d.lostSoFar.toFixed(1) : '—';
  const toGo = d.toGoal != null ? d.toGoal.toFixed(1) : '—';

  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="${C.h1}">Dashboard</h1>

      <!-- Stats banner: 3 equal-width cards -->
      <div class="grid grid-cols-3 gap-4" role="list" aria-label="Key metrics">
        <div class="${C.card} text-center dark:border-0" role="listitem">
          <div class="text-[1.5rem] font-extrabold text-[var(--color-text-primary)] leading-tight">
            ${cw}<sup class="text-sm font-normal text-[var(--color-text-secondary)]">lbs</sup>
          </div>
          <div class="text-[0.6rem] uppercase tracking-[0.8px] text-[var(--color-text-disabled)] mt-1">CURRENT</div>
        </div>
        <div class="${C.card} text-center dark:border-0" role="listitem">
          <div class="text-[1.5rem] font-extrabold text-[var(--color-accent)] leading-tight">
            −${lost}<sup class="text-sm font-normal text-[var(--color-text-secondary)]">lbs</sup>
          </div>
          <div class="text-[0.6rem] uppercase tracking-[0.8px] text-[var(--color-text-disabled)] mt-1">LOST</div>
        </div>
        <div class="${C.card} text-center dark:border-0" role="listitem">
          <div class="text-[1.5rem] font-extrabold text-[var(--color-text-primary)] leading-tight">
            ${toGo}<sup class="text-sm font-normal text-[var(--color-text-secondary)]">lbs</sup>
          </div>
          <div class="text-[0.6rem] uppercase tracking-[0.8px] text-[var(--color-text-disabled)] mt-1">TO GO</div>
        </div>
      </div>

      <!-- Progress bar -->
      <div class="${C.card}">
        <div class="flex justify-between text-xs mb-2">
          <span class="text-[var(--color-text-secondary)]">${d.startingWeight} lbs</span>
          <span class="font-semibold text-[var(--color-accent)]">${d.progressPct}%</span>
          <span class="text-[var(--color-text-secondary)]">${d.goalWeight} lbs</span>
        </div>
        <div class="w-full rounded-full overflow-hidden"
             style="background: var(--color-accent-subtle); height: 6px;"
             role="progressbar" aria-valuenow="${d.progressPct}" aria-valuemin="0" aria-valuemax="100"
             aria-label="Weight loss progress">
          <div class="rounded-full transition-all duration-700"
               style="width:${d.progressPct}%; height: 6px; background: linear-gradient(90deg, var(--color-accent), #22c55e)"></div>
        </div>
      </div>

      <!-- Chart + Quick-log panel -->
      <div class="grid grid-cols-1 md:grid-cols-[1fr_160px] gap-4 items-start">
        <div class="${C.card}">
          <h2 class="${C.h2}">Weight trend</h2>
          ${d.chart.labels.length === 0
            ? `<p class="text-[var(--color-text-disabled)] text-sm text-center py-8">
                 No weight entries yet. Log your first weight to see the chart.
               </p>`
            : '<canvas id="weight-chart" height="120"></canvas>'}
        </div>

        <!-- Quick-log panel -->
        <div class="${C.card} flex flex-col gap-3">
          <h2 class="text-sm font-semibold text-[var(--color-text-secondary)]">Quick Log</h2>
          <button onclick="navigate('weight')"
                  class="bg-[var(--color-accent)] hover:bg-[var(--color-accent-hover)] text-[var(--color-text-inverted)] px-4 py-2.5 rounded-lg font-semibold transition-colors min-h-[44px] text-sm w-full">
            ＋ Weight
          </button>
          <button onclick="navigate('meals')"
                  class="bg-[var(--color-accent-subtle)] text-[var(--color-accent-text)] border border-[var(--color-accent)] hover:bg-[var(--color-accent)] hover:text-[var(--color-text-inverted)] px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors min-h-[44px] w-full">
            ＋ Meal
          </button>
        </div>
      </div>
    </div>`;

  if (d.chart.labels.length > 0) {
    const labels  = d.chart.labels.slice(-7);
    const weights = d.chart.weights.slice(-7);
    const isDark  = document.documentElement.classList.contains('dark');
    const gridColor = isDark ? 'rgba(255,255,255,0.07)' : 'rgba(0,0,0,0.06)';
    const tickColor = isDark ? getComputedStyle(document.documentElement).getPropertyValue('--color-text-secondary').trim()
                              : getComputedStyle(document.documentElement).getPropertyValue('--color-text-secondary').trim();

    const accentHex = getComputedStyle(document.documentElement).getPropertyValue('--color-accent').trim();
    const hexMatch  = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(accentHex);
    const rgb = hexMatch
      ? { r: parseInt(hexMatch[1], 16), g: parseInt(hexMatch[2], 16), b: parseInt(hexMatch[3], 16) }
      : { r: 22, g: 163, b: 74 };
    const n = labels.length;
    const bgColors = labels.map((_, i) => {
      const alpha = (0.3 + 0.7 * (i / Math.max(n - 1, 1))).toFixed(2);
      return `rgba(${rgb.r},${rgb.g},${rgb.b},${alpha})`;
    });

    const ctx = document.getElementById('weight-chart').getContext('2d');
    activeChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Weight (lbs)',
          data: weights,
          backgroundColor: bgColors,
          borderColor: 'transparent',
          borderRadius: 4,
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: false }
        },
        scales: {
          x: {
            ticks: { color: tickColor, font: { size: 11 } },
            grid:  { color: gridColor, lineWidth: 0.5 }
          },
          y: {
            ticks: { color: tickColor, font: { size: 11 } },
            grid:  { color: gridColor, lineWidth: 0.5 },
            title: { display: true, text: 'lbs', color: tickColor }
          }
        },
        backgroundColor: 'transparent'
      }
    });
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat(uxv3): redesign dashboard — 3-card banner, gradient progress bar, bar chart, quick-log panel"
```

---

## Task 10: Update md() Renderer

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js:33-45`

Replace hardcoded `text-indigo-*` and `text-gray-*` classes in the markdown renderer with token-based equivalents.

- [ ] **Step 1: Replace the `md()` function body (lines 33–45)**

Find:
```js
  return text
    .replace(/^### (.+)$/gm, '<h3 class="text-base font-semibold mt-3 mb-1 text-gray-800 dark:text-gray-100">$1</h3>')
    .replace(/^## (.+)$/gm,  '<h2 class="text-lg font-bold mt-4 mb-2 text-indigo-700 dark:text-indigo-400">$1</h2>')
    .replace(/^# (.+)$/gm,   '<h1 class="text-xl font-bold mt-4 mb-2 text-gray-800 dark:text-gray-100">$1</h1>')
    .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.+?)\*/g,     '<em>$1</em>')
    .replace(/^- (.+)$/gm,    '<li class="ml-4 list-disc text-gray-700 dark:text-gray-200">$1</li>')
    .replace(/(<li[\s\S]*?<\/li>)/g, '<ul class="my-1">$1</ul>')
    .replace(/\n{2,}/g, '</p><p class="mt-2 text-gray-700 dark:text-gray-200">')
    .replace(/^(?!<[hul])(.+)$/gm, '<p class="mt-1 text-gray-700 dark:text-gray-200">$1</p>');
```

Replace with:
```js
  return text
    .replace(/^### (.+)$/gm, '<h3 class="text-base font-semibold mt-3 mb-1 text-[var(--color-text-primary)]">$1</h3>')
    .replace(/^## (.+)$/gm,  '<h2 class="text-lg font-bold mt-4 mb-2 text-[var(--color-accent)]">$1</h2>')
    .replace(/^# (.+)$/gm,   '<h1 class="text-xl font-bold mt-4 mb-2 text-[var(--color-text-primary)]">$1</h1>')
    .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.+?)\*/g,     '<em>$1</em>')
    .replace(/^- (.+)$/gm,    '<li class="ml-4 list-disc text-[var(--color-text-primary)]">$1</li>')
    .replace(/(<li[\s\S]*?<\/li>)/g, '<ul class="my-1">$1</ul>')
    .replace(/\n{2,}/g, '</p><p class="mt-2 text-[var(--color-text-primary)]">')
    .replace(/^(?!<[hul])(.+)$/gm, '<p class="mt-1 text-[var(--color-text-primary)]">$1</p>');
```

- [ ] **Step 2: Commit**

```bash
git add WeightLossTracker/wwwroot/js/app.js
git commit -m "feat(uxv3): update md() renderer to token-based text colors"
```

---

## Verification Checklist

After both agents complete, run the following checks:

- [ ] **Start the dev server**

```bash
cd WeightLossTracker
dotnet run
```

Expected: Server starts on `http://localhost:5256`

- [ ] **Light mode checks (open in browser)**

1. Log in — login card has green border, scale logo visible in accent color
2. Sidebar: green-50 background, scale icon in green, nav hover shows green-100 tint, active item has left border + green-100 bg
3. Goal progress strip visible above logout
4. Dashboard: 3 stat cards (Current / Lost / To Go), gradient progress bar, bar chart, quick-log buttons
5. Chart bars use opacity gradient (oldest = 30%, newest = 100%)
6. No indigo color visible anywhere on screen

- [ ] **Dark mode checks (toggle dark mode)**

1. Sidebar: slate-950 background, teal accent
2. Dashboard cards: no border (dark mode `dark:border-0`)
3. Chart bar color is teal (reads `--color-accent: #0D9488` in dark)
4. Mobile tab bar: teal active indicator

- [ ] **Browser console**

Open DevTools → Console. No JS errors. No `undefined` values for `lastCurrentWeight`.

- [ ] **Final commit**

```bash
git add .
git commit -m "feat(uxv3): complete Dual-Mode Hybrid redesign — green/teal palette, new dashboard, token system"
```

---

## Spec Coverage Self-Review

| Spec Section | Covered By |
|---|---|
| §3 Color Tokens | Task 1 |
| §4 Sidebar (bg, border, SVG, nav active, goal strip, toggle) | Tasks 2, 4, 8 |
| §5 Dashboard (3-card banner, progress bar, bar chart, quick-log) | Task 9 |
| §6 Components (buttons, inputs, cards via C constants) | Task 7 |
| §7 Mobile tab bar | Task 5 |
| §8 C constants, typography tokens | Task 7 |
| §9 Login view | Task 6 |
| §10 Stray style rules (mobile-tab, prose, scrollbar, skip-nav) | Task 2 |
| §11 What's not changing | Not touched |
