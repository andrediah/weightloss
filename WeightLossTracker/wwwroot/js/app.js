// ─── Utilities ────────────────────────────────────────────────────────────────
const DAY_NAMES = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
const DISPLAY_ORDER = [1,2,3,4,5,6,0]; // Mon–Sun display

let activeChart = null;

function md(text) {
  if (!text) return '';
  return text
    .replace(/^### (.+)$/gm, '<h3 class="text-base font-semibold mt-3 mb-1">$1</h3>')
    .replace(/^## (.+)$/gm, '<h2 class="text-lg font-bold mt-4 mb-2 text-indigo-700">$1</h2>')
    .replace(/^# (.+)$/gm, '<h1 class="text-xl font-bold mt-4 mb-2">$1</h1>')
    .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.+?)\*/g, '<em>$1</em>')
    .replace(/^- (.+)$/gm, '<li class="ml-4 list-disc">$1</li>')
    .replace(/(<li[\s\S]*?<\/li>)/g, '<ul class="my-1">$1</ul>')
    .replace(/\n{2,}/g, '</p><p class="mt-2">')
    .replace(/^(?!<[hul])(.+)$/gm, '<p class="mt-1">$1</p>');
}

function showError(containerId, message) {
  const el = document.getElementById(containerId);
  if (!el) return;
  el.innerHTML = `
    <div class="bg-red-50 border border-red-200 text-red-800 rounded p-3 flex items-start gap-2 mt-2">
      <span class="flex-1 text-sm">${escHtml(message)}</span>
      <button onclick="this.parentElement.parentElement.innerHTML=''" class="text-red-500 hover:text-red-700 font-bold text-lg leading-none">&times;</button>
    </div>`;
}

function clearError(containerId) {
  const el = document.getElementById(containerId);
  if (el) el.innerHTML = '';
}

function escHtml(str) {
  return String(str ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function fmtDate(isoStr) {
  if (!isoStr) return '';
  return new Date(isoStr).toLocaleDateString(undefined, { month:'short', day:'numeric', year:'numeric' });
}

function fmtDateTime(isoStr) {
  if (!isoStr) return '';
  return new Date(isoStr).toLocaleString(undefined, { month:'short', day:'numeric', hour:'2-digit', minute:'2-digit' });
}

// ─── Router ───────────────────────────────────────────────────────────────────
function navigate(viewName) {
  if (activeChart) { activeChart.destroy(); activeChart = null; }
  document.querySelectorAll('[data-nav]').forEach(el => {
    el.classList.toggle('bg-indigo-700', el.dataset.nav === viewName);
    el.classList.toggle('text-white', el.dataset.nav === viewName);
    el.classList.toggle('text-indigo-100', el.dataset.nav !== viewName);
  });
  const root = document.getElementById('view-root');
  const views = { dashboard: renderDashboard, weight: renderWeight, exercise: renderExercise, meals: renderMeals, history: renderHistory };
  root.innerHTML = '<div class="flex justify-center py-16"><div class="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div></div>';
  (views[viewName] || renderDashboard)();
}

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function renderDashboard() {
  const root = document.getElementById('view-root');
  const r = await Bridge.call('getDashboard');
  if (!r.ok) { root.innerHTML = `<p class="text-red-600 p-4">Failed to load dashboard: ${escHtml(r.data?.detail || r.data)}</p>`; return; }
  const d = r.data;
  const cwDisplay = d.currentWeight != null ? d.currentWeight.toFixed(1) + ' lbs' : '—';

  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-gray-800">Dashboard</h1>

      <!-- KPI Cards -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
        ${kpiCard('Current Weight', cwDisplay, 'text-indigo-600', '⚖️')}
        ${kpiCard('Lost So Far', d.lostSoFar + ' lbs', 'text-green-600', '📉')}
        ${kpiCard('To Goal', d.toGoal + ' lbs', 'text-orange-600', '🎯')}
        ${kpiCard('Days Logged', d.daysLogged, 'text-purple-600', '📅')}
      </div>

      <!-- Progress Bar -->
      <div class="bg-white rounded-xl shadow p-5">
        <div class="flex justify-between text-sm text-gray-600 mb-2">
          <span>Progress to goal</span>
          <span class="font-semibold text-indigo-600">${d.progressPct}%</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-4 overflow-hidden">
          <div class="bg-indigo-500 h-4 rounded-full transition-all duration-700" style="width:${d.progressPct}%"></div>
        </div>
        <div class="flex justify-between text-xs text-gray-400 mt-1">
          <span>215 lbs</span><span>${d.goalWeight} lbs goal</span>
        </div>
      </div>

      <!-- Chart -->
      <div class="bg-white rounded-xl shadow p-5">
        <h2 class="text-lg font-semibold text-gray-700 mb-4">Weight Trend</h2>
        ${d.chart.labels.length === 0
          ? '<p class="text-gray-400 text-sm text-center py-8">No weight entries yet. Log your first weight to see the chart.</p>'
          : '<canvas id="weight-chart" height="100"></canvas>'}
      </div>
    </div>`;

  if (d.chart.labels.length > 0) {
    const ctx = document.getElementById('weight-chart').getContext('2d');
    const goalLine = Array(d.chart.labels.length).fill(d.goalWeight);
    activeChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: d.chart.labels,
        datasets: [
          { label: 'Weight (lbs)', data: d.chart.weights, borderColor: '#6366f1', backgroundColor: 'rgba(99,102,241,0.1)', tension: 0.3, pointRadius: 4, fill: true },
          { label: 'Trend', data: d.chart.trendLine, borderColor: '#f97316', borderDash: [6,3], pointRadius: 0, tension: 0 },
          { label: 'Goal', data: goalLine, borderColor: '#22c55e', borderDash: [4,4], pointRadius: 0, tension: 0 }
        ]
      },
      options: { responsive: true, plugins: { legend: { position: 'top' } }, scales: { y: { title: { display: true, text: 'lbs' } } } }
    });
  }
}

function kpiCard(label, value, colorClass, icon) {
  return `
    <div class="bg-white rounded-xl shadow p-5 flex flex-col gap-1">
      <div class="text-2xl">${icon}</div>
      <div class="text-2xl font-bold ${colorClass}">${value}</div>
      <div class="text-sm text-gray-500">${label}</div>
    </div>`;
}

// ─── WEIGHT LOG ───────────────────────────────────────────────────────────────
async function renderWeight() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-gray-800">Weight Log</h1>
      <div class="bg-white rounded-xl shadow p-5">
        <h2 class="text-lg font-semibold mb-4">Log Today's Weight</h2>
        <div id="weight-error"></div>
        <form id="weight-form" class="flex flex-wrap gap-3 items-end">
          <div>
            <label class="block text-sm text-gray-600 mb-1">Weight (lbs)</label>
            <input id="wt-weight" type="number" step="0.1" min="50" max="500" required
              class="border rounded-lg px-3 py-2 w-32 focus:outline-none focus:ring-2 focus:ring-indigo-400" placeholder="e.g. 212.5">
          </div>
          <div class="flex-1 min-w-48">
            <label class="block text-sm text-gray-600 mb-1">Notes (optional)</label>
            <input id="wt-notes" type="text" class="border rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-indigo-400" placeholder="e.g. After workout">
          </div>
          <button type="submit" class="bg-indigo-600 hover:bg-indigo-700 text-white px-5 py-2 rounded-lg font-medium transition">Save</button>
        </form>
      </div>
      <div class="bg-white rounded-xl shadow p-5">
        <h2 class="text-lg font-semibold mb-4">History</h2>
        <div id="weight-table-wrap"></div>
      </div>
    </div>`;

  await loadWeightTable();

  document.getElementById('weight-form').addEventListener('submit', async e => {
    e.preventDefault();
    clearError('weight-error');
    const weight = parseFloat(document.getElementById('wt-weight').value);
    const notes = document.getElementById('wt-notes').value.trim() || null;
    const r = await Bridge.call('saveWeight', { weight, notes });
    if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
    document.getElementById('wt-weight').value = '';
    document.getElementById('wt-notes').value = '';
    await loadWeightTable();
  });
}

async function loadWeightTable() {
  const wrap = document.getElementById('weight-table-wrap');
  if (!wrap) return;
  const r = await Bridge.call('getWeightEntries');
  if (!r.ok) { wrap.innerHTML = `<p class="text-red-500 text-sm">Failed to load entries.</p>`; return; }
  const entries = r.data;
  if (!entries.length) { wrap.innerHTML = '<p class="text-gray-400 text-sm">No entries yet.</p>'; return; }

  wrap.innerHTML = `
    <div class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead><tr class="border-b text-left text-gray-500">
          <th class="py-2 pr-4">Date</th><th class="py-2 pr-4">Weight</th><th class="py-2 flex-1">Notes</th><th class="py-2">Actions</th>
        </tr></thead>
        <tbody id="weight-tbody">
          ${entries.map(e => weightRow(e)).join('')}
        </tbody>
      </table>
    </div>`;
}

function weightRow(e) {
  return `<tr id="row-${e.id}" class="border-b last:border-0 hover:bg-gray-50">
    <td class="py-2 pr-4 text-gray-600">${fmtDate(e.date)}</td>
    <td class="py-2 pr-4 font-medium">${e.weight.toFixed(1)}</td>
    <td class="py-2 text-gray-500">${escHtml(e.notes || '')}</td>
    <td class="py-2 flex gap-2">
      <button onclick="startEditWeight(${e.id},${e.weight},'${escHtml(e.notes||'')}')" class="text-indigo-500 hover:text-indigo-700 text-xs">Edit</button>
      <button onclick="deleteWeight(${e.id})" class="text-red-400 hover:text-red-600 text-xs">Delete</button>
    </td>
  </tr>`;
}

let editingWeightId = null;
function startEditWeight(id, weight, notes) {
  if (editingWeightId) cancelEditWeight(editingWeightId);
  editingWeightId = id;
  const row = document.getElementById(`row-${id}`);
  const cells = row.querySelectorAll('td');
  cells[1].innerHTML = `<input id="edit-w-${id}" type="number" step="0.1" min="50" max="500" value="${weight}" class="border rounded px-2 py-1 w-24 text-sm focus:ring-1 focus:ring-indigo-400">`;
  cells[2].innerHTML = `<input id="edit-n-${id}" type="text" value="${escHtml(notes)}" class="border rounded px-2 py-1 w-full text-sm focus:ring-1 focus:ring-indigo-400">`;
  cells[3].innerHTML = `
    <button onclick="saveEditWeight(${id})" class="text-green-600 hover:text-green-800 text-xs mr-1">Save</button>
    <button onclick="cancelEditWeight(${id})" class="text-gray-400 hover:text-gray-600 text-xs">Cancel</button>`;
}

function cancelEditWeight(id) {
  editingWeightId = null;
  loadWeightTable();
}

async function saveEditWeight(id) {
  const weight = parseFloat(document.getElementById(`edit-w-${id}`).value);
  const notes = document.getElementById(`edit-n-${id}`).value.trim() || null;
  if (isNaN(weight)) return;
  const r = await Bridge.call('updateWeight', { id, weight, notes });
  if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
  editingWeightId = null;
  await loadWeightTable();
}

async function deleteWeight(id) {
  if (!confirm('Delete this weight entry?')) return;
  const r = await Bridge.call('deleteWeight', { id });
  if (!r.ok) { showError('weight-error', r.data?.detail || r.data); return; }
  await loadWeightTable();
}

// ─── EXERCISE ─────────────────────────────────────────────────────────────────
let scheduleData = [];
let exerciseHistory = {};
let selectedHistoryId = null;

async function renderExercise() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-gray-800">Exercise Schedule</h1>
      <div id="exercise-error"></div>

      <!-- Schedule Grid -->
      <div class="bg-white rounded-xl shadow p-5">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-lg font-semibold">Weekly Schedule</h2>
          <div class="flex gap-2">
            <button onclick="saveSchedule()" class="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm font-medium transition">Save Schedule</button>
            <button id="btn-gen-week" onclick="generateWeek()" class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition">Generate Full Week</button>
          </div>
        </div>
        <div id="schedule-grid" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-7 gap-3"></div>
        <div id="week-progress" class="hidden mt-3 text-sm text-indigo-600 font-medium"></div>
      </div>

      <!-- Workout Results -->
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
  // Show the most recent suggestion per day, in Mon–Sun display order
  const latestPerDay = new Map();
  for (const s of history) {
    if (!latestPerDay.has(s.dayOfWeek)) latestPerDay.set(s.dayOfWeek, s);
  }
  const resultsDiv = document.getElementById('workout-results');
  if (!resultsDiv) return;
  resultsDiv.innerHTML = '';
  for (const dow of DISPLAY_ORDER) {
    if (latestPerDay.has(dow)) renderWorkoutResult(latestPerDay.get(dow), false);
  }
}

function renderScheduleGrid() {
  const grid = document.getElementById('schedule-grid');
  if (!grid) return;
  grid.innerHTML = DISPLAY_ORDER.map(dow => {
    const day = scheduleData.find(s => s.dayOfWeek === dow) || { dayOfWeek: dow, location: 'Rest' };
    return `
      <div class="border rounded-lg p-3 flex flex-col gap-2 ${day.location === 'Rest' ? 'bg-gray-50' : 'bg-indigo-50 border-indigo-200'}">
        <div class="font-semibold text-sm text-center text-gray-700">${DAY_NAMES[dow]}</div>
        <select onchange="updateScheduleLocal(${dow},this.value)" class="text-sm border rounded px-2 py-1 focus:outline-none focus:ring-1 focus:ring-indigo-400">
          ${['Rest','Home','Gym'].map(loc => `<option value="${loc}" ${day.location===loc?'selected':''}>${loc}</option>`).join('')}
        </select>
        ${day.location !== 'Rest' ? `<button onclick="generateDay(${dow})" class="text-xs bg-indigo-600 hover:bg-indigo-700 text-white rounded px-2 py-1 transition">Generate ${DAY_NAMES[dow]}</button>` : ''}
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
  // Save schedule first
  const items = scheduleData.map(s => ({ dayOfWeek: s.dayOfWeek, location: s.location }));
  await Bridge.call('saveSchedule', items);
  const btn = document.querySelector(`button[onclick="generateDay(${dow})"]`);
  if (btn) { btn.disabled = true; btn.textContent = 'Generating…'; }

  const r = await Bridge.call('generateDayWorkout', { dayOfWeek: dow });

  if (btn) { btn.disabled = false; btn.textContent = `Generate ${DAY_NAMES[dow]}`; }
  if (!r.ok) { showError('exercise-error', r.data?.detail || r.data || 'Generation failed.'); return; }

  // Remove any existing card for this day before rendering the new one
  const resultsDiv = document.getElementById('workout-results');
  if (resultsDiv) {
    resultsDiv.querySelectorAll('[data-suggestion-id]').forEach(card => {
      if (card.querySelector('h3')?.textContent.startsWith(DAY_NAMES[dow])) card.remove();
    });
  }
  renderWorkoutResult(r.data, true);
}

async function generateWeek() {
  clearError('exercise-error');
  const items = scheduleData.map(s => ({ dayOfWeek: s.dayOfWeek, location: s.location }));
  await Bridge.call('saveSchedule', items);

  const btn = document.getElementById('btn-gen-week');
  const progress = document.getElementById('week-progress');
  btn.disabled = true;
  progress.classList.remove('hidden');

  const activeDays = DISPLAY_ORDER.filter(dow => {
    const d = scheduleData.find(s => s.dayOfWeek === dow);
    return d && d.location !== 'Rest';
  });

  const resultsDiv = document.getElementById('workout-results');
  resultsDiv.innerHTML = '';

  const r = await Bridge.call('generateWeekWorkouts', {});
  btn.disabled = false;
  progress.classList.add('hidden');

  if (!r.ok) { showError('exercise-error', r.data?.detail || r.data || 'Generation failed.'); return; }

  const results = Array.isArray(r.data) ? r.data : [];
  results.forEach((item, idx) => {
    if (item.error) {
      showError('exercise-error', `Day ${idx + 1}: ${item.error}`);
    } else if (item.suggestion) {
      renderWorkoutResult(item.suggestion, false);
    }
  });
}

function renderWorkoutResult(suggestion, prepend = false) {
  const resultsDiv = document.getElementById('workout-results');
  if (!resultsDiv) return;
  const dayName = suggestion.dayOfWeek != null ? DAY_NAMES[suggestion.dayOfWeek] : '';
  const categoryColors = { Cardio: 'bg-blue-100 text-blue-700', Strength: 'bg-purple-100 text-purple-700', Flexibility: 'bg-green-100 text-green-700' };
  const badgeClass = categoryColors[suggestion.category] || 'bg-gray-100 text-gray-700';

  const div = document.createElement('div');
  div.className = 'bg-white rounded-xl shadow p-5';
  div.dataset.suggestionId = suggestion.id;
  div.innerHTML = `
    <div class="flex justify-between items-start mb-3">
      <div>
        <h3 class="font-bold text-gray-800 text-lg">${escHtml(dayName)} — ${escHtml(suggestion.location)}</h3>
        <p class="text-xs text-gray-400">${fmtDateTime(suggestion.createdAt)}</p>
      </div>
      <div class="flex items-center gap-2">
        <span class="text-xs font-semibold px-2 py-1 rounded-full ${badgeClass}">${escHtml(suggestion.category)}</span>
        <button onclick="deleteWorkoutResult(${suggestion.id}, this)" class="text-gray-300 hover:text-red-500 transition text-lg leading-none" title="Delete">&times;</button>
      </div>
    </div>
    <div class="prose prose-sm max-w-none text-gray-700">${md(suggestion.content)}</div>`;

  if (prepend && resultsDiv.firstChild) {
    resultsDiv.insertBefore(div, resultsDiv.firstChild);
  } else {
    resultsDiv.appendChild(div);
  }
}

async function deleteWorkoutResult(id, btn) {
  btn.disabled = true;
  const r = await Bridge.call('deleteExerciseHistory', { id });
  if (!r.ok) { btn.disabled = false; showError('exercise-error', 'Failed to delete workout.'); return; }
  const card = document.querySelector(`[data-suggestion-id="${id}"]`);
  if (card) card.remove();
}

// ─── MEALS ────────────────────────────────────────────────────────────────────
async function renderMeals() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-gray-800">Meal Log</h1>
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">

        <!-- Left: entry form + table -->
        <div class="space-y-4">
          <div class="bg-white rounded-xl shadow p-5">
            <h2 class="text-lg font-semibold mb-4">Add Meal</h2>
            <div id="meal-error"></div>
            <form id="meal-form" class="space-y-3">
              <div class="flex gap-3">
                <div class="flex-1">
                  <label class="block text-sm text-gray-600 mb-1">Type</label>
                  <select id="meal-type" class="border rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-indigo-400">
                    <option>Breakfast</option><option>Lunch</option><option>Dinner</option><option>Snack</option>
                  </select>
                </div>
                <div class="w-28">
                  <label class="block text-sm text-gray-600 mb-1">Calories</label>
                  <input id="meal-cal" type="number" min="0" class="border rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-indigo-400" placeholder="opt.">
                </div>
              </div>
              <div>
                <label class="block text-sm text-gray-600 mb-1">Description</label>
                <input id="meal-desc" type="text" required class="border rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-indigo-400" placeholder="e.g. Oatmeal with berries">
              </div>
              <div>
                <label class="block text-sm text-gray-600 mb-1">Notes</label>
                <input id="meal-notes" type="text" class="border rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-indigo-400" placeholder="optional">
              </div>
              <button type="submit" class="w-full bg-indigo-600 hover:bg-indigo-700 text-white py-2 rounded-lg font-medium transition">Add Meal</button>
            </form>
          </div>

          <div class="bg-white rounded-xl shadow p-5">
            <div id="meal-table-wrap"></div>
          </div>
        </div>

        <!-- Right: AI advice -->
        <div class="bg-white rounded-xl shadow p-5 flex flex-col gap-4">
          <h2 class="text-lg font-semibold">Ask Gemini for Nutrition Advice</h2>
          <div id="advice-error"></div>
          <div class="flex flex-col gap-2">
            <textarea id="advice-question" rows="3" class="border rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" placeholder="e.g. What should I eat after my workout?"></textarea>
            <button id="btn-ask" onclick="askAdvice()" class="bg-green-600 hover:bg-green-700 text-white py-2 rounded-lg font-medium transition">Ask Gemini</button>
          </div>
          <div id="advice-panel" class="prose prose-sm text-gray-700 flex-1 overflow-y-auto"></div>
        </div>
      </div>
    </div>`;

  await loadMealTable();

  document.getElementById('meal-form').addEventListener('submit', async e => {
    e.preventDefault();
    clearError('meal-error');
    const mealType = document.getElementById('meal-type').value;
    const description = document.getElementById('meal-desc').value.trim();
    const cal = document.getElementById('meal-cal').value;
    const calories = cal ? parseInt(cal) : null;
    const notes = document.getElementById('meal-notes').value.trim() || null;
    const r = await Bridge.call('addMeal', { mealType, description, calories, notes });
    if (!r.ok) { showError('meal-error', r.data?.detail || r.data); return; }
    document.getElementById('meal-desc').value = '';
    document.getElementById('meal-cal').value = '';
    document.getElementById('meal-notes').value = '';
    await loadMealTable();
  });
}

async function loadMealTable() {
  const wrap = document.getElementById('meal-table-wrap');
  if (!wrap) return;
  const r = await Bridge.call('getTodayMeals');
  if (!r.ok) { wrap.innerHTML = '<p class="text-red-500 text-sm">Failed to load meals.</p>'; return; }
  const meals = r.data;
  const totalCal = meals.reduce((s, m) => s + (m.calories || 0), 0);

  wrap.innerHTML = `
    <div class="flex justify-between items-center mb-3">
      <h3 class="font-semibold text-gray-700">Today's Meals</h3>
      <span class="text-sm font-medium text-indigo-600">${totalCal > 0 ? totalCal + ' cal total' : ''}</span>
    </div>
    ${meals.length === 0 ? '<p class="text-gray-400 text-sm">No meals logged today.</p>' : `
    <div class="space-y-2">
      ${meals.map(m => `
        <div class="flex justify-between items-start border rounded-lg p-3 hover:bg-gray-50">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2">
              <span class="text-xs font-semibold bg-indigo-100 text-indigo-700 rounded-full px-2 py-0.5">${escHtml(m.mealType)}</span>
              ${m.calories ? `<span class="text-xs text-gray-400">${m.calories} cal</span>` : ''}
            </div>
            <div class="text-sm font-medium text-gray-800 mt-1">${escHtml(m.description)}</div>
            ${m.notes ? `<div class="text-xs text-gray-400">${escHtml(m.notes)}</div>` : ''}
          </div>
          <button onclick="deleteMeal(${m.id})" class="text-red-400 hover:text-red-600 text-xs ml-2 flex-shrink-0">Delete</button>
        </div>`).join('')}
    </div>`}`;
}

async function deleteMeal(id) {
  if (!confirm('Delete this meal?')) return;
  const r = await Bridge.call('deleteMeal', { id });
  if (!r.ok) { showError('meal-error', r.data?.detail || r.data); return; }
  await loadMealTable();
}

async function askAdvice() {
  clearError('advice-error');
  const question = document.getElementById('advice-question').value.trim();
  if (!question) return;
  const btn = document.getElementById('btn-ask');
  const panel = document.getElementById('advice-panel');
  btn.disabled = true;
  btn.textContent = 'Asking…';
  panel.innerHTML = '<div class="animate-pulse text-gray-400 text-sm">Generating advice…</div>';

  const r = await Bridge.call('getMealAdvice', { question });
  btn.disabled = false;
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
let aiHistoryData = [];
let selectedAiId = null;

async function renderHistory() {
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="space-y-4">
      <h1 class="text-2xl font-bold text-gray-800">AI History</h1>
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 h-[70vh]">

        <!-- Left: filter + list -->
        <div class="flex flex-col gap-3">
          <div class="flex gap-1 flex-wrap" id="history-filters">
            ${['All','Exercise','Meal','General'].map(f =>
              `<button onclick="setHistoryFilter('${f}')" data-filter="${f}"
                class="text-xs px-3 py-1.5 rounded-full font-medium transition
                  ${f === aiHistoryFilter ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}">${f}</button>`
            ).join('')}
          </div>
          <div id="history-list" class="flex-1 overflow-y-auto space-y-2 pr-1"></div>
        </div>

        <!-- Right: detail pane -->
        <div class="lg:col-span-2 bg-white rounded-xl shadow overflow-y-auto">
          <div id="history-detail" class="p-5 h-full flex items-center justify-center text-gray-400 text-sm">
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
  if (!aiHistoryData.length) { list.innerHTML = '<p class="text-gray-400 text-sm">No entries.</p>'; return; }
  const typeColors = { Exercise: 'bg-purple-100 text-purple-700', Meal: 'bg-green-100 text-green-700', General: 'bg-blue-100 text-blue-700' };
  list.innerHTML = aiHistoryData.map(item => {
    const badge = typeColors[item.promptType] || 'bg-gray-100 text-gray-600';
    const isSelected = item.id === selectedAiId;
    return `
      <div onclick="selectAiHistory(${item.id})" class="cursor-pointer border rounded-lg p-3 hover:bg-indigo-50 transition ${isSelected ? 'border-indigo-400 bg-indigo-50' : 'bg-white'}">
        <div class="flex justify-between items-start gap-2">
          <span class="text-xs font-semibold px-2 py-0.5 rounded-full ${badge}">${escHtml(item.promptType)}</span>
          <button onclick="event.stopPropagation();deleteAiHistory(${item.id})" class="text-red-400 hover:text-red-600 text-xs">Delete</button>
        </div>
        <p class="text-xs text-gray-500 mt-1">${fmtDateTime(item.createdAt)}</p>
        <p class="text-sm text-gray-700 mt-1 truncate">${escHtml(item.prompt.substring(0, 80))}…</p>
      </div>`;
  }).join('');
}

function setHistoryFilter(filter) {
  aiHistoryFilter = filter;
  selectedAiId = null;
  document.querySelectorAll('[data-filter]').forEach(btn => {
    const active = btn.dataset.filter === filter;
    btn.className = `text-xs px-3 py-1.5 rounded-full font-medium transition ${active ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`;
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
    <div class="space-y-4">
      <div class="flex justify-between items-start">
        <div>
          <h3 class="font-bold text-gray-800">${escHtml(item.promptType)} Prompt</h3>
          <p class="text-xs text-gray-400">${fmtDateTime(item.createdAt)} · ${escHtml(item.model)}</p>
        </div>
        <div class="text-right text-xs text-gray-400">
          <div>${item.inputTokens} in / ${item.outputTokens} out tokens</div>
        </div>
      </div>
      <div>
        <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Prompt</h4>
        <pre class="text-xs bg-gray-50 border rounded p-3 overflow-x-auto whitespace-pre-wrap text-gray-700">${escHtml(item.prompt)}</pre>
      </div>
      <div>
        <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Response</h4>
        <div class="prose prose-sm max-w-none bg-gray-50 border rounded p-3 text-gray-700">${md(item.response)}</div>
      </div>
    </div>`;
}

async function deleteAiHistory(id) {
  if (!confirm('Delete this AI history entry?')) return;
  const r = await Bridge.call('deleteAiHistory', { id });
  if (!r.ok) {
    alert(r.data?.detail || r.data || 'Failed to delete.');
    return;
  }
  if (selectedAiId === id) {
    selectedAiId = null;
    const detail = document.getElementById('history-detail');
    if (detail) detail.innerHTML = '<div class="h-full flex items-center justify-center text-gray-400 text-sm">Select an entry to view details</div>';
  }
  await loadAiHistory();
}

// ─── Init ─────────────────────────────────────────────────────────────────────
navigate('dashboard');
