// Pure, DOM-free helpers shared by the browser (window.TD) and node:test.
(function (global) {
  function formatFireRate(fr) {
    return `${fr}s / shot`;
  }

  function matchesSearch(item, query) {
    if (!query) return true;
    return String(item.name || '').toLowerCase().includes(query.toLowerCase());
  }

  function towerMatchesFilters(t, f) {
    if (f.effect && (!t.effect || t.effect.name !== f.effect)) return false;
    if (f.targeting && t.targeting !== f.targeting) return false;
    return true;
  }

  function ruleMatchesFilters(r, f) {
    if (f.segment && !(r.segments || []).includes(f.segment)) return false;
    return true;
  }

  function enemyMatchesFilters(e, f) {
    if (f.type && e.type !== f.type) return false;
    return true;
  }

  function uniqueValues(items, accessor) {
    const set = new Set();
    items.forEach(i => { const v = accessor(i); if (v) set.add(v); });
    return Array.from(set).sort();
  }

  const TD = {
    formatFireRate, matchesSearch, towerMatchesFilters,
    ruleMatchesFilters, enemyMatchesFilters, uniqueValues,
  };

  if (typeof module !== 'undefined' && module.exports) module.exports = TD;
  else global.TD = TD;
})(typeof window !== 'undefined' ? window : globalThis);
