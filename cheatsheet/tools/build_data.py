#!/usr/bin/env python3
"""One-time generator: read Unity prefab/asset files -> cheatsheet/data/data.json.
Run from the repo root:  python cheatsheet/tools/build_data.py
Re-run after changing game data, then refresh the site."""
import glob, json, os, re

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(REPO, "cheatsheet", "data", "data.json")

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
    with open(path, encoding='utf-8') as fh:
        lines = [l for l in fh.read().splitlines() if not l.startswith('%')]
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
