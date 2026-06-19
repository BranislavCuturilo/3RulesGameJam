# 3TD Cheatsheet Website Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

> **GIT NOTE (user instruction):** The user has requested **no git operations** (no add/commit/branch/push) until they explicitly say otherwise. Every "Checkpoint" step below is a verification gate only — do **not** run git. When the user later authorizes commits, each checkpoint is a natural commit boundary.

**Goal:** Build a static, dependency-free reference website ("cheatsheet") for 3TD with four pages — Towers, Rules, Enemies, Maps — each a searchable, filterable grid of cards with click-to-open detail modals.

**Architecture:** Plain HTML/CSS/JS. Four HTML pages share one stylesheet and two scripts. `core.js` holds pure logic (effect/targeting labels, search + filter predicates) and is unit-tested with Node's built-in `node:test`. `app.js` does DOM rendering and reads a single `data/data.json`. Content is generated once from the Unity `.prefab`/`.asset` files by a Python script and committed as `data.json`; the site itself ships no build step and no dependencies.

**Tech Stack:** HTML5, CSS3 (CSS Grid, custom properties), vanilla ES2017 JS (UMD pattern for browser+Node), Python 3 (one-time data generator), Node 22 `node:test` (dev-only unit tests), Google Fonts (Cinzel + a sans).

---

## File Structure

```
cheatsheet/
  index.html            # Towers (landing)
  rules.html            # Rules
  enemies.html          # Enemies
  maps.html             # Maps
  css/style.css         # shared dark-fantasy theme
  js/core.js            # pure logic (browser + node), unit-tested
  js/app.js             # DOM: fetch data, render grid + modal, wire search/filters
  data/data.json        # generated content (the site reads this)
  tools/build_data.py   # one-time generator: Unity assets -> data.json
  tests/core.test.js    # node:test unit tests for core.js
  img/
    towers/cannon.png, machinegun.png
    enemies/slime.png, slime_fast.png, slime_tank.png
    rules/attackmode.png ... tanky.png
    maps/map1.jpg
    placeholder.svg
  README.md             # how to update content
```

**Responsibility boundaries:**
- `data.json` — single source of content. Nothing else hardcodes game numbers.
- `core.js` — pure, DOM-free, side-effect-free helpers. Testable in isolation.
- `app.js` — all DOM/rendering; depends on `core.js` + `data.json`.
- `style.css` — all theme/layout.
- `build_data.py` — dev tool; not loaded by the site.

---

## Reference Data (already extracted — use these exact values)

**EffectType enum** (index → label): `0 None, 1 AOE Front, 2 AOE Impact, 3 Slow, 4 AOE Slow, 5 DOT, 6 DOT AOE, 7 Stun, 8 AOE Stun`.

**`FireRate` is seconds between shots (cooldown) — lower = faster.** The UI must label it accordingly.

**Towers** (base stats + 3 upgrade tiers; targeting; effect):

| id | name | targeting | effect | base D/FR/R/Cost | tier2 D/FR/R/Cost | tier3 D/FR/R/Cost |
|----|------|-----------|--------|------------------|-------------------|-------------------|
| cannon | Cannon Tower | Strongest | AOE Impact (r3) | 50 / 1.5 / 5 / 100 | 75 / 1.5 / 5.5 / 300 | 200 / 1 / 6 / 1000 |
| machinegun | Machine Gun Tower | First | None | 20 / 0.5 / 4.5 / 125 | 35 / 0.4 / 4.5 / 200 | 80 / 0.25 / 4.5 / 600 |
| shotgun | Shotgun Tower | First | None | 25 / 8 / 1 / 100 | 50 / 0.9 / 7 / 125 | 100 / 0.85 / 7 / 200 |
| sniper | Sniper Tower | Last | None | 200 / 7 / 10 / 75 | 250 / 5 / 12.5 / 200 | 400 / 4 / 15 / 1100 |
| frost | Frost Tower | Strongest | AOE Slow (r3, dur1, str0.8) | 25 / 1.25 / 4 / 250 | 40 / 1.25 / 4.25 / 250 | 60 / 1.25 / 4.5 / 350 |
| poison | Poison Tower | First | DOT (dur5, dot5) | 25 / 8 / 1 / 100 | 10 / 0.7 / 5 / 150 | 25 / 0.6 / 5 / 325 |
| tesla | Tesla Tower | Last | Stun (dur0.15) | 15 / 0.6 / 6.5 / 200 | 25 / 0.45 / 6.5 / 175 | 40 / 0.25 / 6.5 / 400 |

> Note: base stats above equal tier-1 stats except Poison/Shotgun, whose prefab base differs slightly from tier-1 — `build_data.py` uses tier-1 as the displayed base for consistency; the table’s "base" column is informational.

**Enemies:**

| id | name | type | health | moveSpeed | moneyValue | leakDamage | image |
|----|------|------|--------|-----------|------------|------------|-------|
| normal | Slime | normal | 100 | 1.5 | 10 | 2 | enemies/slime.png |
| fast | Fast Slime | fast | 50 | 2.75 | 15 | 1 | enemies/slime_fast.png |
| tank | Tank Slime | tank | 500 | 0.5 | 50 | 5 | enemies/slime_tank.png |

**Maps:** one map — id `map1`, name `Map 1`, image `maps/map1.jpg`.

**Rule → card image mapping** (resolved via GUID):
`AttackMode→Necromancer_1.png, Boss→Necromancer_8.png, Default→Necromancer_5.png, FAST&Furious→Necromancer_17.png, FUN→Necromancer_22.png, NonStop→Necromancer_9.png, Quickly→Necromancer_21.png, SlowDown→Necromancer_13.png, Tanky→Necromancer_12.png`

**Tower art available:** only `Cannon.png` and `MG.png` map to real towers (→ cannon, machinegun). Shotgun/Sniper/Frost/Poison/Tesla use `placeholder.svg` until art is added. Enemies all use slime sprites (`slime_idle1.png`, `slime_move.png`, `slime_jump.png`). Map uses `Assets/map.jpg`.

The full rule content (names, segments, per-level descriptions + changed multipliers) is produced by `build_data.py` parsing `Assets/RULES/*.asset` — see Task 1.

---

## Task 1: Generate `data.json` from Unity assets

**Files:**
- Create: `cheatsheet/tools/build_data.py`
- Create (generated): `cheatsheet/data/data.json`

- [ ] **Step 1: Create the generator script**

Create `cheatsheet/tools/build_data.py` with this exact content:

```python
#!/usr/bin/env python3
"""One-time generator: read Unity prefab/asset files -> cheatsheet/data/data.json.
Run from the repo root:  python cheatsheet/tools/build_data.py
Re-run after changing game data, then refresh the site."""
import glob, json, os, re, sys

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(REPO, "cheatsheet", "data", "data.json")

EFFECTS = ["None","AOE Front","AOE Impact","Slow","AOE Slow","DOT","DOT AOE","Stun","AOE Stun"]

# --- Towers (literal: stats verified from Assets/Prefabs/Towers/*.prefab) ---
TOWERS = [
 {"id":"cannon","name":"Cannon Tower","image":"img/towers/cannon.png","targeting":"Strongest",
  "effect":{"name":"AOE Impact","radius":3,"duration":0,"strength":0,"dot":0},
  "tiers":[{"level":1,"damage":50,"fireRate":1.5,"range":5,"cost":100},
           {"level":2,"damage":75,"fireRate":1.5,"range":5.5,"cost":300},
           {"level":3,"damage":200,"fireRate":1,"range":6,"cost":1000}]},
 {"id":"machinegun","name":"Machine Gun Tower","image":"img/towers/machinegun.png","targeting":"First",
  "effect":{"name":"None","radius":0,"duration":0,"strength":0,"dot":0},
  "tiers":[{"level":1,"damage":20,"fireRate":0.5,"range":4.5,"cost":125},
           {"level":2,"damage":35,"fireRate":0.4,"range":4.5,"cost":200},
           {"level":3,"damage":80,"fireRate":0.25,"range":4.5,"cost":600}]},
 {"id":"shotgun","name":"Shotgun Tower","image":"img/placeholder.svg","targeting":"First",
  "effect":{"name":"None","radius":0,"duration":0,"strength":0,"dot":0},
  "tiers":[{"level":1,"damage":25,"fireRate":8,"range":1,"cost":100},
           {"level":2,"damage":50,"fireRate":0.9,"range":7,"cost":125},
           {"level":3,"damage":100,"fireRate":0.85,"range":7,"cost":200}]},
 {"id":"sniper","name":"Sniper Tower","image":"img/placeholder.svg","targeting":"Last",
  "effect":{"name":"None","radius":0,"duration":0,"strength":0,"dot":0},
  "tiers":[{"level":1,"damage":200,"fireRate":7,"range":10,"cost":75},
           {"level":2,"damage":250,"fireRate":5,"range":12.5,"cost":200},
           {"level":3,"damage":400,"fireRate":4,"range":15,"cost":1100}]},
 {"id":"frost","name":"Frost Tower","image":"img/placeholder.svg","targeting":"Strongest",
  "effect":{"name":"AOE Slow","radius":3,"duration":1,"strength":0.8,"dot":0},
  "tiers":[{"level":1,"damage":25,"fireRate":1.25,"range":4,"cost":250},
           {"level":2,"damage":40,"fireRate":1.25,"range":4.25,"cost":250},
           {"level":3,"damage":60,"fireRate":1.25,"range":4.5,"cost":350}]},
 {"id":"poison","name":"Poison Tower","image":"img/placeholder.svg","targeting":"First",
  "effect":{"name":"DOT","radius":0,"duration":5,"strength":0,"dot":5},
  "tiers":[{"level":1,"damage":25,"fireRate":8,"range":1,"cost":100},
           {"level":2,"damage":10,"fireRate":0.7,"range":5,"cost":150},
           {"level":3,"damage":25,"fireRate":0.6,"range":5,"cost":325}]},
 {"id":"tesla","name":"Tesla Tower","image":"img/placeholder.svg","targeting":"Last",
  "effect":{"name":"Stun","radius":0,"duration":0.15,"strength":0,"dot":0},
  "tiers":[{"level":1,"damage":15,"fireRate":0.6,"range":6.5,"cost":200},
           {"level":2,"damage":25,"fireRate":0.45,"range":6.5,"cost":175},
           {"level":3,"damage":40,"fireRate":0.25,"range":6.5,"cost":400}]},
]
for t in TOWERS:
    t["base"] = {k: t["tiers"][0][k] for k in ("damage","fireRate","range","cost")}

ENEMIES = [
 {"id":"normal","name":"Slime","type":"normal","image":"img/enemies/slime.png",
  "health":100,"moveSpeed":1.5,"moneyValue":10,"leakDamage":2},
 {"id":"fast","name":"Fast Slime","type":"fast","image":"img/enemies/slime_fast.png",
  "health":50,"moveSpeed":2.75,"moneyValue":15,"leakDamage":1},
 {"id":"tank","name":"Tank Slime","type":"tank","image":"img/enemies/slime_tank.png",
  "health":500,"moveSpeed":0.5,"moneyValue":50,"leakDamage":5},
]

MAPS = [
 {"id":"map1","name":"Map 1","image":"img/maps/map1.jpg",
  "description":"The default map. Slimes follow a fixed winding path from spawn to your base; place towers along it."},
]

RULE_IMG = {
 "AttackMode":"attackmode","Boss":"boss","Default":"default","FAST&Furious":"fastfurious",
 "FUN":"fun","NonStop":"nonstop","Quickly":"quickly","SlowDown":"slowdown","Tanky":"tanky",
}

RULE_DEFAULT = {
 'enemySpeedMultiplier':1,'enemyHealthMultiplier':1,'enemyQuantityMultiplier':1,
 'enemyMoneyValueMultiplier':1,'enemyLeakDamageMultiplier':1,'useFixedEnemyCount':0,
 'fixedEnemyCount':0,'useFixedEnemyHealth':0,'fixedEnemyHealth':0,'useFixedLeakDamage':0,
 'fixedLeakDamage':1,'useFixedSpawnDelay':0,'towerFireRateMultiplier':1,'towerDamageMultiplier':1,
 'towerRangeMultiplier':1,'towerPlacementCostMultiplier':1,'forcedTargetingMode':0,
 'dotDamageMultiplier':1,'slowEffectMultiplier':1,'stunDurationMultiplier':1,'aoeRadiusMultiplier':1,
 'projectileSpeedMultiplier':1,'effectDurationMultiplier':1,'waveCompleteMoneyMultiplier':1,
 'enemyKillMoneyMultiplier':1,'upgradeDiscountMultiplier':1,'useFixedWaveCompleteMoney':0,
 'fixedWaveCompleteMoney':0,'useFixedEnemyKillMoney':0,'fixedEnemyKillMoney':0,
}
ENEMY_KEYS = {'enemySpeedMultiplier','enemyHealthMultiplier','enemyQuantityMultiplier',
 'enemyMoneyValueMultiplier','enemyLeakDamageMultiplier','useFixedEnemyCount','fixedEnemyCount',
 'useFixedEnemyHealth','fixedEnemyHealth','useFixedLeakDamage','fixedLeakDamage','useFixedSpawnDelay'}
TOWER_KEYS = {'towerFireRateMultiplier','towerDamageMultiplier','towerRangeMultiplier',
 'towerPlacementCostMultiplier','forcedTargetingMode','dotDamageMultiplier','slowEffectMultiplier',
 'stunDurationMultiplier','aoeRadiusMultiplier','projectileSpeedMultiplier','effectDurationMultiplier'}
ECON_KEYS = {'waveCompleteMoneyMultiplier','enemyKillMoneyMultiplier','upgradeDiscountMultiplier',
 'useFixedWaveCompleteMoney','fixedWaveCompleteMoney','useFixedEnemyKillMoney','fixedEnemyKillMoney'}

def load_asset(path):
    import yaml
    lines = [l for l in open(path, encoding='utf-8').read().splitlines() if not l.startswith('%')]
    raw = re.sub(r'--- !u!\d+ &\d+', '', "\n".join(lines)).replace('!u!','')
    return yaml.safe_load(raw)['MonoBehaviour']

def build_rules():
    rules = []
    for path in sorted(glob.glob(os.path.join(REPO,'Assets','RULES','*.asset'))):
        name = os.path.splitext(os.path.basename(path))[0]
        if name not in RULE_IMG:
            continue
        mb = load_asset(path)
        rid = RULE_IMG[name]
        levels, segset = [], set()
        for i, lvl in enumerate(mb.get('progressionLevels') or []):
            if not lvl:
                continue
            desc = (lvl.get('levelDescription') or '').strip()
            changes = {}
            for k, dv in RULE_DEFAULT.items():
                v = lvl.get(k, dv)
                if isinstance(v, float) and v == int(v): v = int(v)
                if v != dv:
                    changes[k] = v
                    if k in ENEMY_KEYS: segset.add('Enemy')
                    elif k in TOWER_KEYS: segset.add('Tower')
                    elif k in ECON_KEYS: segset.add('Economy')
            if not desc and not changes:
                continue
            levels.append({"level": i+1, "description": desc, "changes": changes})
        rules.append({"id": rid, "name": mb.get('baseRuleSetName', name).strip(),
                      "image": f"img/rules/{rid}.png", "segments": sorted(segset), "levels": levels})
    return rules

def main():
    data = {"meta":{"game":"3TD — Endless Tower Defense","generated":"2026-06-19"},
            "towers":TOWERS,"rules":build_rules(),"enemies":ENEMIES,"maps":MAPS}
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT,"w",encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f"Wrote {OUT}: {len(data['towers'])} towers, {len(data['rules'])} rules, "
          f"{len(data['enemies'])} enemies, {len(data['maps'])} maps")

if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Run the generator**

Run: `python cheatsheet/tools/build_data.py`
Expected output: `Wrote .../data.json: 7 towers, 9 rules, 3 enemies, 1 maps`
(Requires PyYAML — already used earlier in this repo. If missing: `pip install pyyaml`.)

- [ ] **Step 3: Validate the generated JSON**

Run:
```bash
python -c "import json;d=json.load(open('cheatsheet/data/data.json',encoding='utf-8'));print(len(d['towers']),len(d['rules']),len(d['enemies']),len(d['maps']));print([r['name'] for r in d['rules']]);print([t['id'] for t in d['towers']])"
```
Expected: `7 9 3 1` then the 9 rule names (AttackMode, BOSS, Default, Fast & Furious, FUN, Non STOP, Quickly, Slow Down, Tanky) then the 7 tower ids.

- [ ] **Step 4: Checkpoint** — data.json exists and validates. (No git per user instruction.)

---

## Task 2: Pure logic in `core.js` (TDD)

**Files:**
- Create: `cheatsheet/js/core.js`
- Test: `cheatsheet/tests/core.test.js`

- [ ] **Step 1: Write the failing tests**

Create `cheatsheet/tests/core.test.js`:

```javascript
const test = require('node:test');
const assert = require('node:assert');
const TD = require('../js/core.js');

test('formatFireRate labels seconds and marks faster-is-lower', () => {
  assert.strictEqual(TD.formatFireRate(1.5), '1.5s / shot');
  assert.strictEqual(TD.formatFireRate(0.25), '0.25s / shot');
});

test('matchesSearch is case-insensitive substring on name', () => {
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, 'can'), true);
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, 'TOWER'), true);
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, 'frost'), false);
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, ''), true);
});

test('towerMatchesFilters checks effect and targeting (empty = pass)', () => {
  const t = { effect: { name: 'AOE Impact' }, targeting: 'Strongest' };
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: '', targeting: '' }), true);
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: 'AOE Impact', targeting: '' }), true);
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: 'DOT', targeting: '' }), false);
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: '', targeting: 'First' }), false);
});

test('ruleMatchesFilters passes when rule has the chosen segment', () => {
  const r = { segments: ['Enemy', 'Tower'] };
  assert.strictEqual(TD.ruleMatchesFilters(r, { segment: '' }), true);
  assert.strictEqual(TD.ruleMatchesFilters(r, { segment: 'Tower' }), true);
  assert.strictEqual(TD.ruleMatchesFilters(r, { segment: 'Economy' }), false);
});

test('enemyMatchesFilters checks type', () => {
  const e = { type: 'fast' };
  assert.strictEqual(TD.enemyMatchesFilters(e, { type: '' }), true);
  assert.strictEqual(TD.enemyMatchesFilters(e, { type: 'fast' }), true);
  assert.strictEqual(TD.enemyMatchesFilters(e, { type: 'tank' }), false);
});

test('uniqueValues collects sorted distinct values via accessor', () => {
  const items = [{ targeting: 'First' }, { targeting: 'Last' }, { targeting: 'First' }];
  assert.deepStrictEqual(TD.uniqueValues(items, x => x.targeting), ['First', 'Last']);
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `node --test cheatsheet/tests/`
Expected: FAIL — `Cannot find module '../js/core.js'`.

- [ ] **Step 3: Implement `core.js`**

Create `cheatsheet/js/core.js`:

```javascript
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `node --test cheatsheet/tests/`
Expected: PASS — all 6 tests pass.

- [ ] **Step 5: Checkpoint** — core logic tested and green. (No git.)

---

## Task 3: Shared theme `css/style.css`

**Files:**
- Create: `cheatsheet/css/style.css`

- [ ] **Step 1: Create the stylesheet**

Create `cheatsheet/css/style.css`:

```css
:root {
  --bg: #15100c;
  --bg-panel: #221a13;
  --bg-card: #2b2118;
  --gold: #c9a227;
  --gold-soft: #e6c65c;
  --text: #ece3d2;
  --text-dim: #b6a489;
  --border: #4a3a26;
  --enemy: #b5483f;
  --tower: #3f7bb5;
  --economy: #58a05a;
  --radius: 12px;
  --shadow: 0 6px 18px rgba(0,0,0,.45);
}
* { box-sizing: border-box; }
body {
  margin: 0; background: var(--bg); color: var(--text);
  font-family: system-ui, "Segoe UI", Roboto, sans-serif; line-height: 1.5;
}
h1, h2, .title { font-family: "Cinzel", Georgia, serif; letter-spacing: .5px; }
a { color: var(--gold-soft); text-decoration: none; }

header.nav {
  position: sticky; top: 0; z-index: 10;
  display: flex; align-items: center; gap: 1.5rem;
  padding: .9rem 1.5rem; background: var(--bg-panel);
  border-bottom: 2px solid var(--gold); box-shadow: var(--shadow);
}
header.nav .brand { font-family: "Cinzel", serif; color: var(--gold-soft); font-size: 1.25rem; }
header.nav nav { display: flex; gap: 1rem; flex-wrap: wrap; }
header.nav nav a { color: var(--text-dim); padding: .25rem .5rem; border-radius: 6px; }
header.nav nav a.active, header.nav nav a:hover { color: var(--bg); background: var(--gold); }

main { max-width: 1200px; margin: 0 auto; padding: 1.5rem; }
.controls { display: flex; flex-wrap: wrap; gap: .75rem; align-items: center; margin-bottom: 1.25rem; }
.controls input[type="search"] {
  flex: 1 1 220px; min-width: 180px; padding: .6rem .8rem;
  background: var(--bg-card); border: 1px solid var(--border); border-radius: 8px; color: var(--text);
}
.chips { display: flex; flex-wrap: wrap; gap: .4rem; }
.chip {
  cursor: pointer; padding: .35rem .7rem; border-radius: 999px;
  background: var(--bg-card); border: 1px solid var(--border); color: var(--text-dim); font-size: .85rem;
}
.chip.active { background: var(--gold); color: var(--bg); border-color: var(--gold); }

.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1.1rem; }
.card {
  background: var(--bg-card); border: 1px solid var(--border); border-top: 3px solid var(--gold);
  border-radius: var(--radius); padding: 1rem; cursor: pointer;
  transition: transform .12s ease, box-shadow .12s ease, border-color .12s ease;
}
.card:hover { transform: translateY(-4px); box-shadow: var(--shadow); border-color: var(--gold-soft); }
.card .thumb {
  width: 100%; height: 130px; object-fit: contain; background: #0d0a07;
  border-radius: 8px; margin-bottom: .6rem;
}
.card .title { font-size: 1.05rem; color: var(--gold-soft); margin: 0 0 .35rem; }
.card .stats { display: grid; grid-template-columns: auto auto; gap: .15rem .6rem; font-size: .85rem; color: var(--text-dim); }
.card .stats b { color: var(--text); font-weight: 600; }
.tags { display: flex; gap: .35rem; flex-wrap: wrap; margin-top: .5rem; }
.tag { font-size: .72rem; padding: .15rem .5rem; border-radius: 999px; border: 1px solid var(--border); color: var(--text-dim); }
.tag.Enemy { color: var(--enemy); border-color: var(--enemy); }
.tag.Tower { color: var(--tower); border-color: var(--tower); }
.tag.Economy { color: var(--economy); border-color: var(--economy); }

.empty { color: var(--text-dim); padding: 2rem; text-align: center; }

.modal-backdrop {
  position: fixed; inset: 0; background: rgba(0,0,0,.7);
  display: none; align-items: center; justify-content: center; padding: 1rem; z-index: 20;
}
.modal-backdrop.open { display: flex; }
.modal {
  background: var(--bg-panel); border: 1px solid var(--gold); border-radius: var(--radius);
  max-width: 640px; width: 100%; max-height: 85vh; overflow: auto; padding: 1.5rem; box-shadow: var(--shadow);
}
.modal h2 { margin-top: 0; color: var(--gold-soft); }
.modal table { width: 100%; border-collapse: collapse; margin: .75rem 0; font-size: .9rem; }
.modal th, .modal td { border: 1px solid var(--border); padding: .4rem .55rem; text-align: center; }
.modal th { background: var(--bg-card); color: var(--gold-soft); }
.modal .close { float: right; cursor: pointer; color: var(--text-dim); font-size: 1.4rem; line-height: 1; }
.modal .level { border-left: 3px solid var(--gold); padding: .5rem .75rem; margin: .6rem 0; background: var(--bg-card); border-radius: 6px; }
.modal .level .lvl-name { color: var(--gold-soft); font-weight: 600; }
.modal .level p { margin: .3rem 0; white-space: pre-line; color: var(--text-dim); }
@media (max-width: 520px) { .card .stats { grid-template-columns: 1fr 1fr; } }
```

- [ ] **Step 2: Checkpoint** — stylesheet present (visual verification happens in Task 6). (No git.)

---

## Task 4: The four HTML pages

**Files:**
- Create: `cheatsheet/index.html`, `cheatsheet/rules.html`, `cheatsheet/enemies.html`, `cheatsheet/maps.html`

Each page is identical except `<title>`, the `data-page` attribute, and the active nav link.

- [ ] **Step 1: Create `cheatsheet/index.html`** (Towers, `data-page="towers"`):

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>3TD Cheatsheet — Towers</title>
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@500;700&display=swap" rel="stylesheet" />
  <link rel="stylesheet" href="css/style.css" />
</head>
<body data-page="towers">
  <header class="nav">
    <span class="brand">3TD Cheatsheet</span>
    <nav>
      <a href="index.html" class="active">Towers</a>
      <a href="rules.html">Rules</a>
      <a href="enemies.html">Enemies</a>
      <a href="maps.html">Maps</a>
    </nav>
  </header>
  <main>
    <h1>Towers</h1>
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
  <script src="js/core.js"></script>
  <script src="js/app.js"></script>
</body>
</html>
```

- [ ] **Step 2: Create `cheatsheet/rules.html`** — copy of index.html with these exact differences:
  - `<title>3TD Cheatsheet — Rules</title>`
  - `<body data-page="rules">`
  - In `<nav>`, move `class="active"` from the Towers link to the Rules link.
  - `<h1>Rules</h1>`

- [ ] **Step 3: Create `cheatsheet/enemies.html`** — copy of index.html with:
  - `<title>3TD Cheatsheet — Enemies</title>`
  - `<body data-page="enemies">`
  - `class="active"` on the Enemies link only.
  - `<h1>Enemies</h1>`

- [ ] **Step 4: Create `cheatsheet/maps.html`** — copy of index.html with:
  - `<title>3TD Cheatsheet — Maps</title>`
  - `<body data-page="maps">`
  - `class="active"` on the Maps link only.
  - `<h1>Maps</h1>`

- [ ] **Step 5: Checkpoint** — four pages exist with correct markers. (No git.)

---

## Task 5: `app.js` — load data and render grids

**Files:**
- Create: `cheatsheet/js/app.js`

- [ ] **Step 1: Create `app.js` with data loading + per-category card rendering**

Create `cheatsheet/js/app.js`:

```javascript
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

  // filters + modal are added in later tasks; default no-ops keep render working now.
  function passesFilters() { return true; }
  function openModal() {}

  // expose hooks so later tasks can replace them
  window.__cheat = { get items() { return items; }, render, filterState,
    set passesFilters(fn) { passesFilters = fn; }, set openModal(fn) { openModal = fn; },
    els: { filtersEl, backdrop, modal, searchEl } };

  fetch('data/data.json')
    .then(r => r.json())
    .then(data => { items = data[key] || []; render(); if (searchEl) searchEl.addEventListener('input', render); })
    .catch(err => { emptyEl.hidden = false; emptyEl.textContent = 'Failed to load data.json'; console.error(err); });
})();
```

> Note: `passesFilters`/`openModal` are intentionally no-ops here and are replaced in Tasks 6–7 by reassigning through `window.__cheat`. This keeps each task independently runnable.

- [ ] **Step 2: Verify in a browser (towers grid renders)**

Run a local server from the repo root (file:// blocks `fetch`):
`python -m http.server 8000`
Open `http://localhost:8000/cheatsheet/index.html`.
Expected: a grid of 7 tower cards with names, stats, and effect/targeting tags. (Images may be broken until Task 8 — that is fine; placeholder fallback applies once Task 8 adds `placeholder.svg`.)

- [ ] **Step 3: Verify the other three pages render**

Open `rules.html` (9 cards), `enemies.html` (3 cards), `maps.html` (1 card).
Expected: each shows the right count with category-appropriate fields.

- [ ] **Step 4: Checkpoint** — all four grids render from data.json. (No git.)

---

## Task 6: Detail modal

**Files:**
- Modify: `cheatsheet/js/app.js` (replace the `openModal` no-op and add modal builders + close handlers)

- [ ] **Step 1: Add modal builders and wire close behavior**

In `app.js`, immediately **before** the `window.__cheat = {...}` line, insert:

```javascript
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
```

- [ ] **Step 2: Activate the modal**

In `app.js`, change the no-op line `function openModal() {}` to delegate to `showModal`:

Replace:
```javascript
  function openModal() {}
```
with:
```javascript
  function openModal(it) { showModal(it); }
```

- [ ] **Step 3: Verify in browser**

Reload `http://localhost:8000/cheatsheet/index.html`. Click a tower card.
Expected: modal opens with a 3-row tier table + effect/targeting line. Close via ×, backdrop click, and Esc all work.
Check `rules.html`: clicking a rule shows its level descriptions; `enemies.html` shows the stat table; `maps.html` shows the map image + description.

- [ ] **Step 4: Checkpoint** — modal works on all four pages. (No git.)

---

## Task 7: Search + category filters

**Files:**
- Modify: `cheatsheet/js/app.js` (replace `passesFilters` no-op, build filter chips)

- [ ] **Step 1: Add filter config + chip builder + predicate**

In `app.js`, replace the line `function passesFilters() { return true; }` with:

```javascript
  // Per-page filter definitions: which chip groups to show and how to test an item.
  const FILTERS = {
    towers: [
      { key: 'effect', label: 'Effect', values: it => TD.uniqueValues(items, x => x.effect && x.effect.name) },
      { key: 'targeting', label: 'Targeting', values: it => TD.uniqueValues(items, x => x.targeting) },
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
```

- [ ] **Step 2: Call `buildFilters()` after data loads**

In `app.js`, in the `fetch(...).then(data => {...})` callback, add `buildFilters();` right after `items = data[key] || [];` so the line reads:

```javascript
    .then(data => { items = data[key] || []; buildFilters(); render(); if (searchEl) searchEl.addEventListener('input', render); })
```

- [ ] **Step 3: Verify in browser**

Reload `index.html`. Expected: chips appear for each Effect (None, AOE Impact, AOE Slow, DOT, Stun) and Targeting (First, Last, Strongest). Clicking a chip filters the grid; clicking again clears it. Typing in search narrows by name and combines with chips (AND). `rules.html` shows Enemy/Tower/Economy chips; `enemies.html` shows type chips; `maps.html` shows no chips.

- [ ] **Step 4: Re-run unit tests (no regressions in core.js)**

Run: `node --test cheatsheet/tests/`
Expected: PASS — all 6 tests still pass.

- [ ] **Step 5: Checkpoint** — search + filters work across pages. (No git.)

---

## Task 8: Images

**Files:**
- Create: `cheatsheet/img/placeholder.svg`
- Copy: tower / enemy / rule / map art into `cheatsheet/img/...`

- [ ] **Step 1: Create the placeholder**

Create `cheatsheet/img/placeholder.svg`:

```svg
<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
  <rect width="200" height="200" fill="#0d0a07"/>
  <rect x="8" y="8" width="184" height="184" rx="14" fill="none" stroke="#4a3a26" stroke-width="3"/>
  <text x="100" y="108" font-family="Georgia, serif" font-size="20" fill="#c9a227"
        text-anchor="middle">No image</text>
</svg>
```

- [ ] **Step 2: Copy art into place**

Run these copies from the repo root (PowerShell):

```powershell
New-Item -ItemType Directory -Force -Path cheatsheet/img/towers, cheatsheet/img/enemies, cheatsheet/img/rules, cheatsheet/img/maps | Out-Null

Copy-Item "Assets/Assets/Towers/Cannon.png" "cheatsheet/img/towers/cannon.png" -Force
Copy-Item "Assets/Assets/Towers/MG.png"     "cheatsheet/img/towers/machinegun.png" -Force

Copy-Item "Assets/Slimes/slime_idle1.png" "cheatsheet/img/enemies/slime.png" -Force
Copy-Item "Assets/Slimes/slime_move.png"  "cheatsheet/img/enemies/slime_fast.png" -Force
Copy-Item "Assets/Slimes/slime_jump.png"  "cheatsheet/img/enemies/slime_tank.png" -Force

Copy-Item "Assets/map.jpg" "cheatsheet/img/maps/map1.jpg" -Force

Copy-Item "Assets/RULES/Necromancer_1.png"  "cheatsheet/img/rules/attackmode.png" -Force
Copy-Item "Assets/RULES/Necromancer_8.png"  "cheatsheet/img/rules/boss.png" -Force
Copy-Item "Assets/RULES/Necromancer_5.png"  "cheatsheet/img/rules/default.png" -Force
Copy-Item "Assets/RULES/Necromancer_17.png" "cheatsheet/img/rules/fastfurious.png" -Force
Copy-Item "Assets/RULES/Necromancer_22.png" "cheatsheet/img/rules/fun.png" -Force
Copy-Item "Assets/RULES/Necromancer_9.png"  "cheatsheet/img/rules/nonstop.png" -Force
Copy-Item "Assets/RULES/Necromancer_21.png" "cheatsheet/img/rules/quickly.png" -Force
Copy-Item "Assets/RULES/Necromancer_13.png" "cheatsheet/img/rules/slowdown.png" -Force
Copy-Item "Assets/RULES/Necromancer_12.png" "cheatsheet/img/rules/tanky.png" -Force
```

- [ ] **Step 2.5: Verify all referenced images exist**

Run:
```powershell
python -c "import json,os; d=json.load(open('cheatsheet/data/data.json',encoding='utf-8')); paths=[x['image'] for k in ('towers','rules','enemies','maps') for x in d[k]]; missing=[p for p in paths if not os.path.exists(os.path.join('cheatsheet',p))]; print('MISSING:',missing if missing else 'none')"
```
Expected: `MISSING: none` (towers shotgun/sniper/frost/poison/tesla reference `img/placeholder.svg`, which exists).

- [ ] **Step 3: Verify in browser**

Reload all four pages. Expected: Cannon and Machine Gun show real sprites; other towers show the "No image" placeholder; enemies show slime sprites; rules show their card art; the map shows `map1.jpg`. No broken-image icons.

- [ ] **Step 4: Checkpoint** — images present, nothing broken. (No git.)

---

## Task 9: README + final verification

**Files:**
- Create: `cheatsheet/README.md`

- [ ] **Step 1: Write the README**

Create `cheatsheet/README.md`:

```markdown
# 3TD Cheatsheet

A static reference site for **3TD — Endless Tower Defense**: Towers, Rules, Enemies, Maps.
Pure HTML/CSS/JS — no build step, no dependencies.

## Run locally
`fetch` needs http(s), so use a tiny server (not file://):

    python -m http.server 8000
    # then open http://localhost:8000/cheatsheet/index.html

## Hosting
Upload the `cheatsheet/` folder to any static host (GitHub Pages, itch.io, Netlify).

## Updating content
All content lives in `data/data.json`. Two ways to update:
1. **Edit `data/data.json` by hand** (e.g. tweak a stat or description).
2. **Regenerate from the Unity project:** `python cheatsheet/tools/build_data.py`
   (reads `Assets/Prefabs/Towers/*.prefab`, `Assets/Prefabs/Enemy/*.prefab`, `Assets/RULES/*.asset`).
   Tower/enemy/map values are literals in the script; rule data is parsed from the assets.
   Requires `pyyaml` (`pip install pyyaml`).

## Adding images
Drop files in `img/<category>/` and point the matching `image` field in `data.json` at them.
Missing images fall back to `img/placeholder.svg`. Towers without art today:
shotgun, sniper, frost, poison, tesla.

## Tests
Pure logic in `js/core.js` is unit-tested with Node's built-in runner:

    node --test cheatsheet/tests/
```

- [ ] **Step 2: Full regression — unit tests**

Run: `node --test cheatsheet/tests/`
Expected: PASS — 6/6.

- [ ] **Step 3: Full manual verification pass**

With `python -m http.server 8000` running, confirm on each page:
- [ ] Towers: 7 cards; search "frost" → 1 card; Effect=DOT chip → Poison only; modal shows tier table.
- [ ] Rules: 9 cards; Segment=Economy chip → Boss/FUN/NonStop/Quickly/Tanky; modal lists level descriptions.
- [ ] Enemies: 3 cards; Type=tank → Tank Slime; modal shows stat table.
- [ ] Maps: 1 card; modal shows map image + description; no chips.
- [ ] Responsive: narrow the window — cards reflow/stack; nav stays usable.

- [ ] **Step 4: Checkpoint** — feature complete and verified. (No git; awaiting user authorization to commit.)

---

## Self-Review Notes (planner)

- **Spec coverage:** 4 pages (Tasks 4–7) ✓; card grid + images (5, 8) ✓; modal details (6) ✓; search + filters (7) ✓; data.json from Unity assets (1) ✓; dark-fantasy theme (3) ✓; vanilla/no-build ✓; English ✓; README/update path (9) ✓. Non-goals respected (no backend/login/i18n/auto-sync).
- **Type consistency:** `data.json` field names (`base`, `tiers`, `effect.name`, `targeting`, `segments`, `levels[].description`, enemy `type/health/moveSpeed/moneyValue/leakDamage`, map `image/description`) are used identically in `app.js` renderers/modals and `core.js` predicates. Filter state keys (`effect`, `targeting`, `segment`, `type`) match between `FILTERS`, `filterState`, and the `core.js` `*MatchesFilters` functions.
- **No placeholders:** every step contains runnable content; the only intentional no-ops (`passesFilters`/`openModal`) are explicitly replaced in later tasks.
- **Git:** all commit steps replaced with non-git checkpoints per the user's standing instruction.
```
