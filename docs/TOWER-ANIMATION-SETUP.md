# Tower Directional Animation — Unity Setup

How to wire up 8-direction tower animations (idle + attack) using pre-rendered
sprite sets, with **smooth turning** and **frame continuity** (the attack keeps
playing from the same frame while the tower rotates).

Scripts involved:
- `Assets/Scripts/Tower/TowerDirectionalAnimator.cs` — new component that picks the
  sprite by direction + frame.
- `Assets/Scripts/Tower/Tower.cs` — updated to drive the animator (faces the target
  every frame, fires the attack clip on each shot).

---

## 1. How it works (read this first)

- The tower has **8 facing directions**, 45° apart.
- Two clips per tower:
  - **idle** (e.g. `animations/animating/`) — loops forever.
  - **attack** (e.g. `animations/swinging_a_heavy_war_hammer.../`) — plays **once**
    per shot, then automatically returns to idle.
- The displayed **direction** comes from a *smoothly interpolated* facing angle
  (`turnSpeedDegPerSec`). Turning West → East rotates **through** SW/S/SE (or
  NW/N/NE), it does **not** snap.
- The **frame index is shared** and is **not reset** when the direction changes.
  So if the attack is on frame 2 and the tower turns West → East, it keeps playing
  frame 2, 3, 4 … in the new direction's sprites. This is exactly the requested
  behaviour.

### Direction index order (assign sprites in THIS order)

| Index | Direction   | Angle | Art folder name |
|-------|-------------|-------|-----------------|
| 0     | East        | 0°    | `east`          |
| 1     | North-East  | 45°   | `north-east`    |
| 2     | North        | 90°   | `north`         |
| 3     | North-West  | 135°  | `north-west`    |
| 4     | West        | 180°  | `west`          |
| 5     | South-West  | 225°  | `south-west`    |
| 6     | South        | 270°  | `south`         |
| 7     | South-East  | 315°  | `south-east`    |

> If your tower faces the wrong way, the most common cause is assigning the 8
> direction slots in the wrong order. Double-check against this table.

---

## 2. Sprite import settings

The frames are individual PNGs (e.g. `frame_000.png … frame_008.png`). For each
PNG (or select the whole folder and set them all at once) in the Inspector:

- **Texture Type:** `Sprite (2D and UI)`
- **Sprite Mode:** `Single`
- **Pixels Per Unit:** match the rest of the game (use the same value your existing
  tower/enemy sprites use).
- **Filter Mode:** `Point (no filter)`  ← keeps pixel art crisp
- **Compression:** `None`
- Click **Apply**.

(These are separate files per frame, so there's **no** sprite slicing / sheet
needed.)

---

## 3. Prefab / GameObject setup

Assume a tower prefab with a child that shows the art (a `SpriteRenderer`).

1. Select the GameObject that has (or should have) the **SpriteRenderer** for the
   tower body.
2. **Add Component → `TowerDirectionalAnimator`**.
   - It requires a `SpriteRenderer` on the same GameObject (auto-added if missing).
3. Fill **Idle** (size 8) and **Attack** (size 8):
   - Set each array's **Size = 8**.
   - For each element `0..7` (East, NE, N, NW, W, SW, S, SE — see table above):
     - Expand the element, set **Frames** size to the number of frames
       (idle = 4, attack = 9 for Svarog), and drag the `frame_000 … frame_00N`
       sprites in order.
   - Tip: lock the Inspector (padlock) and multi-select `frame_000…00N` in the
     Project window, then drag them onto the **Frames** array header to fill it in
     order in one drop.
4. **Playback:**
   - `Idle Fps` ≈ 8
   - `Attack Fps` — set so the attack reads well **and** finishes within the fire
     interval: `attackFrames / attackFps < FireRate`.
     Example: 9 attack frames, FireRate = 1.0s → `attackFps ≥ 10` (9/10 = 0.9s).
5. **Rotation:**
   - `Turn Speed Deg Per Sec` ≈ 540 (≈1.5 turns/sec). Lower = slower, more
     visible smooth turning. Higher = snappier.
   - `Instant Turn` = leave **off** for the smooth West→East behaviour. Turn it on
     only if you want the direction to snap.

`Tower.cs` finds the animator automatically via `GetComponentInChildren<TowerDirectionalAnimator>()`,
so the animator can live on the prefab root **or** any child.

---

## 4. What `Tower.cs` now does

- Every frame it has a target, it calls `dirAnimator.FaceDirection(toTarget)` —
  this only sets the desired angle; the animator does the smooth turn.
- On each shot (`CoolDown >= FireRate`) it calls `dirAnimator.PlayAttack()`.
- If **no** `TowerDirectionalAnimator` is present, it falls back to the old
  behaviour (`transform.right = toTarget`), so existing towers without directional
  art still work.

### Note on `firePoint`

The tower transform is **no longer rotated** when a directional animator is used
(rotating it would rotate the pre-rendered sprite and break the art). That means a
child `firePoint` no longer swings around. Projectiles still home onto the target
(`Projectile.Initialize`), so this usually doesn't matter. If you need the muzzle to
sit on the facing side, offset the spawn position by the facing direction in
`Tower.SpawnProjectileOrHit` (optional, not required for it to work).

---

## 5. Quick test checklist

1. Place the tower, let an enemy walk into range.
2. Idle loop plays while no target / between shots.
3. On each shot the attack clip plays once, then returns to idle.
4. Move enemies around the tower (or watch a curved path): the tower turns
   **smoothly** through the intermediate directions.
5. Fire while the enemy crosses from one side to the other: the swing keeps going
   from its current frame as the direction changes (no restart, no snap).

---

## 6. Reusing for other towers (Stribog, Perun, …)

Same steps. Just assign that tower's `animating` (idle) and its attack folder
(whatever the action is named) into the 8 direction slots. Frame counts can differ
per tower — the component reads the array length, no code change needed.
