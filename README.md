# TankChaos 2D

A top-down 2D tank arcade shooter built with Unity 2022.3 LTS. Fight through 5 progressively challenging levels, collect power-ups, customize your tank, and take on a 3-phase boss in the final showdown.

---

## Gameplay

Control a tank from a top-down perspective. Move with WASD, aim with the mouse, and eliminate all enemies to clear each level. Every level introduces new mechanics and a tougher enemy lineup.

### Controls

| Input | Action |
|-------|--------|
| `W A S D` | Move tank |
| `Mouse` | Aim cannon |
| `Left Mouse Button` | Fire |
| `Right Mouse Button` | Switch weapon (3 s cooldown) |
| `Left Shift` | Dash (0.4 s burst, 1.5 s cooldown) |
| `Space` | Pause / Resume |

### Weapons

| Weapon | Description | Fire Rate |
|--------|-------------|-----------|
| **Cannon** | Single accurate shot | 1.0 s |
| **Shotgun** | 3-bullet spread, short range | 0.8 s |

---

### Level Progression (3 Levels)

| Level | Name | Description |
|-------|------|-------------|
| 1 | **The Proving Grounds** | 10 basic enemies, 150s time limit |
| 2 | **The Escalation** | 20 mixed enemies with respawn waves, 150s time limit |
| 3 | **Titan's Wrath** | Boss fight with reinforcements, no time limit |

Levels unlock sequentially. Progress is saved locally so you can pick up where you left off.

---

## Boss — The Titan (L3)

The Titan has **60 HP** and three distinct phases:

| Phase | HP Threshold | Speed | Attack |
|-------|-------------|-------|--------|
| 1 — Hunting | > 66 % | Slow | Single shot every 2 s |
| 2 — Berserker | > 33 % | Medium | 3-spread shots every 1 s |
| 3 — RAMPAGE | ≤ 33 % | Fast | Wide 3-spread every 0.4 s |

Phase transitions trigger screen-space floating text and summon additional minions.

---

## Power-ups

| Pick-up | Effect |
|---------|--------|
| **Health Pack** | Restores HP |
| **Damage Boost** | 2× attack multiplier for a limited time (orange glow effect) |

---

## Features

- **Procedural arena** — 23×23 map generated each run; mix of indestructible boundaries and destructible walls
- **Object pooling** — `BulletPool` recycles projectiles to avoid GC spikes
- **Garage / Customization** — paint body, tracks, and barrel independently; saved between sessions
- **Leaderboard** — local Hall of Fame records player name, time, and score for each cleared level
- **Audio** — per-level BGM tracks + sound effects; volume controlled in Settings
- **VFX** — muzzle flash, explosions, floating score text, camera shake, particle effects
- **HUD** — live HP, score, countdown timer, dash cooldown radial, weapon cooldown radial, Boss health bar

---

## Architecture

```
Assets/
├── Scripts/
│   ├── Core/          # GameManager (FSM), MapGenerator, LevelConfig/Database, GameBootstrapper
│   ├── Player/        # PlayerController, PlayerHealth
│   ├── Enemies/       # EnemyBase, BasicEnemy, FastEnemy, BossEnemy, EnemyAI, EnemySpawner
│   ├── Combat/        # Bullet, BulletPool
│   ├── Pickups/       # PickupBase, HealthPickup, DamageBoostPickup, PickupSpawner
│   ├── Map/           # DestructibleWall
│   ├── UI/            # UIManager, UIBuilder, all panel scripts
│   ├── Audio/         # AudioManager
│   ├── VFX/           # VfxManager, CameraShake, FloatingText, ParticleBehaviour, DropShadow
│   └── Data/          # SaveSystem, LeaderboardData, SkinManager
└── Editor/
    ├── UISceneBuilder  # One-click full UI scene generator (Tools > Game Tools > Build UI Scene)
    ├── PrefabBuilder   # Prefab generation helper
    └── GameSetupTool   # Scene bootstrap utility
```

**Key design decisions:**
- All UI is built entirely in code — no Prefab dependencies, no manual Inspector wiring required
- `GameManager` drives a strict `GameState` FSM (`MainMenu → LevelSelect → Playing → Paused / Victory / GameOver`)
- `UIManager` subscribes to `OnGameStateChanged` and activates the correct panel automatically
- `EnemySpawner` is event-driven; `GameManager` reacts to `OnAllEnemiesCleared` to trigger Victory

---

## Getting Started

### Requirements

- **Unity 2022.3.x LTS** (tested on 2022.3.62f1)
- No third-party packages — only standard Unity modules

### Running the project

1. Clone or download this repository
2. Open the project in **Unity Hub** with Unity 2022.3.x
3. Open `Assets/Scenes/GameScene.unity` (or `MainGame.unity`)
4. Press **Play**

### Rebuilding the UI (optional)

If the scene Canvas is missing or broken, regenerate the entire UI hierarchy from the menu bar:

```
Tools > Game Tools > Build UI Scene
```

---

## Building / Packaging

1. **File → Build Settings** (`Ctrl+Shift+B`)
2. Confirm `Assets/Scenes/GameScene` is listed under *Scenes In Build*
3. Select target platform (e.g. **PC, Mac & Linux Standalone → Windows x86_64**)
4. Click **Build** and choose an output folder (e.g. `Builds/Windows/`)

---

## Project Info

| Field | Value |
|-------|-------|
| Engine | Unity 2022.3.62f1 LTS |
| Render pipeline | Built-in (2D) |
| Scripting backend | Mono (.NET Standard 2.1) |
| Input system | Legacy Input Manager |
| Target resolution | 1920 × 1080 (scales with screen) |
| Platforms tested | Windows PC |

---

## License

This project is released for educational and portfolio purposes. Feel free to study, fork, or build upon it.
