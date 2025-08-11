## 3TD — Endless Tower Defense

A fast-to-build, modular tower defense designed for high replayability via escalating, randomly offered rule cards between waves.

### Overview
- **Genre**: Endless Tower Defense
- **Platform**: PC (Unity)
- **Developer**: Branislav Čuturilo
- **Game Jam Duration**: 4 Aug 2025 – 11 Aug 2025
- **Run Length**: Endless; the game ends only when the player loses

### Core Loop
1. The player builds towers.
2. A wave of enemies arrives.
3. After the wave, the player chooses one of 3 randomly offered rule cards.
4. The chosen rule is applied; gameplay parameters change.
5. Waves escalate; rules scale (become stronger or more penalizing).
6. Repeat until defeat.

### Rule System
- **Generation**: At the end of each wave, 3 rules are randomly selected from the available pool.
- **Scaling**: Each rule can have unlimited levels (e.g., AttackMod I → II → III → …). Higher levels:
  - Increase positive/negative impact
  - Shift tower–enemy balance (e.g., tougher enemies, weaker bonuses)
- **Rule Types**:
  - Tower stats (damage, range, fire rate, effects…)
  - Enemies (HP, speed, armor, spawn frequency…)
  - Economy (gold per kill, starting gold…)
- **Example Rules (assets present)**:
  - `Default`, `AttackMode`, `Tanky`, `SlowDown`, `Quickly`, `FAST&Furious`, `NonStop`, `Boss`, `FUN`

### Tower System
- **Modular**: Add new towers via drag-and-drop; unlimited upgrade levels
- **Adjustable Stats**: range, fire rate, projectile speed, damage, cost
- **Effects**: slow, aoe_slow, dot, aoe_dot, aoe_impact, stun, aoe_stun, standard damage
- **Targeting**: First, Last, Strongest
- **Per-level Assets**: Shooting/visual per level loaded individually
- **Effect combos**: e.g., AoE impact + AoE slow; DOT + AoE DOT; Slow + DOT

### Enemy System
- **Rapid authoring** of new enemy types
- **Adjustable Stats**: HP, speed, armor
- **Resistances**: e.g., immune to slow, immune to DOT
- **Wave Scaling**: Each wave increases enemy strength; rules may further buff enemies

### Map
- **Modular paths**: Easily change path layouts
- **Multi-map ready**: Add new maps without code changes
- **Visuals**: In development; placeholder graphics for now

### Audio
- Background music created in Bosca Ceoil (placeholder loop)
- Planned: improved shooting and explosion SFX

### Visual Identity
- Not final; current assets are placeholders
- Target styles to explore: fantasy, steampunk, sci‑fi

### Technical Advantages
- **Fast development**: Systems are modular and easily expandable
- **Replayability**: Endless format + random rule set each run
- **Scalability**: Add rules, towers, enemies, and maps without changing the game core
