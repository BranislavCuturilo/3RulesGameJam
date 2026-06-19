// DOM layer: fetch data.json, render the grid for this page, wire search/filters/modal.
(function () {
  const page = document.body.dataset.page;              // 'towers' | 'rules' | 'enemies' | 'maps'
  const key = page;                                     // matches data.json array names
  const gridEl = document.getElementById('grid');
  const emptyEl = document.getElementById('empty');
  const searchEl = document.getElementById('search');
  const filtersEl = document.getElementById('filters');
  const backdrop = document.getElementById('modal-backdrop');
  const modal = document.getElementById('modal');

  let items = [];
  const filterState = {};                               // page-specific filter selections

  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;' }[c]));

  // ---- card renderers (one per category) ----
  function towerCard(t) {
    const b = t.base;
    return `<img class="thumb" src="${esc(t.image)}" alt="${esc(t.name)}" loading="lazy"
              onerror="this.src='img/placeholder.svg'"/>
      <h3 class="title">${esc(t.name)}</h3>
      <div class="stats">
        <span>Damage</span><b>${b.damage}</b>
        <span>Range</span><b>${b.range}</b>
        <span>Fire rate</span><b>${TD.formatFireRate(b.fireRate)}</b>
        <span>Cost</span><b>${b.cost}</b>
      </div>
      <div class="tags"><span class="tag">${esc(t.effect.name)}</span><span class="tag">${esc(t.targeting)}</span></div>`;
  }
  function ruleCard(r) {
    const tags = (r.segments.length ? r.segments : ['Neutral'])
      .map(s => `<span class="tag ${esc(s)}">${esc(s)}</span>`).join('');
    return `<img class="thumb" src="${esc(r.image)}" alt="${esc(r.name)}" loading="lazy"
              onerror="this.src='img/placeholder.svg'"/>
      <h3 class="title">${esc(r.name)}</h3>
      <div class="stats"><span>Levels</span><b>${r.levels.length}</b></div>
      <div class="tags">${tags}</div>`;
  }
  function enemyCard(e) {
    return `<img class="thumb" src="${esc(e.image)}" alt="${esc(e.name)}" loading="lazy"
              onerror="this.src='img/placeholder.svg'"/>
      <h3 class="title">${esc(e.name)}</h3>
      <div class="stats">
        <span>Health</span><b>${e.health}</b>
        <span>Speed</span><b>${e.moveSpeed}</b>
        <span>Reward</span><b>${e.moneyValue}</b>
        <span>Leak dmg</span><b>${e.leakDamage}</b>
      </div>
      <div class="tags"><span class="tag">${esc(e.type)}</span></div>`;
  }
  function mapCard(m) {
    return `<img class="thumb" src="${esc(m.image)}" alt="${esc(m.name)}" loading="lazy"
              onerror="this.src='img/placeholder.svg'"/>
      <h3 class="title">${esc(m.name)}</h3>
      <div class="stats"><span>Type</span><b>Single path</b></div>`;
  }
  const CARD = { towers: towerCard, rules: ruleCard, enemies: enemyCard, maps: mapCard };

  function render() {
    const q = searchEl ? searchEl.value : '';
    const visible = items.filter(it => TD.matchesSearch(it, q) && passesFilters(it));
    gridEl.innerHTML = '';
    visible.forEach((it, idx) => {
      const card = document.createElement('div');
      card.className = 'card';
      card.innerHTML = CARD[key](it);
      card.addEventListener('click', () => openModal(it));
      gridEl.appendChild(card);
    });
    emptyEl.hidden = visible.length !== 0;
  }

  // Per-page filter definitions: which chip groups to show and how to test an item.
  const FILTERS = {
    towers: [
      { key: 'effect', label: 'Effect', values: () => TD.uniqueValues(items, x => x.effect && x.effect.name) },
      { key: 'targeting', label: 'Targeting', values: () => TD.uniqueValues(items, x => x.targeting) },
    ],
    rules: [
      { key: 'segment', label: 'Segment', values: () => ['Enemy', 'Tower', 'Economy'] },
    ],
    enemies: [
      { key: 'type', label: 'Type', values: () => TD.uniqueValues(items, x => x.type) },
    ],
    maps: [],
  };
  const PREDICATE = {
    towers: (it) => TD.towerMatchesFilters(it, filterState),
    rules: (it) => TD.ruleMatchesFilters(it, filterState),
    enemies: (it) => TD.enemyMatchesFilters(it, filterState),
    maps: () => true,
  };
  function passesFilters(it) { return PREDICATE[key](it); }

  function buildFilters() {
    if (!filtersEl) return;
    filtersEl.innerHTML = '';
    (FILTERS[key] || []).forEach(group => {
      group.values().forEach(val => {
        const chip = document.createElement('span');
        chip.className = 'chip';
        chip.textContent = val;
        chip.addEventListener('click', () => {
          if (filterState[group.key] === val) { delete filterState[group.key]; chip.classList.remove('active'); }
          else {
            filterState[group.key] = val;
            Array.from(filtersEl.children).forEach(c => { if (c.dataset.group === group.key) c.classList.remove('active'); });
            chip.classList.add('active');
          }
          render();
        });
        chip.dataset.group = group.key;
        filtersEl.appendChild(chip);
      });
    });
  }
  function openModal(it) { showModal(it); }

  function towerModal(t) {
    const rows = t.tiers.map(x =>
      `<tr><td>${x.level}</td><td>${x.damage}</td><td>${TD.formatFireRate(x.fireRate)}</td><td>${x.range}</td><td>${x.cost}</td></tr>`).join('');
    const eff = t.effect.name === 'None' ? 'None'
      : `${esc(t.effect.name)}${t.effect.radius ? ` · radius ${t.effect.radius}` : ''}`
        + `${t.effect.duration ? ` · ${t.effect.duration}s` : ''}${t.effect.dot ? ` · ${t.effect.dot} dot` : ''}`;
    return `<h2>${esc(t.name)}</h2>
      <p><b>Targeting:</b> ${esc(t.targeting)} &nbsp; <b>Effect:</b> ${eff}</p>
      <table><thead><tr><th>Tier</th><th>Damage</th><th>Fire rate</th><th>Range</th><th>Cost</th></tr></thead>
      <tbody>${rows}</tbody></table>
      <p class="hint">Fire rate is seconds between shots — lower is faster.</p>`;
  }
  function ruleModal(r) {
    const tags = (r.segments.length ? r.segments : ['Neutral'])
      .map(s => `<span class="tag ${esc(s)}">${esc(s)}</span>`).join(' ');
    const levels = r.levels.map(l =>
      `<div class="level"><div class="lvl-name">Level ${l.level}</div><p>${esc(l.description || '—')}</p></div>`).join('');
    return `<h2>${esc(r.name)}</h2><div class="tags">${tags}</div>${levels}`;
  }
  function enemyModal(e) {
    return `<h2>${esc(e.name)}</h2>
      <table><tbody>
        <tr><th>Type</th><td>${esc(e.type)}</td></tr>
        <tr><th>Health</th><td>${e.health}</td></tr>
        <tr><th>Move speed</th><td>${e.moveSpeed}</td></tr>
        <tr><th>Kill reward</th><td>${e.moneyValue}</td></tr>
        <tr><th>Leak damage</th><td>${e.leakDamage}</td></tr>
      </tbody></table>`;
  }
  function mapModal(m) {
    return `<h2>${esc(m.name)}</h2>
      <img class="thumb" style="height:240px" src="${esc(m.image)}" alt="${esc(m.name)}" onerror="this.src='img/placeholder.svg'"/>
      <p>${esc(m.description || '')}</p>`;
  }
  const MODAL = { towers: towerModal, rules: ruleModal, enemies: enemyModal, maps: mapModal };

  function showModal(it) {
    modal.innerHTML = `<span class="close" id="modal-close" aria-label="Close">×</span>` + MODAL[key](it);
    backdrop.classList.add('open');
    document.getElementById('modal-close').addEventListener('click', closeModal);
  }
  function closeModal() { backdrop.classList.remove('open'); }
  backdrop.addEventListener('click', e => { if (e.target === backdrop) closeModal(); });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') closeModal(); });

  fetch('data/data.json')
    .then(r => r.json())
    .then(data => { items = data[key] || []; buildFilters(); render(); if (searchEl) searchEl.addEventListener('input', render); })
    .catch(err => { emptyEl.hidden = false; emptyEl.textContent = 'Failed to load data.json'; console.error(err); });
})();
