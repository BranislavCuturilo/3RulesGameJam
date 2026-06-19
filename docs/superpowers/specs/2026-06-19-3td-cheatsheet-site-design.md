# 3TD Cheatsheet Website — Design

**Date:** 2026-06-19
**Author:** Branislav Čuturilo (with Claude)
**Status:** Approved design, pending implementation plan

## Purpose

A static reference website ("cheatsheet") for the game **3TD — Endless Tower Defense**.
It serves players (and the developer) as a quick visual catalogue of all game content:
towers, rules, enemies, and maps. The look is inspired by card-catalogue pages such as the
Hearthstone cards page — a grid of cards/boxes with images and stat descriptions, dark fantasy theme.

Language: **English**. No backend, no build step, easy to host (GitHub Pages / itch.io / open locally).

## Goals & Success Criteria

- Four pages — Towers, Rules, Enemies, Maps — each a responsive grid of cards.
- Every card shows an image + key stats; clicking a card opens a modal with full details.
- Each page has a **search box** (filter by name) and **filter chips** (category-specific).
- All data is **accurate**, extracted once from the actual Unity `.prefab` / `.asset` files into a single `data.json`.
- Pure static HTML/CSS/JS (vanilla, no framework, no dependencies beyond Google Fonts).
- The site is data-driven: updating content later = editing `data.json` (and adding images).

## Non-Goals (YAGNI)

- No backend, database, login, or analytics.
- No automatic/continuous sync with Unity — `data.json` is a one-time snapshot, updated manually.
- No build tooling (no Node/Vite/Astro).
- No internationalization — English only.
- No editing UI — the cheatsheet is read-only.

## File Structure

A new top-level folder `cheatsheet/`, kept **outside** `Assets/` so the Unity editor does not import it.

```
cheatsheet/
  index.html        # Towers (landing page)
  rules.html        # Rules
  enemies.html      # Enemies
  maps.html         # Maps
  css/
    style.css       # shared dark-fantasy theme
  js/
    app.js          # shared: fetch data.json, render grid + modal, search + filters
  data/
    data.json       # all content, extracted from Unity assets
  img/
    towers/ ...     # copied tower sprites (+ placeholder for towers without art)
    enemies/ ...     # slime sprites
    rules/ ...      # rule card art
    maps/ ...       # map image(s)
    placeholder.svg # generic icon for missing art
  README.md         # how to update data.json and add images
```

All four HTML pages share the same `<header>` nav and load the same `style.css` and `app.js`.
Each page sets a small inline marker (e.g. `<body data-page="towers">`) so `app.js` knows which
category of `data.json` to render.

## Data Model (`data.json`)

Top-level object with four arrays. Values below are illustrative; real values are extracted
from the Unity assets during implementation.

```json
{
  "meta": { "game": "3TD — Endless Tower Defense", "generated": "2026-06-19" },
  "towers": [
    {
      "id": "cannon",
      "name": "Cannon Tower",
      "image": "img/towers/cannon.png",
      "targeting": "Strongest",
      "effect": "AOE Impact",
      "base":  { "damage": 50, "fireRate": 1.5, "range": 5,   "cost": 100 },
      "tiers": [
        { "level": 1, "damage": 50,  "fireRate": 1.5, "range": 5,   "cost": 100 },
        { "level": 2, "damage": 75,  "fireRate": 1.5, "range": 5.5, "cost": 300 },
        { "level": 3, "damage": 200, "fireRate": 1.0, "range": 6,   "cost": 1000 }
      ]
    }
  ],
  "rules": [
    {
      "id": "attackmode",
      "name": "AttackMode",
      "image": "img/rules/attackmode.png",
      "segments": ["Tower", "Enemy"],
      "description": "Short human-readable summary.",
      "levels": [
        { "level": 1, "description": "...", "key": { "towerDamageMultiplier": 1.2, "enemyHealthMultiplier": 1.1 } }
      ]
    }
  ],
  "enemies": [
    {
      "id": "fast",
      "name": "Fast Enemy",
      "image": "img/enemies/fast.png",
      "health": 30,
      "moveSpeed": 4,
      "moneyValue": 12,
      "leakDamage": 1,
      "resistances": []
    }
  ],
  "maps": [
    {
      "id": "map1",
      "name": "Map 1",
      "image": "img/maps/map1.jpg",
      "checkpoints": 0,
      "description": "..."
    }
  ]
}
```

### Source of truth (extraction map)

| Category | Source files | Fields pulled |
|----------|--------------|---------------|
| Towers   | `Assets/Prefabs/Towers/*.prefab` (CannonTower, MachineGunTower, ShotGunTower, SniperTower, FrostTower, PoisonTower, TeslaTower) | `TowerName`, base `Damage/FireRate/Range/Cost`, `TowerUpgrade` tier array, targeting (`First/Last/Strongest`), `TowerEffects.effectType` |
| Rules    | `Assets/RULES/*.asset` (AttackMode, Boss, Default, FAST&Furious, FUN, NonStop, Quickly, SlowDown, Tanky) | `baseRuleSetName`, `progressionLevels[]` (descriptions + non-default multipliers), which segments (enemy/tower/economy) are touched |
| Enemies  | `Assets/Prefabs/Enemy/*.prefab` (Enemy, FastEnemy, TankEnemy) | `Health`, `movespeed`, `EnemyMoneyValue`, `LeakDamage` |
| Maps     | `Assets/Assets/Level/Map.prefab` + `map.jpg` | name, checkpoint count, image |

The `EffectType` enum is read from the C# source to map numeric `effectType` → readable label
(Slow, DOT, Stun, AOE_Slow, DOT_AOE, AOE_Stun, AOE_Impact, AOE_Front).

Towers without finished sprite art (Sniper, Frost, Poison, Tesla, Shotgun) reference
`img/placeholder.svg` until art is added.

## Rendering & Interaction (`app.js`)

Single shared script, no framework:

1. On load, `fetch('data/data.json')`, read `body[data-page]`, pick the matching array.
2. **Render grid:** one card per item. Card content is category-specific (a small render
   function per category: `renderTowerCard`, `renderRuleCard`, `renderEnemyCard`, `renderMapCard`).
3. **Search:** an input filters visible cards by `name` (case-insensitive substring), live on keyup.
4. **Filters (chips):** category-specific, combined with search (AND):
   - Towers: by **effect type** and/or **targeting mode**.
   - Rules: by **segment** (Enemy / Tower / Economy).
   - Enemies: by **type** (normal / fast / tank).
   - Maps: none (single map for now) — chips hidden when not applicable.
5. **Modal:** clicking a card opens a centered modal with full details (all tiers for towers,
   all progression levels for rules, full stat block for enemies). Close via X, backdrop click, or Esc.
   One generic modal element reused; content filled per category.

Empty/!found states: if search yields nothing, show a small "No results" message.

## Visual Design (`style.css`)

- **Theme:** dark fantasy. Dark background (#1a1410-ish), gold/bronze accents, parchment-toned text.
- **Cards:** rounded panel with a colored border keyed to category meaning
  (tower border by effect family or tier strength; rule border by primary segment; enemy border by type).
  Hover: subtle lift + glow.
- **Typography:** display font for titles (e.g. Cinzel via Google Fonts), clean sans for body.
- **Layout:** CSS Grid `auto-fit minmax(220px, 1fr)`; responsive down to mobile (cards stack).
- **Nav:** sticky top bar with the four page links; active page highlighted.

## Images

A one-time copy of needed art from `Assets/` into `cheatsheet/img/`, organized by category, so the
site is self-contained and portable. A `placeholder.svg` covers any missing art. The README documents
the naming convention (`img/<category>/<id>.<ext>` matching the `image` field in `data.json`).

## Components & Boundaries

| Unit | Responsibility | Depends on |
|------|----------------|------------|
| `data.json` | Single source of content | (produced once from Unity assets) |
| `app.js` data loader | Fetch + select category | `data.json`, `body[data-page]` |
| Per-category render fns | Build card + modal HTML | data item shape |
| Search/filter module | Filter the in-memory list, re-render | render fns |
| `style.css` | All theming/layout | (none) |
| HTML pages | Structure + page marker + nav | css/js |

Each render function is independently understandable and testable with a sample data item.

## Testing / Verification

Since this is a static site with no framework:
- Open each of the 4 pages locally; confirm cards render from `data.json` with correct stats.
- Spot-check 2–3 values per category against the Unity source files.
- Verify search filters live; verify each filter chip; verify modal open/close (X, backdrop, Esc).
- Verify responsive layout (narrow viewport stacks cards).
- Verify placeholder image appears for art-less towers.

## Open Questions

None outstanding. Language = English; search + filters = included (confirmed).
