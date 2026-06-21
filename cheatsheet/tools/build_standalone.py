#!/usr/bin/env python3
"""Build a single self-contained HTML file from the cheatsheet site.

Inlines data.json, CSS, JS, and every referenced image (as base64 data URIs)
into one file: cheatsheet/3td-cheatsheet.html. The result opens with a plain
double-click (file://) — no web server needed — so it can be shared as a single
download.

Run from anywhere:  python cheatsheet/tools/build_standalone.py
"""
import base64
import json
import mimetypes
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))  # .../cheatsheet
OUT = os.path.join(ROOT, "3td-cheatsheet.html")

PAGES = [
    ("towers", "Towers"),
    ("rules", "Rules"),
    ("enemies", "Enemies"),
    ("maps", "Maps"),
]


def read(path):
    with open(os.path.join(ROOT, path), "r", encoding="utf-8") as f:
        return f.read()


def data_uri(rel_path):
    """Return a base64 data URI for an image referenced relative to cheatsheet/."""
    abs_path = os.path.join(ROOT, rel_path)
    mime, _ = mimetypes.guess_type(abs_path)
    if mime is None:
        mime = "application/octet-stream"
    with open(abs_path, "rb") as f:
        b64 = base64.b64encode(f.read()).decode("ascii")
    return f"data:{mime};base64,{b64}"


def main():
    data = json.loads(read("data/data.json"))
    css = read("css/style.css")
    core_js = read("js/core.js")

    # Collect every image path the data references, plus the placeholder, and
    # inline each one. Then rewrite the "image" fields to point at data URIs.
    image_cache = {}

    def inline(rel):
        if rel not in image_cache:
            try:
                image_cache[rel] = data_uri(rel)
            except FileNotFoundError:
                image_cache[rel] = data_uri("img/placeholder.svg")
        return image_cache[rel]

    placeholder = inline("img/placeholder.svg")

    for key, _ in PAGES:
        for item in data.get(key, []):
            if item.get("image"):
                item["image"] = inline(item["image"])

    data_json = json.dumps(data, ensure_ascii=False, separators=(",", ":"))

    html = STANDALONE_TEMPLATE.format(
        css=css,
        core_js=core_js,
        data_json=data_json,
        placeholder=placeholder,
        nav="".join(
            f'<a href="#" data-nav="{key}">{label}</a>' for key, label in PAGES
        ),
    )

    with open(OUT, "w", encoding="utf-8") as f:
        f.write(html)

    size_mb = os.path.getsize(OUT) / (1024 * 1024)
    print(f"Wrote {OUT} ({size_mb:.1f} MB)")


# The app logic below is adapted from js/app.js but driven by an in-page page
# switcher instead of separate .html files and a fetch() call.
STANDALONE_TEMPLATE = r"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>3TD Cheatsheet</title>
  <style>
{css}
  </style>
</head>
<body>
  <header class="nav">
    <span class="brand">3TD Cheatsheet</span>
    <nav id="nav">{nav}</nav>
  </header>
  <main>
    <h1 id="page-title">Towers</h1>
    <div class="controls">
      <input type="search" id="search" placeholder="Search by name…" aria-label="Search" />
      <div class="chips" id="filters"></div>
    </div>
    <div class="grid" id="grid"></div>
    <div class="empty" id="empty" hidden>No results.</div>
  </main>
  <div class="modal-backdrop" id="modal-backdrop">
    <div class="modal" id="modal" role="dialog" aria-modal="true"></div>
  </div>

  <script>
{core_js}
  </script>
  <script>
  (function () {{
    const DATA = {data_json};
    const PLACEHOLDER = "{placeholder}";
    const TITLES = {{ towers: "Towers", rules: "Rules", enemies: "Enemies", maps: "Maps" }};

    const gridEl = document.getElementById('grid');
    const emptyEl = document.getElementById('empty');
    const searchEl = document.getElementById('search');
    const filtersEl = document.getElementById('filters');
    const backdrop = document.getElementById('modal-backdrop');
    const modal = document.getElementById('modal');
    const navEl = document.getElementById('nav');
    const titleEl = document.getElementById('page-title');

    let key = 'towers';
    let items = [];
    let filterState = {{}};

    const esc = s => String(s).replace(/[&<>"]/g, c => ({{ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;' }}[c]));
    const onerr = `onerror="this.onerror=null;this.src='${{PLACEHOLDER}}'"`;

    function towerCard(t) {{
      const b = t.base;
      return `<img class="thumb" src="${{esc(t.image)}}" alt="${{esc(t.name)}}" loading="lazy" ${{onerr}}/>
        <h3 class="title">${{esc(t.name)}}</h3>
        <div class="stats">
          <span>Damage</span><b>${{b.damage}}</b>
          <span>Range</span><b>${{b.range}}</b>
          <span>Fire rate</span><b>${{TD.formatFireRate(b.fireRate)}}</b>
          <span>Cost</span><b>${{b.cost}}</b>
        </div>
        <div class="tags"><span class="tag">${{esc(t.effect.name)}}</span><span class="tag">${{esc(t.targeting)}}</span></div>`;
    }}
    function ruleCard(r) {{
      const tags = (r.segments.length ? r.segments : ['Neutral'])
        .map(s => `<span class="tag ${{esc(s)}}">${{esc(s)}}</span>`).join('');
      return `<img class="thumb" src="${{esc(r.image)}}" alt="${{esc(r.name)}}" loading="lazy" ${{onerr}}/>
        <h3 class="title">${{esc(r.name)}}</h3>
        <div class="stats"><span>Levels</span><b>${{r.levels.length}}</b></div>
        <div class="tags">${{tags}}</div>`;
    }}
    function enemyCard(e) {{
      return `<img class="thumb" src="${{esc(e.image)}}" alt="${{esc(e.name)}}" loading="lazy" ${{onerr}}/>
        <h3 class="title">${{esc(e.name)}}</h3>
        <div class="stats">
          <span>Health</span><b>${{e.health}}</b>
          <span>Speed</span><b>${{e.moveSpeed}}</b>
          <span>Reward</span><b>${{e.moneyValue}}</b>
          <span>Leak dmg</span><b>${{e.leakDamage}}</b>
        </div>
        <div class="tags"><span class="tag">${{esc(e.type)}}</span></div>`;
    }}
    function mapCard(m) {{
      return `<img class="thumb" src="${{esc(m.image)}}" alt="${{esc(m.name)}}" loading="lazy" ${{onerr}}/>
        <h3 class="title">${{esc(m.name)}}</h3>
        <div class="stats"><span>Type</span><b>Single path</b></div>`;
    }}
    const CARD = {{ towers: towerCard, rules: ruleCard, enemies: enemyCard, maps: mapCard }};

    function render() {{
      const q = searchEl ? searchEl.value : '';
      const visible = items.filter(it => TD.matchesSearch(it, q) && passesFilters(it));
      gridEl.innerHTML = '';
      visible.forEach(it => {{
        const card = document.createElement('div');
        card.className = 'card';
        card.innerHTML = CARD[key](it);
        card.addEventListener('click', () => showModal(it));
        gridEl.appendChild(card);
      }});
      emptyEl.hidden = visible.length !== 0;
    }}

    const FILTERS = {{
      towers: [
        {{ key: 'effect', label: 'Effect', values: () => TD.uniqueValues(items, x => x.effect && x.effect.name) }},
        {{ key: 'targeting', label: 'Targeting', values: () => TD.uniqueValues(items, x => x.targeting) }},
      ],
      rules: [
        {{ key: 'segment', label: 'Segment', values: () => ['Enemy', 'Tower', 'Economy'] }},
      ],
      enemies: [
        {{ key: 'type', label: 'Type', values: () => TD.uniqueValues(items, x => x.type) }},
      ],
      maps: [],
    }};
    const PREDICATE = {{
      towers: (it) => TD.towerMatchesFilters(it, filterState),
      rules: (it) => TD.ruleMatchesFilters(it, filterState),
      enemies: (it) => TD.enemyMatchesFilters(it, filterState),
      maps: () => true,
    }};
    function passesFilters(it) {{ return PREDICATE[key](it); }}

    function buildFilters() {{
      filtersEl.innerHTML = '';
      (FILTERS[key] || []).forEach(group => {{
        group.values().forEach(val => {{
          const chip = document.createElement('span');
          chip.className = 'chip';
          chip.textContent = val;
          chip.dataset.group = group.key;
          chip.addEventListener('click', () => {{
            if (filterState[group.key] === val) {{ delete filterState[group.key]; chip.classList.remove('active'); }}
            else {{
              filterState[group.key] = val;
              Array.from(filtersEl.children).forEach(c => {{ if (c.dataset.group === group.key) c.classList.remove('active'); }});
              chip.classList.add('active');
            }}
            render();
          }});
          filtersEl.appendChild(chip);
        }});
      }});
    }}

    function towerModal(t) {{
      const rows = t.tiers.map(x =>
        `<tr><td>${{x.level}}</td><td>${{x.damage}}</td><td>${{TD.formatFireRate(x.fireRate)}}</td><td>${{x.range}}</td><td>${{x.cost}}</td></tr>`).join('');
      const eff = t.effect.name === 'None' ? 'None'
        : `${{esc(t.effect.name)}}${{t.effect.radius ? ` · radius ${{t.effect.radius}}` : ''}}`
          + `${{t.effect.duration ? ` · ${{t.effect.duration}}s` : ''}}${{t.effect.dot ? ` · ${{t.effect.dot}} dot` : ''}}`;
      return `<h2>${{esc(t.name)}}</h2>
        <p><b>Targeting:</b> ${{esc(t.targeting)}} &nbsp; <b>Effect:</b> ${{eff}}</p>
        <table><thead><tr><th>Tier</th><th>Damage</th><th>Fire rate</th><th>Range</th><th>Cost</th></tr></thead>
        <tbody>${{rows}}</tbody></table>
        <p class="hint">Fire rate is seconds between shots — lower is faster.</p>`;
    }}
    function ruleModal(r) {{
      const tags = (r.segments.length ? r.segments : ['Neutral'])
        .map(s => `<span class="tag ${{esc(s)}}">${{esc(s)}}</span>`).join(' ');
      const levels = r.levels.map(l =>
        `<div class="level"><div class="lvl-name">Level ${{l.level}}</div><p>${{esc(l.description || '—')}}</p></div>`).join('');
      return `<h2>${{esc(r.name)}}</h2><div class="tags">${{tags}}</div>${{levels}}`;
    }}
    function enemyModal(e) {{
      return `<h2>${{esc(e.name)}}</h2>
        <table><tbody>
          <tr><th>Type</th><td>${{esc(e.type)}}</td></tr>
          <tr><th>Health</th><td>${{e.health}}</td></tr>
          <tr><th>Move speed</th><td>${{e.moveSpeed}}</td></tr>
          <tr><th>Kill reward</th><td>${{e.moneyValue}}</td></tr>
          <tr><th>Leak damage</th><td>${{e.leakDamage}}</td></tr>
        </tbody></table>`;
    }}
    function mapModal(m) {{
      return `<h2>${{esc(m.name)}}</h2>
        <img class="thumb" style="height:240px" src="${{esc(m.image)}}" alt="${{esc(m.name)}}" ${{onerr}}/>
        <p>${{esc(m.description || '')}}</p>`;
    }}
    const MODAL = {{ towers: towerModal, rules: ruleModal, enemies: enemyModal, maps: mapModal }};

    function showModal(it) {{
      modal.innerHTML = `<span class="close" id="modal-close" aria-label="Close">×</span>` + MODAL[key](it);
      backdrop.classList.add('open');
      document.getElementById('modal-close').addEventListener('click', closeModal);
    }}
    function closeModal() {{ backdrop.classList.remove('open'); }}
    backdrop.addEventListener('click', e => {{ if (e.target === backdrop) closeModal(); }});
    document.addEventListener('keydown', e => {{ if (e.key === 'Escape') closeModal(); }});

    function switchPage(next) {{
      key = next;
      items = DATA[key] || [];
      filterState = {{}};
      titleEl.textContent = TITLES[key];
      if (searchEl) searchEl.value = '';
      Array.from(navEl.children).forEach(a => a.classList.toggle('active', a.dataset.nav === key));
      buildFilters();
      render();
    }}

    navEl.addEventListener('click', e => {{
      const a = e.target.closest('[data-nav]');
      if (!a) return;
      e.preventDefault();
      switchPage(a.dataset.nav);
    }});
    if (searchEl) searchEl.addEventListener('input', render);

    switchPage('towers');
  }})();
  </script>
</body>
</html>
"""


if __name__ == "__main__":
    main()
