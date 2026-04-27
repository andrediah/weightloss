# Weight Tab Trend Chart & UX Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a line chart to the Weight tab and align its structure and CSS tokens with the Blood Pressure tab so both vitals tabs look and feel identical.

**Architecture:** All changes are in `WeightLossTracker/wwwroot/js/app.js`. Restructure `renderWeightContent` to the BP tab's 3-card layout (form → chart → history), add `renderWeightChart` mirroring `renderBpChart`, extract table rendering into `renderWeightTable`, rename `loadWeightTable` → `loadWeightData`, and replace hardcoded `text-gray-*` classes in `weightRow` and the table header with CSS variable equivalents.

**Tech Stack:** Vanilla JS / Chart.js / CSS custom properties (no build step — edit the file directly)

---

## Files

| Action | Path | Change |
|---|---|---|
| Modify | `WeightLossTracker/wwwroot/js/app.js` | All changes — see tasks below |

---

### Task 1: Restructure weight tab to match BP tab layout

**Files:**
- Modify: `WeightLossTracker/wwwroot/js/app.js`

The current `renderWeightContent` (lines 457–508) has 2 cards (form + history). The BP tab has 3 cards (form + chart + history). This task restructures the weight tab to match, wires in chart rendering, and fixes all CSS token uses.

**Current state of the relevant functions (for reference):**

`renderWeightContent` currently (lines 457–508):
```javascript
async function renderWeightContent(container) {
  container.innerHTML = `
    <div class="space-y-4">
      <h2 class="${C.h2}">Weight log</h2>

      <div class="${C.card}">
        <h2 class="${C.h2}">Log today's weight</h2>
        <div id="weight-error"></div>
        <form id="weight-form" class="flex flex-wrap gap-3 items-end" novalidate>
          <div>
            <label for="wt-weight" class="${C.label}">Weight (lbs)</label>
            <input id="wt-weight" type="number" step="0.1" min="50" max="500" required
                   aria-describedby="wt-weight-hint"
                   class="${C.input} w-32" placeholder="e.g. 212.5">
            <p id="wt-weight-hint" class="text-xs text-gray-400 dark:text-gray-500 mt-1">50–500 lbs</p>
          </div>
          <div class="flex-1 min-w-48">
            <label for="wt-notes" class="${C.label}">Notes (optional)</label>
            <input id="wt-notes" type="text"
                   class="${C.input}" placeholder="e.g. After workout">
          </div>
          <button type="submit" class="${C.btnPrimary} px-5">Save weight</button>
        </form>
      </div>

      <div class="${C.card}">
        <h2 class="${C.h2}">History</h2>
        <div id="weight-table-wrap"></div>
      </div>
    </div>`;

  await loadWeightTable();

  document.getElementById('weight-form').addEventListener('submit', async e => {
    e.preventDefault();
    clearError('weight-error');
    const weight = parseFloat(document.getElementById('wt-weight').value);
    const notes  = document.getElementById('wt-notes').value.trim() || null;

    if (isNaN(weight) || weight < 50 || weight > 500) {
      showError('weight-error', 'Weight must be between 50 and 500 lbs.');
      document.getElementById('wt-weight').focus();
      return;
    }

    const r = await Bridge.call('saveWeight', { weight, notes });
    if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
    document.getElementById('wt-weight').value = '';
    document.getElementById('wt-notes').value  = '';
    await loadWeightTable();
  });
}
```

`loadWeightTable` currently (lines 748–780):
```javascript
async function loadWeightTable() {
  const wrap = document.getElementById('weight-table-wrap');
  if (!wrap) return;
  const r = await Bridge.call('getWeightEntries');
  if (!r.ok) {
    wrap.innerHTML = `<p class="text-red-500 dark:text-red-400 text-sm">Failed to load entries.</p>`;
    return;
  }
  const entries = r.data;
  if (!entries.length) {
    wrap.innerHTML = `<p class="${C.mutedText}">No entries yet. Log your first weight above.</p>`;
    return;
  }

  wrap.innerHTML = `
    <div class="overflow-x-auto">
      <table class="w-full text-sm" aria-label="Weight history">
        <thead>
          <tr class="${C.divider} text-left">
            <th class="py-2 pr-4 text-gray-500 dark:text-gray-400 font-medium">Date</th>
            <th class="py-2 pr-4 text-gray-500 dark:text-gray-400 font-medium">Weight</th>
            <th class="py-2 flex-1 text-gray-500 dark:text-gray-400 font-medium">Notes</th>
            <th class="py-2 text-gray-500 dark:text-gray-400 font-medium">
              <span class="sr-only">Actions</span>
            </th>
          </tr>
        </thead>
        <tbody id="weight-tbody">
          ${entries.map(e => weightRow(e)).join('')}
        </tbody>
      </table>
    </div>`;
}
```

`weightRow` currently (lines 782–803):
```javascript
function weightRow(e) {
  return `
    <tr id="row-${e.id}" class="${C.trow}">
      <td class="py-2 pr-4 text-gray-600 dark:text-gray-300">${fmtDate(e.date)}</td>
      <td class="py-2 pr-4 font-medium text-gray-800 dark:text-gray-100">${e.weight.toFixed(1)}</td>
      <td class="py-2 text-gray-500 dark:text-gray-400">${escHtml(e.notes || '')}</td>
      <td class="py-2">
        <div class="flex gap-3">
          <button onclick="startEditWeight(${e.id}, ${e.weight}, '${escHtml(e.notes||'')}')"
                  aria-label="Edit weight entry for ${fmtDate(e.date)}"
                  class="text-indigo-500 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-200 text-xs font-medium">
            Edit
          </button>
          <button onclick="deleteWeight(${e.id})"
                  aria-label="Delete weight entry for ${fmtDate(e.date)}"
                  class="text-red-400 dark:text-red-500 hover:text-red-600 dark:hover:text-red-300 text-xs font-medium">
            Delete
          </button>
        </div>
      </td>
    </tr>`;
}
```

`cancelEditWeight`, `saveEditWeight`, `deleteWeight` all call `loadWeightTable()` (lines 834, 844, 851).

---

- [ ] **Step 1: Replace `renderWeightContent` with the 3-card version**

  Open `WeightLossTracker/wwwroot/js/app.js`. Replace the entire `renderWeightContent` function (lines 457–508) with:

  ```javascript
  async function renderWeightContent(container) {
    container.innerHTML = `
      <div class="space-y-4">
        <h2 class="${C.h2}">Weight</h2>

        <div class="${C.card}">
          <h3 class="${C.h3} mb-3">Log today's weight</h3>
          <div id="weight-error"></div>
          <form id="weight-form" class="flex flex-wrap gap-3 items-end" novalidate>
            <div>
              <label for="wt-weight" class="${C.label}">Weight (lbs)</label>
              <input id="wt-weight" type="number" step="0.1" min="50" max="500" required
                     aria-describedby="wt-weight-hint"
                     class="${C.input} w-32" placeholder="e.g. 212.5">
              <p id="wt-weight-hint" class="text-xs text-[var(--color-text-disabled)] mt-1">50–500 lbs</p>
            </div>
            <div class="flex-1 min-w-48">
              <label for="wt-notes" class="${C.label}">Notes (optional)</label>
              <input id="wt-notes" type="text"
                     class="${C.input}" placeholder="e.g. After workout">
            </div>
            <button type="submit" class="${C.btnPrimary} px-5">Save weight</button>
          </form>
        </div>

        <div class="${C.card}">
          <h3 class="${C.h3} mb-3">Trend</h3>
          <div id="weight-chart-inner"></div>
        </div>

        <div class="${C.card}">
          <h3 class="${C.h3} mb-3">History</h3>
          <div id="weight-table-wrap"></div>
        </div>
      </div>`;

    await loadWeightData();

    document.getElementById('weight-form').addEventListener('submit', async e => {
      e.preventDefault();
      clearError('weight-error');
      const weight = parseFloat(document.getElementById('wt-weight').value);
      const notes  = document.getElementById('wt-notes').value.trim() || null;

      if (isNaN(weight) || weight < 50 || weight > 500) {
        showError('weight-error', 'Weight must be between 50 and 500 lbs.');
        document.getElementById('wt-weight').focus();
        return;
      }

      const r = await Bridge.call('saveWeight', { weight, notes });
      if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
      document.getElementById('wt-weight').value = '';
      document.getElementById('wt-notes').value  = '';
      await loadWeightData();
    });
  }
  ```

- [ ] **Step 2: Add `renderWeightChart` after `renderBpChart` (after line 681)**

  Insert the following new function immediately after the closing `}` of `renderBpChart` (currently around line 681, before `function renderBpTable`):

  ```javascript
  function renderWeightChart(entries) {
    const wrap = document.getElementById('weight-chart-inner');
    if (!wrap) return;

    if (!entries.length) {
      wrap.innerHTML = `<p class="${C.mutedText} text-center py-6">No entries yet. Log your first weight above.</p>`;
      return;
    }

    const slice   = entries.slice(0, 14).reverse();
    const labels  = slice.map(e => fmtDate(e.date));
    const weights = slice.map(e => e.weight);

    wrap.innerHTML = '<canvas id="weight-chart-tab" height="100"></canvas>';
    if (activeChart) { activeChart.destroy(); activeChart = null; }

    const isDark    = document.documentElement.classList.contains('dark');
    const gridColor = isDark ? 'rgba(255,255,255,0.07)' : 'rgba(0,0,0,0.06)';
    const tickColor = getComputedStyle(document.documentElement).getPropertyValue('--color-text-secondary').trim();
    const accent    = getComputedStyle(document.documentElement).getPropertyValue('--color-accent').trim();

    activeChart = new Chart(document.getElementById('weight-chart-tab').getContext('2d'), {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: 'Weight (lbs)',
          data: weights,
          borderColor: accent,
          backgroundColor: 'transparent',
          tension: 0.3,
          pointRadius: 3,
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: false }
        },
        scales: {
          x: { ticks: { color: tickColor, font: { size: 11 } }, grid: { color: gridColor, lineWidth: 0.5 } },
          y: { ticks: { color: tickColor, font: { size: 11 } }, grid: { color: gridColor, lineWidth: 0.5 },
               title: { display: true, text: 'lbs', color: tickColor } }
        },
        backgroundColor: 'transparent'
      }
    });
  }
  ```

- [ ] **Step 3: Replace `loadWeightTable` with `loadWeightData` + `renderWeightTable`**

  Replace the entire `loadWeightTable` function (lines 748–780) with two functions:

  ```javascript
  async function loadWeightData() {
    const r = await Bridge.call('getWeightEntries');
    if (!r.ok) {
      const wrap = document.getElementById('weight-table-wrap');
      if (wrap) wrap.innerHTML = `<p class="${C.mutedText} text-[var(--color-feedback-error)]">Failed to load entries.</p>`;
      return;
    }
    const entries = r.data;
    renderWeightChart(entries);
    renderWeightTable(entries);
  }

  function renderWeightTable(entries) {
    const wrap = document.getElementById('weight-table-wrap');
    if (!wrap) return;

    if (!entries.length) {
      wrap.innerHTML = `<p class="${C.mutedText}">No entries yet. Log your first weight above.</p>`;
      return;
    }

    wrap.innerHTML = `
      <div class="overflow-x-auto">
        <table class="w-full text-sm" aria-label="Weight history">
          <thead>
            <tr class="${C.divider} text-left">
              <th class="py-2 pr-4 ${C.tinyText} font-medium">Date</th>
              <th class="py-2 pr-4 ${C.tinyText} font-medium">Weight</th>
              <th class="py-2 flex-1 ${C.tinyText} font-medium">Notes</th>
              <th class="py-2"><span class="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody id="weight-tbody">
            ${entries.map(e => weightRow(e)).join('')}
          </tbody>
        </table>
      </div>`;
  }
  ```

- [ ] **Step 4: Update `weightRow` to use CSS variable tokens**

  Replace the entire `weightRow` function (lines 782–803) with:

  ```javascript
  function weightRow(e) {
    return `
      <tr id="row-${e.id}" class="${C.trow}">
        <td class="py-2 pr-4 text-[var(--color-text-secondary)]">${fmtDate(e.date)}</td>
        <td class="py-2 pr-4 font-medium text-[var(--color-text-primary)]">${e.weight.toFixed(1)}</td>
        <td class="py-2 text-[var(--color-text-disabled)]">${escHtml(e.notes || '')}</td>
        <td class="py-2">
          <div class="flex gap-3">
            <button onclick="startEditWeight(${e.id}, ${e.weight}, '${escHtml(e.notes||'')}')"
                    aria-label="Edit weight entry for ${fmtDate(e.date)}"
                    class="text-indigo-500 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-200 text-xs font-medium">
              Edit
            </button>
            <button onclick="deleteWeight(${e.id})"
                    aria-label="Delete weight entry for ${fmtDate(e.date)}"
                    class="text-red-400 dark:text-red-500 hover:text-red-600 dark:hover:text-red-300 text-xs font-medium">
              Delete
            </button>
          </div>
        </td>
      </tr>`;
  }
  ```

- [ ] **Step 5: Update `cancelEditWeight`, `saveEditWeight`, and `deleteWeight` to call `loadWeightData`**

  Find these three occurrences of `loadWeightTable()` after line 805 and replace each with `loadWeightData()`:

  - `cancelEditWeight` (line ~834): `loadWeightTable()` → `loadWeightData()`
  - `saveEditWeight` (line ~844): `await loadWeightTable()` → `await loadWeightData()`
  - `deleteWeight` (line ~851): `await loadWeightTable()` → `await loadWeightData()`

- [ ] **Step 6: Build and verify the app runs**

  ```bash
  dotnet build WeightLossTracker/
  ```

  Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 7: Run the test suite**

  ```bash
  dotnet test WeightLossTracker.Tests/
  ```

  Expected: `Passed! - Failed: 0, Passed: 10, Skipped: 0`

- [ ] **Step 8: Manual smoke test**

  Start the app:
  ```bash
  dotnet run --project WeightLossTracker/
  ```

  Open the app in a browser. Navigate to the Vitals → Weight tab. Verify:
  1. Weight tab shows 3 cards: form, Trend, History (same layout as BP tab)
  2. With no entries: Trend card shows "No entries yet. Log your first weight above."
  3. Log a weight entry — Trend card renders a line chart with the entry
  4. Log a second entry on a different date — chart shows both points connected by a line
  5. Toggle dark mode — chart colors update via CSS variables, history table text uses correct token colors
  6. Edit and delete weight entries work without JS errors

- [ ] **Step 9: Commit**

  ```bash
  git add WeightLossTracker/wwwroot/js/app.js
  git commit -m "feat: add weight trend chart and align weight tab UX with BP tab"
  ```
