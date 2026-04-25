// Bridge API — maps named actions to HTTP calls
const Bridge = (() => {
  async function call(action, data) {
    const routes = {
      // Auth
      login:                { method: 'POST',   url: '/api/auth/login' },
      logout:               { method: 'POST',   url: '/api/auth/logout' },
      getMe:                { method: 'GET',    url: '/api/auth/me' },
      // Profile
      updateProfile:        { method: 'PUT',    url: '/api/profiles/{id}' },
      // Dashboard
      getDashboard:         { method: 'GET',    url: '/api/dashboard' },
      // Weight
      getWeightEntries:     { method: 'GET',    url: '/api/weight' },
      saveWeight:           { method: 'POST',   url: '/api/weight' },
      updateWeight:         { method: 'PUT',    url: '/api/weight/{id}' },
      deleteWeight:         { method: 'DELETE', url: '/api/weight/{id}' },
      // Schedule
      getSchedule:          { method: 'GET',    url: '/api/schedule' },
      saveSchedule:         { method: 'PUT',    url: '/api/schedule' },
      // Exercise
      generateDayWorkout:   { method: 'POST',   url: '/api/exercise/generate-day' },
      generateWeekWorkouts: { method: 'POST',   url: '/api/exercise/generate-week' },
      getExerciseHistory:   { method: 'GET',    url: '/api/exercise/history' },
      deleteExerciseHistory:{ method: 'DELETE', url: '/api/exercise/history/{id}' },
      // Meals
      getTodayMeals:        { method: 'GET',    url: '/api/meals/today' },
      addMeal:              { method: 'POST',   url: '/api/meals' },
      deleteMeal:           { method: 'DELETE', url: '/api/meals/{id}' },
      getMealAdvice:        { method: 'POST',   url: '/api/meals/advice' },
      // AI history
      getAiHistory:         { method: 'GET',    url: '/api/ai-history' },
      deleteAiHistory:      { method: 'DELETE', url: '/api/ai-history/{id}' },
      // Blood Pressure
      getBpEntries:         { method: 'GET',    url: '/api/bp' },
      addBpEntry:           { method: 'POST',   url: '/api/bp' },
      deleteBpEntry:        { method: 'DELETE', url: '/api/bp/{id}' },
    };

    const route = routes[action];
    if (!route) throw new Error(`Unknown action: ${action}`);

    let url = route.url;
    let body = undefined;

    if (data && data.id !== undefined) {
      url = url.replace('{id}', data.id);
    }

    if (route.method === 'GET' && data) {
      const params = new URLSearchParams();
      if (data.dayOfWeek !== undefined) params.set('dayOfWeek', data.dayOfWeek);
      if (data.type !== undefined) params.set('type', data.type);
      const qs = params.toString();
      if (qs) url += '?' + qs;
    }

    if (route.method !== 'GET' && route.method !== 'DELETE' && data) {
      if (Array.isArray(data)) {
        body = JSON.stringify(data);
      } else {
        const { id, ...rest } = data;
        if (Object.keys(rest).length) body = JSON.stringify(rest);
      }
    }

    const headers = {};
    if (body) headers['Content-Type'] = 'application/json';

    try {
      const res = await fetch(url, { method: route.method, headers, body, credentials: 'same-origin' });

      if (res.status === 401 && action !== 'login' && action !== 'getMe') {
        showLoginView();
        return { ok: false, status: 401, data: 'Session expired. Please log in again.' };
      }

      let responseData;
      const ct = res.headers.get('Content-Type') || '';
      if (ct.includes('application/json')) {
        responseData = await res.json();
      } else {
        responseData = await res.text();
      }

      return { ok: res.ok, status: res.status, data: responseData };
    } catch (err) {
      return { ok: false, status: 0, data: err.message };
    }
  }

  return { call };
})();
