// ─── Constants ────────────────────────────────────────────────────────────────
const DAY_NAMES = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
const DISPLAY_ORDER = [1,2,3,4,5,6,0]; // Mon–Sun display order

// Reusable Tailwind class sets (light + dark variants) for dynamic HTML
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

let activeChart = null;
let activeProfile = null;
let currentUser = null;
let currentView = 'dashboard';
let lastCurrentWeight = null;

// ─── Markdown renderer ─────────────────────────────────────────────────────────
function md(text) {
  if (!text) return '';
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
}

// ─── Error display ─────────────────────────────────────────────────────────────
function showError(containerId, message) {
  const el = document.getElementById(containerId);
  if (!el) return;
  el.innerHTML = `
    <div role="alert" class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-300 rounded-lg p-3 flex items-start gap-2 mt-2">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none"
           stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
           class="flex-shrink-0 mt-0.5" aria-hidden="true">
        <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <span class="flex-1 text-sm">${escHtml(message)}</span>
      <button onclick="this.closest('[role=alert]').remove()"
              aria-label="Dismiss error"
              class="text-red-500 dark:text-red-400 hover:text-red-700 dark:hover:text-red-200 font-bold text-lg leading-none">
        &times;
      </button>
    </div>`;
}

function clearError(containerId) {
  const el = document.getElementById(containerId);
  if (el) el.innerHTML = '';
}

function escHtml(str) {
  return String(str ?? '')
    .replace(/&/g,'&amp;').replace(/</g,'&lt;')
    .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function fmtDate(isoStr) {
  if (!isoStr) return '';
  return new Date(isoStr).toLocaleDateString(undefined, {
    month:'short', day:'numeric', year:'numeric'
  });
}

function fmtDateTime(isoStr) {
  if (!isoStr) return '';
  return new Date(isoStr).toLocaleString(undefined, {
    month:'short', day:'numeric', hour:'2-digit', minute:'2-digit'
  });
}

// ─── Dark mode ────────────────────────────────────────────────────────────────
function initTheme() {
  const saved = localStorage.getItem('theme');
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  const isDark = saved === 'dark' || (!saved && prefersDark);
  applyTheme(isDark);

  // Keep in sync if OS preference changes and no user override is stored
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
    if (!localStorage.getItem('theme')) applyTheme(e.matches);
  });
}

function applyTheme(isDark) {
  document.documentElement.classList.toggle('dark', isDark);
  document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
  // Update every theme toggle button (desktop sidebar + mobile header)
  document.querySelectorAll('[data-theme-toggle]').forEach(btn => {
    const sunIcon  = btn.querySelector('[data-sun]');
    const moonIcon = btn.querySelector('[data-moon]');
    const label    = btn.querySelector('[data-theme-label]');
    if (isDark) {
      sunIcon?.classList.remove('hidden');
      moonIcon?.classList.add('hidden');
      if (label) label.textContent = 'Light mode';
      btn.setAttribute('aria-label', 'Switch to light mode');
    } else {
      sunIcon?.classList.add('hidden');
      moonIcon?.classList.remove('hidden');
      if (label) label.textContent = 'Dark mode';
      btn.setAttribute('aria-label', 'Switch to dark mode');
    }
  });
}

function toggleTheme() {
  const isDark = !document.documentElement.classList.contains('dark');
  localStorage.setItem('theme', isDark ? 'dark' : 'light');
  applyTheme(isDark);
}

// ─── Auth ──────────────────────────────────────────────────────────────────
function showLoginView() {
  document.getElementById('login-view').classList.remove('hidden');
}

function hideLoginView() {
  document.getElementById('login-view').classList.add('hidden');
}

async function initAuth() {
  const r = await Bridge.call('getMe');
  if (!r.ok) {
    showLoginView();
    return false;
  }
  currentUser = { username: r.data.username };
  activeProfile = r.data.profile;
  updateProfileUI();
  return true;
}

async function handleLogout() {
  await Bridge.call('logout');
  currentUser = null;
  activeProfile = null;
  showLoginView();
}

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

document.getElementById('login-form').addEventListener('submit', async e => {
  e.preventDefault();
  const errEl = document.getElementById('login-error');
  errEl.innerHTML = '';

  const username = document.getElementById('login-username').value.trim();
  const password = document.getElementById('login-password').value;
  const btn = document.getElementById('login-btn');

  if (!username || !password) {
    showError('login-error', 'Username and password are required.');
    return;
  }

  btn.disabled = true;
  btn.textContent = 'Logging in…';

  const r = await Bridge.call('login', { username, password });

  btn.disabled = false;
  btn.textContent = 'Log in';

  if (!r.ok) {
    showError('login-error', 'Invalid username or password.');
    document.getElementById('login-password').value = '';
    document.getElementById('login-username').focus();
    return;
  }

  currentUser = { username: r.data.username };
  hideLoginView();

  const meResult = await Bridge.call('getMe');
  if (meResult.ok) {
    activeProfile = meResult.data.profile;
    updateProfileUI();
  }

  navigate('dashboard');
});

// ─── Router ───────────────────────────────────────────────────────────────────
function navigate(viewName) {
  currentView = viewName;
  if (activeChart) { activeChart.destroy(); activeChart = null; }

  // Desktop sidebar active state
  document.querySelectorAll('[data-nav]').forEach(el => {
    const active = el.dataset.nav === viewName;
    el.classList.toggle('nav-active', active);
  });
  // Mobile bottom tab active state
  document.querySelectorAll('[data-mobile-nav]').forEach(el => {
    el.classList.toggle('mobile-tab-active', el.dataset.mobileNav === viewName);
  });

  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="flex justify-center py-16" role="status" aria-label="Loading">
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-[var(--color-accent)]"></div>
    </div>`;

  const views = {
    dashboard: renderDashboard,
    vitals:    renderVitals,
    exercise:  renderExercise,
    meals:     renderMeals,
    history:   renderHistory,
    profile:   renderProfile,
  };
  (views[viewName] || renderDashboard)();
}

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
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

function kpiCard(label, value, colorClass, icon) {
  const icons = {
    'scale':        '<path d="M12 3a9 9 0 1 0 9 9H3a9 9 0 0 0 9-9z"/><line x1="12" y1="3" x2="12" y2="3.01"/><line x1="3" y1="12" x2="21" y2="12"/>',
    'trending-down':'<polyline points="23 18 13.5 8.5 8.5 13.5 1 6"/><polyline points="17 18 23 18 23 12"/>',
    'target':       '<circle cx="12" cy="12" r="10"/><circle cx="12" cy="12" r="6"/><circle cx="12" cy="12" r="2"/>',
    'calendar':     '<rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>',
  };
  return `
    <div class="${C.card} flex flex-col gap-1" role="listitem">
      <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24"
           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
           stroke-linejoin="round" class="${colorClass}" aria-hidden="true">
        ${icons[icon] || ''}
      </svg>
      <div class="text-2xl font-bold ${colorClass}">${value}</div>
      <div class="${C.mutedText}">${label}</div>
    </div>`;
}

// ─── VITALS (tab router) ──────────────────────────────────────────────────────
async function renderVitals(activeTab = 'weight') {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="${C.h1}">Vitals</h1>
      <div class="flex border-b border-[var(--color-border-subtle)]">
        <button onclick="renderVitals('weight')"
                class="px-4 py-2 text-sm font-semibold border-b-2 transition-colors ${activeTab === 'weight'
                  ? 'border-[var(--color-accent)] text-[var(--color-accent)]'
                  : 'border-transparent text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)]'}">
          Weight
        </button>
        <button onclick="renderVitals('bp')"
                class="px-4 py-2 text-sm font-semibold border-b-2 transition-colors ${activeTab === 'bp'
                  ? 'border-[var(--color-accent)] text-[var(--color-accent)]'
                  : 'border-transparent text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)]'}">
          Blood Pressure
        </button>
      </div>
      <div id="vitals-content"></div>
    </div>`;

  const content = document.getElementById('vitals-content');
  if (activeTab === 'bp') {
    await renderBloodPressureContent(content);
  } else {
    await renderWeightContent(content);
  }
}

// ─── WEIGHT LOG ───────────────────────────────────────────────────────────────
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

let editingWeightId = null;

function startEditWeight(id, weight, notes) {
  if (editingWeightId) cancelEditWeight(editingWeightId);
  editingWeightId = id;
  const row = document.getElementById(`row-${id}`);
  const cells = row.querySelectorAll('td');
  cells[1].innerHTML = `
    <input id="edit-w-${id}" type="number" step="0.1" min="50" max="500" value="${weight}"
           aria-label="Edit weight" class="${C.inputSm} w-24">`;
  cells[2].innerHTML = `
    <input id="edit-n-${id}" type="text" value="${escHtml(notes)}"
           aria-label="Edit notes" class="${C.inputSm} w-full">`;
  cells[3].innerHTML = `
    <div class="flex gap-2">
      <button onclick="saveEditWeight(${id})"
              class="text-green-600 dark:text-green-400 hover:text-green-800 dark:hover:text-green-200 text-xs font-medium">
        Save
      </button>
      <button onclick="cancelEditWeight(${id})"
              class="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 text-xs font-medium">
        Cancel
      </button>
    </div>`;
  document.getElementById(`edit-w-${id}`)?.focus();
}

function cancelEditWeight(id) {
  editingWeightId = null;
  loadWeightTable();
}

async function saveEditWeight(id) {
  const weight = parseFloat(document.getElementById(`edit-w-${id}`)?.value ?? '');
  const notes  = document.getElementById(`edit-n-${id}`)?.value.trim() || null;
  if (isNaN(weight)) return;
  const r = await Bridge.call('updateWeight', { id, weight, notes });
  if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
  editingWeightId = null;
  await loadWeightTable();
}

async function deleteWeight(id) {
  if (!confirm('Delete this weight entry? This cannot be undone.')) return;
  const r = await Bridge.call('deleteWeight', { id });
  if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
  await loadWeightTable();
}

// ─── EXERCISE SCHEDULE ────────────────────────────────────────────────────────
let scheduleData = [];

async function renderExercise() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="${C.h1}">Exercise schedule</h1>
      <div id="exercise-error"></div>

      <div class="${C.card}">
        <div class="flex flex-wrap justify-between items-center mb-4 gap-3">
          <h2 class="text-lg font-semibold text-gray-700 dark:text-gray-200">Weekly schedule</h2>
          <div class="flex gap-2 flex-wrap">
            <button onclick="saveSchedule()" class="${C.btnSecondary}">Save schedule</button>
            <button id="btn-gen-week" onclick="generateWeek()"
                    class="${C.btnPrimary} px-4 text-sm">
              Generate full week
            </button>
          </div>
        </div>
        <div id="schedule-grid" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-7 gap-3"></div>
        <div id="week-progress" class="hidden mt-3 text-sm text-indigo-600 dark:text-indigo-400 font-medium"
             role="status" aria-live="polite"></div>
      </div>

      <div id="workout-results" class="space-y-4"></div>
    </div>`;

  await loadSchedule();
  await loadExerciseHistory();
}

async function loadSchedule() {
  const r = await Bridge.call('getSchedule');
  if (!r.ok) { showError('exercise-error', 'Failed to load schedule.'); return; }
  scheduleData = r.data;
  renderScheduleGrid();
}

async function loadExerciseHistory() {
  const r = await Bridge.call('getExerciseHistory');
  if (!r.ok) return;
  const history = Array.isArray(r.data) ? r.data : [];

  // Most recent per day
  const latestPerDay = new Map();
  for (const s of history) {
    if (!latestPerDay.has(s.dayOfWeek)) latestPerDay.set(s.dayOfWeek, s);
  }

  const resultsDiv = document.getElementById('workout-results');
  if (!resultsDiv) return;
  resultsDiv.innerHTML = '';
  for (const dow of DISPLAY_ORDER) {
    if (latestPerDay.has(dow)) renderWorkoutCard(latestPerDay.get(dow), false);
  }
}

function renderScheduleGrid() {
  const grid = document.getElementById('schedule-grid');
  if (!grid) return;

  grid.innerHTML = DISPLAY_ORDER.map(dow => {
    const day = scheduleData.find(s => s.dayOfWeek === dow) || { dayOfWeek: dow, location: 'Rest' };
    const isRest = day.location === 'Rest';
    return `
      <div class="border dark:border-gray-700 rounded-lg p-3 flex flex-col gap-2
                  ${isRest
                    ? 'bg-gray-50 dark:bg-gray-900'
                    : 'bg-indigo-50 dark:bg-indigo-900/20 border-indigo-200 dark:border-indigo-800'}">
        <div class="font-semibold text-sm text-center text-gray-700 dark:text-gray-200">
          ${DAY_NAMES[dow]}
        </div>
        <select aria-label="${DAY_NAMES[dow]} location"
                onchange="updateScheduleLocal(${dow}, this.value)"
                class="text-sm border dark:border-gray-600 rounded px-2 py-1 bg-white dark:bg-gray-700 text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-400">
          ${['Rest','Home','Gym'].map(loc =>
            `<option value="${loc}" ${day.location === loc ? 'selected' : ''}>${loc}</option>`
          ).join('')}
        </select>
        ${!isRest ? `
          <button onclick="generateDay(${dow})"
                  id="btn-day-${dow}"
                  class="${C.btnSmPrimary} w-full min-h-[36px]">
            Generate ${DAY_NAMES[dow]}
          </button>` : ''}
      </div>`;
  }).join('');
}

function updateScheduleLocal(dow, location) {
  const day = scheduleData.find(s => s.dayOfWeek === dow);
  if (day) day.location = location;
  renderScheduleGrid();
}

async function saveSchedule() {
  const items = scheduleData.map(s => ({ dayOfWeek: s.dayOfWeek, location: s.location }));
  const r = await Bridge.call('saveSchedule', items);
  if (!r.ok) { showError('exercise-error', r.data?.detail || r.data); return; }
  scheduleData = r.data;
  renderScheduleGrid();
}

async function generateDay(dow) {
  clearError('exercise-error');

  // Persist schedule first
  const items = scheduleData.map(s => ({ dayOfWeek: s.dayOfWeek, location: s.location }));
  await Bridge.call('saveSchedule', items);

  const btn = document.getElementById(`btn-day-${dow}`);
  if (btn) { btn.disabled = true; btn.textContent = 'Generating…'; }

  const r = await Bridge.call('generateDayWorkout', { dayOfWeek: dow });

  if (btn) { btn.disabled = false; btn.textContent = `Generate ${DAY_NAMES[dow]}`; }

  if (!r.ok) {
    showError('exercise-error', r.data?.detail || r.data || 'Generation failed.');
    return;
  }

  // Remove existing card for this day before rendering the new one
  const resultsDiv = document.getElementById('workout-results');
  if (resultsDiv) {
    resultsDiv.querySelectorAll(`[data-dow="${dow}"]`).forEach(card => card.remove());
  }

  renderWorkoutCard(r.data, true);
}

async function generateWeek() {
  clearError('exercise-error');

  // Persist schedule first
  const items = scheduleData.map(s => ({ dayOfWeek: s.dayOfWeek, location: s.location }));
  await Bridge.call('saveSchedule', items);

  const activeDays = DISPLAY_ORDER.filter(dow => {
    const d = scheduleData.find(s => s.dayOfWeek === dow);
    return d && d.location !== 'Rest';
  });

  if (activeDays.length === 0) {
    showError('exercise-error', 'No active days to generate. Set at least one day to Home or Gym first.');
    return;
  }

  const btn      = document.getElementById('btn-gen-week');
  const progress = document.getElementById('week-progress');
  btn.disabled   = true;
  progress.classList.remove('hidden');

  // Clear existing results
  const resultsDiv = document.getElementById('workout-results');
  if (resultsDiv) resultsDiv.innerHTML = '';

  let hadError = false;
  for (let i = 0; i < activeDays.length; i++) {
    const dow = activeDays[i];
    progress.textContent =
      `Generating ${DAY_NAMES[dow]} — day ${i + 1} of ${activeDays.length}…`;

    const r = await Bridge.call('generateDayWorkout', { dayOfWeek: dow });

    if (!r.ok) {
      showError('exercise-error',
        `Failed on ${DAY_NAMES[dow]}: ${r.data?.detail || r.data || 'Generation failed.'}`);
      hadError = true;
      break;
    }

    renderWorkoutCard(r.data, false);
  }

  btn.disabled = false;
  progress.textContent = hadError ? '' : `All ${activeDays.length} workout${activeDays.length !== 1 ? 's' : ''} generated.`;
  if (!hadError) setTimeout(() => progress.classList.add('hidden'), 3000);
}

function renderWorkoutCard(suggestion, prepend = false) {
  const resultsDiv = document.getElementById('workout-results');
  if (!resultsDiv) return;

  const dayName = suggestion.dayOfWeek != null ? DAY_NAMES[suggestion.dayOfWeek] : '';
  const categoryColors = {
    Cardio:      'bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300',
    Strength:    'bg-purple-100 dark:bg-purple-900/40 text-purple-700 dark:text-purple-300',
    Flexibility: 'bg-green-100 dark:bg-green-900/40 text-green-700 dark:text-green-300',
  };
  const badgeClass = categoryColors[suggestion.category] || 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300';

  const div = document.createElement('div');
  div.className = `${C.card}`;
  div.dataset.suggestionId = suggestion.id;
  div.dataset.dow = suggestion.dayOfWeek;
  div.innerHTML = `
    <div class="flex justify-between items-start mb-3">
      <div>
        <h3 class="${C.h3} text-lg">${escHtml(dayName)} — ${escHtml(suggestion.location)}</h3>
        <p class="${C.tinyText}">${fmtDateTime(suggestion.createdAt)}</p>
      </div>
      <div class="flex items-center gap-2">
        <span class="${C.badge} ${badgeClass}">${escHtml(suggestion.category)}</span>
        <button onclick="deleteWorkoutCard(${suggestion.id}, this)"
                aria-label="Delete ${escHtml(dayName)} workout"
                class="text-gray-300 dark:text-gray-600 hover:text-red-500 dark:hover:text-red-400 transition-colors text-xl leading-none min-w-[44px] min-h-[44px] flex items-center justify-center">
          &times;
        </button>
      </div>
    </div>
    <div class="prose prose-sm max-w-none text-gray-700 dark:text-gray-200">
      ${md(suggestion.content)}
    </div>`;

  if (prepend && resultsDiv.firstChild) {
    resultsDiv.insertBefore(div, resultsDiv.firstChild);
  } else {
    resultsDiv.appendChild(div);
  }
}

async function deleteWorkoutCard(id, btn) {
  btn.disabled = true;
  const r = await Bridge.call('deleteExerciseHistory', { id });
  if (!r.ok) {
    btn.disabled = false;
    showError('exercise-error', 'Failed to delete workout.');
    return;
  }
  document.querySelector(`[data-suggestion-id="${id}"]`)?.remove();
}

// ─── MEALS ────────────────────────────────────────────────────────────────────
async function renderMeals() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="${C.h1}">Meal log</h1>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">

        <!-- Left: entry form + today's meals -->
        <div class="space-y-4">
          <div class="${C.card}">
            <h2 class="${C.h2}">Add meal</h2>
            <div id="meal-error"></div>
            <form id="meal-form" class="space-y-3" novalidate>
              <div class="flex gap-3">
                <div class="flex-1">
                  <label for="meal-type" class="${C.label}">Type</label>
                  <select id="meal-type" class="${C.select}">
                    <option>Breakfast</option>
                    <option>Lunch</option>
                    <option>Dinner</option>
                    <option>Snack</option>
                  </select>
                </div>
                <div class="w-28">
                  <label for="meal-cal" class="${C.label}">Calories</label>
                  <input id="meal-cal" type="number" min="0"
                         class="${C.input}" placeholder="optional">
                </div>
              </div>
              <div>
                <label for="meal-desc" class="${C.label}">Description</label>
                <input id="meal-desc" type="text" required
                       class="${C.input}" placeholder="e.g. Oatmeal with berries">
              </div>
              <div>
                <label for="meal-notes" class="${C.label}">Notes</label>
                <input id="meal-notes" type="text"
                       class="${C.input}" placeholder="optional">
              </div>
              <button type="submit" class="${C.btnPrimary} w-full">Add meal</button>
            </form>
          </div>

          <div class="${C.card}">
            <div id="meal-table-wrap"></div>
          </div>
        </div>

        <!-- Right: AI nutrition advice -->
        <div class="${C.card} flex flex-col gap-4">
          <h2 class="text-lg font-semibold text-gray-700 dark:text-gray-200">
            Ask Gemini for nutrition advice
          </h2>
          <div id="advice-error"></div>
          <div class="flex flex-col gap-2">
            <label for="advice-question" class="sr-only">Nutrition question</label>
            <textarea id="advice-question" rows="3"
                      class="${C.input} resize-none"
                      placeholder="e.g. What should I eat after my workout?"></textarea>
            <button id="btn-ask" onclick="askAdvice()" class="${C.btnSuccess} w-full">
              Ask Gemini
            </button>
          </div>
          <div id="advice-panel" class="prose prose-sm text-gray-700 dark:text-gray-200 flex-1 overflow-y-auto"
               aria-live="polite"></div>
        </div>
      </div>
    </div>`;

  await loadMealTable();

  document.getElementById('meal-form').addEventListener('submit', async e => {
    e.preventDefault();
    clearError('meal-error');

    const mealType    = document.getElementById('meal-type').value;
    const description = document.getElementById('meal-desc').value.trim();
    const cal         = document.getElementById('meal-cal').value;
    const calories    = cal ? parseInt(cal) : null;
    const notes       = document.getElementById('meal-notes').value.trim() || null;

    if (!description) {
      showError('meal-error', 'Description is required.');
      document.getElementById('meal-desc').focus();
      return;
    }

    const r = await Bridge.call('addMeal', { mealType, description, calories, notes });
    if (!r.ok) { showError('meal-error', r.data?.detail || r.data); return; }

    document.getElementById('meal-desc').value  = '';
    document.getElementById('meal-cal').value   = '';
    document.getElementById('meal-notes').value = '';
    await loadMealTable();
  });
}

async function loadMealTable() {
  const wrap = document.getElementById('meal-table-wrap');
  if (!wrap) return;
  const r = await Bridge.call('getTodayMeals');
  if (!r.ok) {
    wrap.innerHTML = `<p class="text-red-500 dark:text-red-400 text-sm">Failed to load meals.</p>`;
    return;
  }

  const meals    = r.data;
  const totalCal = meals.reduce((s, m) => s + (m.calories || 0), 0);
  const mealTypeColors = {
    Breakfast: 'bg-yellow-100 dark:bg-yellow-900/40 text-yellow-700 dark:text-yellow-300',
    Lunch:     'bg-green-100 dark:bg-green-900/40 text-green-700 dark:text-green-300',
    Dinner:    'bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300',
    Snack:     'bg-purple-100 dark:bg-purple-900/40 text-purple-700 dark:text-purple-300',
  };

  wrap.innerHTML = `
    <div class="flex justify-between items-center mb-3">
      <h3 class="font-semibold text-gray-700 dark:text-gray-200">Today's meals</h3>
      ${totalCal > 0
        ? `<span class="text-sm font-medium text-indigo-600 dark:text-indigo-400">
             ${totalCal} cal total
           </span>`
        : ''}
    </div>
    ${meals.length === 0
      ? `<p class="${C.mutedText}">No meals logged today.</p>`
      : `<div class="space-y-2" id="meal-error">
           ${meals.map(m => {
             const badgeClass = mealTypeColors[m.mealType] || 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300';
             return `
               <div class="flex justify-between items-start border dark:border-gray-700 rounded-lg p-3 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                 <div class="flex-1 min-w-0">
                   <div class="flex items-center gap-2 flex-wrap">
                     <span class="${C.badge} ${badgeClass}">${escHtml(m.mealType)}</span>
                     ${m.calories ? `<span class="text-xs text-gray-400 dark:text-gray-500">${m.calories} cal</span>` : ''}
                   </div>
                   <div class="text-sm font-medium text-gray-800 dark:text-gray-100 mt-1">
                     ${escHtml(m.description)}
                   </div>
                   ${m.notes ? `<div class="text-xs text-gray-400 dark:text-gray-500">${escHtml(m.notes)}</div>` : ''}
                 </div>
                 <button onclick="deleteMeal(${m.id})"
                         aria-label="Delete ${escHtml(m.mealType)} — ${escHtml(m.description)}"
                         class="text-red-400 dark:text-red-500 hover:text-red-600 dark:hover:text-red-300 text-xs ml-2 flex-shrink-0 min-h-[44px] min-w-[44px] flex items-center justify-end">
                   Delete
                 </button>
               </div>`;
           }).join('')}
         </div>`}`;
}

async function deleteMeal(id) {
  if (!confirm('Delete this meal? This cannot be undone.')) return;
  const r = await Bridge.call('deleteMeal', { id });
  if (!r.ok) { showError('meal-error', r.data?.detail || r.data); return; }
  await loadMealTable();
}

async function askAdvice() {
  clearError('advice-error');
  const question = document.getElementById('advice-question').value.trim();
  if (!question) {
    showError('advice-error', 'Enter a question first.');
    document.getElementById('advice-question').focus();
    return;
  }

  const btn   = document.getElementById('btn-ask');
  const panel = document.getElementById('advice-panel');
  btn.disabled     = true;
  btn.textContent  = 'Asking Gemini…';
  panel.innerHTML  = `
    <div class="flex items-center gap-2 text-gray-400 dark:text-gray-500 text-sm" role="status">
      <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-indigo-500"></div>
      Generating advice…
    </div>`;

  const r = await Bridge.call('getMealAdvice', { question });
  btn.disabled    = false;
  btn.textContent = 'Ask Gemini';

  if (!r.ok) {
    panel.innerHTML = '';
    showError('advice-error', r.data?.detail || r.data || 'Failed to get advice.');
    return;
  }

  panel.innerHTML = md(r.data.advice);
}

// ─── AI HISTORY ───────────────────────────────────────────────────────────────
let aiHistoryFilter = 'All';
let aiHistoryData   = [];
let selectedAiId    = null;

async function renderHistory() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-4">
      <h1 class="${C.h1}">AI history</h1>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6" style="min-height: 70vh;">

        <!-- Left: filter tabs + list -->
        <div class="flex flex-col gap-3">
          <div class="flex gap-1 flex-wrap" id="history-filters" role="tablist" aria-label="Filter AI history">
            ${['All','Exercise','Meal','General'].map(f => `
              <button role="tab"
                      aria-selected="${f === aiHistoryFilter}"
                      onclick="setHistoryFilter('${f}')"
                      data-filter="${f}"
                      class="text-xs px-3 py-2 rounded-full font-medium transition-colors min-h-[36px]
                        ${f === aiHistoryFilter
                          ? 'bg-indigo-600 dark:bg-indigo-700 text-white'
                          : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'}">
                ${f}
              </button>`).join('')}
          </div>
          <div id="history-list" class="flex-1 overflow-y-auto space-y-2 pr-1"
               role="listbox" aria-label="AI history entries"></div>
        </div>

        <!-- Right: detail pane -->
        <div class="lg:col-span-2 bg-white dark:bg-gray-800 rounded-xl shadow overflow-y-auto">
          <div id="history-detail"
               class="p-5 h-full flex items-center justify-center text-gray-400 dark:text-gray-500 text-sm">
            Select an entry to view details
          </div>
        </div>
      </div>
    </div>`;

  await loadAiHistory();
}

async function loadAiHistory() {
  const params = aiHistoryFilter !== 'All' ? { type: aiHistoryFilter } : {};
  const r = await Bridge.call('getAiHistory', params);
  if (!r.ok) return;
  aiHistoryData = r.data;
  renderHistoryList();
}

function renderHistoryList() {
  const list = document.getElementById('history-list');
  if (!list) return;

  if (!aiHistoryData.length) {
    list.innerHTML = `<p class="${C.mutedText}">No entries.</p>`;
    return;
  }

  const typeColors = {
    Exercise: 'bg-purple-100 dark:bg-purple-900/40 text-purple-700 dark:text-purple-300',
    Meal:     'bg-green-100 dark:bg-green-900/40 text-green-700 dark:text-green-300',
    General:  'bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300',
  };

  list.innerHTML = aiHistoryData.map(item => {
    const badge    = typeColors[item.promptType] || 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300';
    const isSelected = item.id === selectedAiId;
    return `
      <div onclick="selectAiHistory(${item.id})"
           role="option"
           aria-selected="${isSelected}"
           tabindex="0"
           onkeydown="if(event.key==='Enter'||event.key===' ')selectAiHistory(${item.id})"
           class="cursor-pointer border dark:border-gray-700 rounded-lg p-3 transition-colors
                  ${isSelected
                    ? 'border-indigo-400 dark:border-indigo-600 bg-indigo-50 dark:bg-indigo-900/20'
                    : 'bg-white dark:bg-gray-800 hover:bg-indigo-50 dark:hover:bg-gray-700'}">
        <div class="flex justify-between items-start gap-2">
          <span class="${C.badge} ${badge}">${escHtml(item.promptType)}</span>
          <button onclick="event.stopPropagation(); deleteAiHistory(${item.id})"
                  aria-label="Delete ${escHtml(item.promptType)} history entry from ${fmtDateTime(item.createdAt)}"
                  class="text-red-400 dark:text-red-500 hover:text-red-600 dark:hover:text-red-300 text-xs font-medium min-h-[32px] px-1">
            Delete
          </button>
        </div>
        <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">${fmtDateTime(item.createdAt)}</p>
        <p class="text-sm text-gray-700 dark:text-gray-200 mt-1 truncate">
          ${escHtml(item.prompt.substring(0, 80))}…
        </p>
      </div>`;
  }).join('');
}

function setHistoryFilter(filter) {
  aiHistoryFilter = filter;
  selectedAiId    = null;

  document.querySelectorAll('[data-filter]').forEach(btn => {
    const active = btn.dataset.filter === filter;
    btn.setAttribute('aria-selected', active);
    btn.className = `text-xs px-3 py-2 rounded-full font-medium transition-colors min-h-[36px] ${
      active
        ? 'bg-indigo-600 dark:bg-indigo-700 text-white'
        : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
    }`;
  });

  loadAiHistory();
}

function selectAiHistory(id) {
  selectedAiId = id;
  const item = aiHistoryData.find(x => x.id === id);
  if (!item) return;
  renderHistoryList();

  const detail = document.getElementById('history-detail');
  detail.innerHTML = `
    <div class="space-y-4 p-5">
      <div class="flex justify-between items-start">
        <div>
          <h3 class="${C.h3}">${escHtml(item.promptType)} prompt</h3>
          <p class="${C.tinyText}">${fmtDateTime(item.createdAt)} · ${escHtml(item.model)}</p>
        </div>
        <div class="text-right text-xs text-gray-400 dark:text-gray-500 shrink-0">
          ${item.inputTokens.toLocaleString()} in<br>
          ${item.outputTokens.toLocaleString()} out tokens
        </div>
      </div>

      <div>
        <h4 class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">
          Prompt
        </h4>
        <pre class="text-xs bg-gray-50 dark:bg-gray-900 border dark:border-gray-700 rounded-lg p-3 overflow-x-auto whitespace-pre-wrap text-gray-700 dark:text-gray-200">${escHtml(item.prompt)}</pre>
      </div>

      <div>
        <h4 class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">
          Response
        </h4>
        <div class="prose prose-sm max-w-none bg-gray-50 dark:bg-gray-900 border dark:border-gray-700 rounded-lg p-3 text-gray-700 dark:text-gray-200">
          ${md(item.response)}
        </div>
      </div>
    </div>`;
}

async function deleteAiHistory(id) {
  if (!confirm('Delete this AI history entry? This cannot be undone.')) return;
  const r = await Bridge.call('deleteAiHistory', { id });
  if (!r.ok) {
    // Show inline error — never use alert() for API errors
    const list = document.getElementById('history-list');
    const errId = 'ai-history-error';
    let errEl = document.getElementById(errId);
    if (!errEl) {
      errEl = document.createElement('div');
      errEl.id = errId;
      list?.parentElement?.insertBefore(errEl, list);
    }
    showError(errId, r.data?.detail || r.data || 'Failed to delete.');
    return;
  }
  if (selectedAiId === id) {
    selectedAiId = null;
    const detail = document.getElementById('history-detail');
    if (detail) {
      detail.innerHTML = `
        <div class="h-full flex items-center justify-center text-gray-400 dark:text-gray-500 text-sm">
          Select an entry to view details
        </div>`;
    }
  }
  await loadAiHistory();
}

// ─── PROFILE MANAGEMENT ──────────────────────────────────────────────────────
async function renderProfile() {
  const root = document.getElementById('view-root');

  if (!activeProfile) {
    root.innerHTML = `<p class="text-red-600 dark:text-red-400 p-4">Profile not loaded.</p>`;
    return;
  }

  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="${C.h1}">Profile</h1>

      <div class="${C.card} max-w-lg">
        <h2 class="${C.h2}">Edit your profile</h2>
        <div id="profile-edit-error"></div>
        <form id="profile-edit-form" class="space-y-3" novalidate>
          <div>
            <label for="pe-name" class="${C.label}">Display name</label>
            <input id="pe-name" type="text" required class="${C.input}" value="${escHtml(activeProfile.name || '')}">
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label for="pe-sw" class="${C.label}">Starting weight (lbs)</label>
              <input id="pe-sw" type="number" step="0.1" min="50" max="500" required class="${C.input}" value="${activeProfile.startingWeight || ''}">
            </div>
            <div>
              <label for="pe-gw" class="${C.label}">Goal weight (lbs)</label>
              <input id="pe-gw" type="number" step="0.1" min="50" max="500" required class="${C.input}" value="${activeProfile.goalWeight || ''}">
            </div>
          </div>
          <div>
            <label for="pe-fl" class="${C.label}">Fitness level</label>
            <select id="pe-fl" class="${C.select}">
              ${['Beginner','Intermediate','Advanced'].map(l =>
                `<option ${activeProfile.fitnessLevel === l ? 'selected' : ''}>${l}</option>`
              ).join('')}
            </select>
          </div>
          <div>
            <label for="pe-inj" class="${C.label}">Injuries</label>
            <input id="pe-inj" type="text" class="${C.input}" value="${escHtml(activeProfile.injuries || '')}" placeholder="e.g. Neck injury, lower back injury">
          </div>
          <div>
            <label for="pe-goals" class="${C.label}">Goals</label>
            <input id="pe-goals" type="text" class="${C.input}" value="${escHtml(activeProfile.goals || '')}" placeholder="e.g. Lose weight, build muscle">
          </div>
          <button type="submit" class="${C.btnPrimary} w-full">Save changes</button>
        </form>
      </div>
    </div>`;

  document.getElementById('profile-edit-form').addEventListener('submit', async e => {
    e.preventDefault();
    clearError('profile-edit-error');
    const data = {
      id: activeProfile.id,
      name: document.getElementById('pe-name').value.trim(),
      startingWeight: parseFloat(document.getElementById('pe-sw').value),
      goalWeight: parseFloat(document.getElementById('pe-gw').value),
      startDate: activeProfile.startDate,
      fitnessLevel: document.getElementById('pe-fl').value,
      injuries: document.getElementById('pe-inj').value.trim(),
      goals: document.getElementById('pe-goals').value.trim(),
    };
    if (!data.name) { showError('profile-edit-error', 'Name is required.'); return; }
    if (isNaN(data.startingWeight) || isNaN(data.goalWeight)) {
      showError('profile-edit-error', 'Starting and goal weights are required.');
      return;
    }
    const r = await Bridge.call('updateProfile', data);
    if (!r.ok) { showError('profile-edit-error', r.data?.detail || r.data); return; }
    activeProfile = r.data;
    updateProfileUI();
    renderProfile();
  });
}

// ─── Initialise ───────────────────────────────────────────────────────────────
initTheme();
initAuth().then(authenticated => {
  if (authenticated) navigate('dashboard');
});
