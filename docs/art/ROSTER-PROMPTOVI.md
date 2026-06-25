# 3TD — Roster + PixelLab promptovi (svi likovi)

Tema: slovenska/srpska mitologija. Stil: **Blasphemous** (mračni gotički HD pixel art),
zagasita paleta + zlatni/krvavi akcenti. Alat: **PixelLab** (`create_character` + `animate_character`).

## Zajednička podešavanja (za sve likove)
- `mode`: **v3** (kvalitet kao Svarog), `view`: **low top-down**, `body_type`: humanoid (osim Aždaje)
- `size`: bogovi/Drekavac **64**, Psoglav **80**, Aždaja **128**
- Stilski cue u opisu: `dark gothic pixel art, somber palette with gold accents`
- Generacije: `create v3` = 2/lik; `animate` = 1/pravac (8 pravaca = 8). Procena celog rostera ≈ **~280 gen** → 1 mesec Tier 1 pretplate pokriva (vidi `pixellab.md`).

### Animacije — pravilo
- **Caster/ranged bogovi**: attack = template **`fireball`** (jeftino, svih 8 pravaca) ili v3 „casting".
- **Melee bogovi** (oružje): attack = v3 custom „swinging/slashing ...", **`directions` = svih 8** (v3 custom inače radi samo jug!). Alternativa: template `surprise-uppercut`/`high-kick`.
- **Idle** za sve: template **`breathing-idle`** (svih 8 pravaca).
- **Čudovišta** dodatno: **walk** (`walking`/`running-6-frames`) + **death** (`falling-back-death`).

---

## ⚙️ MEHANIKA (iz koda — `Tower.cs` / `TowerEffects.cs`)
Toweri **NE stvaraju i NE leče** — samo **gađaju (ranged projektil)** ili **udaraju (melee/instant šteta)**, uz opcioni efekt.
Postojeći efekti: `Slow`, `DOT`, `Stun`, `AOE_Slow`, `DOT_AOE`, `AOE_Stun`, `AOE_Impact` (splash), `AOE_Front` (konus ispred). **Nema:** summon, heal/buff, knockback, chain, multi-smer.

> Ovo je merodavno — ignoriši svaki "summon/aura/heal" iz CSV-a. Mapiranje:

| Bog | Dostava | Efekt | Animacija napada |
|---|---|---|---|
| Perun | Ranged | `AOE_Stun` | `fireball` |
| Svarog | **Melee** | `AOE_Impact`+`DOT` | v3 „swinging hammer" (8 dir) |
| Dažbog | Ranged | — (DPS) | `fireball` / v3 „spear thrust" |
| Morana | Ranged | `Slow`/`AOE_Slow` | `fireball` |
| Veles | Ranged | `DOT_AOE` | `fireball` (otrov, NE summon) |
| Stribog | Ranged | `AOE_Front` | v3 „wind gust" (8 dir) |
| Jarilo | **Melee** | `DOT` | v3 „quick sickle slash" (8 dir) |
| Mokoš | Ranged | `Stun`/`AOE_Slow` | `fireball` (zemlja/korenje, NE heal) |
| Svetovid | **Melee** | `AOE_Front` | v3 „great sword sweep" (8 dir) |
| Triglav | Ranged | `AOE_Impact` | `fireball` (arkana) |

> Ext (kasnije, ako proširite kod): Perun pravi chain, Stribog knockback, Svetovid 4-smerni napad, Triglav switch-forms.

---

# 🏛️ TOWERI (bogovi)

## 1. Perun — Lightning, DPS (chain lightning)
**Description:**
```
Perun the Slavic thunder god, mighty bearded warrior deity in ornate gilded gothic armor,
holding a great battle axe crackling with lightning, electric blue sparks, storm-grey cloak,
dark gothic pixel art, somber palette with gold and electric blue accents
```
**Animacije:** idle `breathing-idle` · attack `fireball` (čita se kao bacanje munje)

## 2. Svarog — Fire, Support ✅ (već napravljen)
`character_id = 796eb7bf-58b8-4a53-8000-54cdbcffdf45`
**Description:**
```
Svarog the Slavic god of fire and the celestial forge, solemn bearded warrior deity in
ornate gilded golden gothic plate armor, holding a glowing forge war hammer, dark crimson cloak,
glowing embers, dark gothic pixel art, somber palette with gold and deep crimson accents
```
**Animacije:** idle `breathing-idle` · attack v3 custom „swinging the war hammer downward" (directions = svih 8)

## 3. Dažbog — Light, DPS (solarni snajper)
**Description:**
```
Dazhbog the Slavic sun god, radiant bearded deity in golden gothic armor, glowing solar crown,
holding a shining sun spear, warm golden light, dark gothic pixel art, somber palette with bright gold accents
```
**Animacije:** idle `breathing-idle` · attack v3 custom „thrusting the spear forward" (dir = svih 8) ili `fireball`

## 4. Morana — Ice, Magic (freeze/usporavanje)
**Description:**
```
Morana the Slavic goddess of death and winter, pale gaunt sorceress in frost-covered gothic gown,
icy crown, holding a frozen staff, cold blue-white frost, dark gothic pixel art,
somber palette with pale ice, silver and faint gold accents
```
**Animacije:** idle `breathing-idle` · attack `fireball` (čita se kao bacanje mraza)

## 5. Veles — Shadow, RANGED (otrovni oblak / `DOT_AOE`) — BEZ summona
**Description:**
```
Veles the Slavic god of the underworld and magic, horned shaman deity in dark tattered robes,
holding a gnarled staff, coiling serpent, eerie green underworld glow, dark gothic pixel art,
somber palette with murky green and gold accents
```
**Animacije:** idle `breathing-idle` · attack `fireball` (bacanje otrova/kletve — NE priziva)

## 6. Stribog — Wind, DPS (knockback)
**Description:**
```
Stribog the Slavic god of wind and storms, lean bearded deity in flowing tattered robes,
swirling wind, holding a curved horn, grey and teal tones, dark gothic pixel art,
somber palette with pale teal and gold accents
```
**Animacije:** idle `breathing-idle` · attack v3 custom „blowing a strong gust, raising the horn" (dir = svih 8)

## 7. Jarilo — Nature, DPS (brz napad)
**Description:**
```
Jarilo the Slavic god of spring, youthful warrior crowned with wheat and flowers,
holding a curved sickle blade, green vines and golden grain, dark gothic pixel art,
somber palette with vivid green and gold accents
```
**Animacije:** idle `breathing-idle` · attack v3 custom „quick slashing with the sickle" (dir = svih 8)

## 8. Mokoš — Earth, RANGED (korenje: `Stun`/`AOE_Slow`) — BEZ heala
**Description:**
```
Mokosh the Slavic earth and fertility goddess, matronly figure in earthen embroidered robes,
holding a spindle and thread, fertile green and brown tones, dark gothic pixel art,
somber palette with warm earth and gold accents
```
**Animacije:** idle `breathing-idle` · attack `fireball` ili v3 „casting binding earth roots" (dir = svih 8) — NE leči

## 9. Svetovid — Light, MELEE (zamah mačem / `AOE_Front`) — 4-smer = ext
**Description:**
```
Svetovid the Slavic four-faced war god, stern deity with four faces, white and gold gothic armor,
holding a great sword, radiant white light, dark gothic pixel art,
somber palette with white and gold accents
```
**Animacije:** idle `breathing-idle` · attack v3 custom „swinging the great sword in a wide arc" (dir = svih 8)

## 10. Triglav — Arcane, Hybrid (menja forme)
**Description:**
```
Triglav the Slavic three-headed god, three-faced deity in ornate arcane robes,
holding a golden staff, mystical purple arcane glow, dark gothic pixel art,
somber palette with deep purple and gold accents
```
**Animacije:** idle `breathing-idle` · attack `fireball` (arkana magija)

---

# 👹 ČUDOVIŠTA (enemies)

## 11. Drekavac — brz, slab (swarm) · 64px
**Description:**
```
Drekavac a gaunt screaming Slavic night demon, emaciated shadowy humanoid creature,
long claws, glowing eyes, wild and feral, dark gothic pixel art,
somber palette with sickly grey and faint blood red
```
**Animacije:** idle `breathing-idle` · walk `running-6-frames` (brz) · attack v3 „lunging claw swipe" (dir = svih 8) · death `falling-back-death`

## 12. Psoglav — tank, spor, jak · 80px
**Description:**
```
Psoglav a hulking dog-headed Slavic ogre giant, brutish heavily armored muscular body,
snarling canine head, massive build, dark gothic pixel art,
somber palette with iron grey and dull bronze accents
```
**Animacije:** idle `breathing-idle` · walk `walking` (težak) · attack `surprise-uppercut` (ili v3 „heavy overhead smash") · death `falling-back-death`

## 13. Aždaja — BOSS, višeglavi zmaj · 128px · `body_type: quadruped`
> ⚠️ Zmaj nije humanoid. Koristi **`body_type: quadruped`** sa template-om **`lion`** ili **`bear`** (4 noge + masivno telo). Quadruped animacije variraju po template-u → posle `create_character` pozovi `get_character` da vidiš dostupne animacije za taj template.
**Description:**
```
Azdaja a massive multi-headed Slavic dragon, three serpentine heads, scaled hulking body,
sharp horns and claws, charred wings, breathing fire, dark gothic pixel art,
somber palette with deep red, charred black and gold accents
```
**Animacije:** idle (template po quadrupedu) · walk · attack v3 „rearing up and breathing fire" (dir = svih 8) · (opc) death
> Ako quadruped ne ispadne dobro: fallback = generiši Aždaju kao **map object** (statična) ili humanoidnog „zmaja-čoveka", uz upozorenje da gubiš pun zmajoliki izgled.

---

## Redosled rada (preporuka)
1. Završi **Svaroga** (idle + attack, 8 pravaca) — referenca kvaliteta.
2. Pusti ostalih 9 bogova (`create_character`), pa batch animacije.
3. Pa 3 čudovišta (Drekavac, Psoglav, pa Aždaja zadnja jer je najrizičnija).
4. Skidaj u `Assets/Art/Towers/<Ime>/` i `Assets/Art/Enemies/<Ime>/`.
