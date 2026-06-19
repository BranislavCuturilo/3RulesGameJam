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
