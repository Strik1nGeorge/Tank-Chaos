# TankChaos 2D

English | [简体中文](README.zh-CN.md)

TankChaos 2D is a top-down arcade tank shooter built with Unity 2022.3 LTS.
Fight through three progressively harder stages, switch between cannon and
shotgun fire, collect temporary upgrades, customize your tank, and defeat a
three-phase boss.

Current version: **1.0.1**

[Download the latest Windows release](https://github.com/GeorgeNum7/Tank-Chaos/releases/latest)

## v1.0.1 highlights

- Added a temporary piercing laser weapon and a new level-3 pickup pool.
- Added background-music loading, track switching, and new music.
- Reworked level pacing, map generation, enemy spawning, and boss behavior.
- Improved the HUD, instructions, menus, settings, and visual feedback.
- Normal cannon rounds now ricochet cleanly from indestructible walls.
- Fixed missing release source files and aligned the project version with 1.0.1.

## Gameplay

Move through a 23 × 23 arena, destroy the current enemy roster before time runs
out, collect dropped upgrades, and unlock the next level. Progress, settings,
tank colors, and local leaderboard entries persist between sessions.

### Controls

| Input | Action |
|---|---|
| `W` `A` `S` `D` | Move the tank |
| Mouse movement | Aim the turret |
| Left mouse button | Fire the current weapon |
| Right mouse button | Switch cannon/shotgun (3-second cooldown) |
| Hold `Left Shift` | Sprint while stamina is available |
| `Space` | Pause or resume |

### Weapons and projectile behavior

| Weapon | Behavior |
|---|---|
| Cannon | Accurate single shot with a 1-second cooldown. Normal player rounds ricochet from indestructible walls. |
| Shotgun | Three-pellet spread with a 0.8-second cooldown and short range. Pellets disappear on wall impact. |
| Laser | Five-second level-3 pickup. Fires faster, deals 2 damage, destroys destructible walls, and pierces indestructible walls. |

Enemy projectiles disappear when they hit walls. Destructible walls are removed
by any damaging projectile.

## Levels

| Level | Name | Configuration |
|---|---|---|
| 1 | **The Proving Grounds** | 10 basic enemies, no respawns, 150-second limit |
| 2 | **The Escalation** | 10 basic + 10 fast enemies, reinforcement spawning, 150-second limit |
| 3 | **Titan's Wrath** | Five opening enemies, the Titan boss, reinforcements, reduced obstacle density, no time limit |

Levels unlock sequentially. Completing a stage records score and clear time in
its local leaderboard.

## The Titan

The Titan has **45 HP** and changes behavior as its health falls.

| Phase | Health | Movement speed | Attack |
|---|---:|---:|---|
| Hunting | Above 66% | 1.0 | Single shot every 2.0 seconds |
| Berserker | 34%–66% | 1.8 | Three-shot spread every 1.0 second |
| RAMPAGE | 33% or below | 3.0 | Wider three-shot spread every 0.7 seconds |

Phase changes update the boss HUD, trigger visual feedback, and can bring in
additional enemies.

## Pickups

Pickups begin appearing from level 2. Defeated enemies also have a 20% chance to
drop one.

| Color | Pickup | Effect |
|---|---|---|
| Green | Health | Restores 1 HP, up to the 5 HP maximum |
| Blue | Damage boost | For 5 seconds, cannon damage becomes ×3 and boosted shotgun pellets deal 2 damage |
| Cyan | Laser | Level 3 only; activates the piercing laser for 5 seconds |

The player starts with **5 HP**, **3 lives**, and 1.5 seconds of invincibility
after taking damage.

## Main systems

- **State-driven game flow:** main menu, level selection, playing, paused,
  victory, and game-over states are coordinated by `GameManager`.
- **Procedural maps:** each run creates a new arena with indestructible and
  destructible cover; level 3 uses a lower obstacle density for the boss fight.
- **Projectile pooling:** `BulletPool` reuses projectile objects to reduce
  allocations during combat.
- **Enemy variety:** basic tanks, fast tanks, and a three-phase boss use
  line-of-sight and movement logic.
- **Garage customization:** body, track, and barrel colors can be selected
  independently and are saved locally.
- **Local progression:** unlocked levels, settings, skins, and leaderboards
  persist between sessions.
- **Audio:** per-level BGM, a victory track, generated sound effects, and
  independent BGM/SFX volume controls.
- **UI and feedback:** HUD, boss health display, cooldown indicators,
  instructions, floating score text, particles, explosions, and camera shake.
- **Editor utilities:** project setup, prefab generation, and UI-scene rebuilding
  are available from `Tools > Game Tools`.

## Requirements

- Unity **2022.3.62f3 LTS** is recommended.
- Windows Build Support is required to create the Windows player.
- The project uses the built-in render pipeline, the legacy Input Manager, and
  Unity registry packages listed in `Packages/manifest.json`.
- No external third-party runtime package is required.

## Run from source

```powershell
git clone https://github.com/GeorgeNum7/Tank-Chaos.git
cd Tank-Chaos
```

1. Add the cloned directory in Unity Hub.
2. Open it with Unity `2022.3.62f3`.
3. Open `Assets/Scenes/MainGame.unity`.
4. Wait for script and asset import to finish.
5. Press Play.

`MainGame.unity` is the active scene. `GameScene.unity` remains in the repository
but is disabled in Build Settings.

### Rebuild generated content

The editor tools are optional and should only be used when the corresponding
scene or prefab content needs to be regenerated:

```text
Tools > Game Tools > Setup Game Scene
Tools > Game Tools > Build Prefabs
Tools > Game Tools > Build UI Scene
Tools > Game Tools > [REBUILD FULL PROJECT]
```

Commit or back up scene changes before running the full rebuild command.

## Build a Windows player

### Unity editor

1. Open `File > Build Settings`.
2. Confirm only `Assets/Scenes/MainGame.unity` is enabled.
3. Select `PC, Mac & Linux Standalone`.
4. Set the target platform to Windows and architecture to `x86_64`.
5. Build to a clean directory such as `Builds/Windows/`.

Keep the executable, `TankChaos_2D_Data`, `UnityPlayer.dll`,
`UnityCrashHandler64.exe`, and `MonoBleedingEdge` together when distributing the
game.

### Command line

```powershell
& "<UnityEditorPath>\Unity.exe" `
  -batchmode -quit `
  -projectPath "<ProjectPath>" `
  -buildTarget Win64 `
  -buildWindows64Player "<OutputPath>\TankChaos_2D.exe" `
  -logFile "<OutputPath>\build.log"
```

Change the project and output paths to match your machine.

## Project structure

```text
Assets/
├── Editor/                 # Scene, prefab, UI, tag, and layer setup tools
├── Resources/
│   ├── Audio/BGM/          # Menu, level, and victory music
│   ├── Prefabs/            # Player, enemy, boss, and bullet prefabs
│   └── Sprites/            # Runtime-loaded sprite assets
├── Scenes/
│   ├── MainGame.unity      # Active game and build scene
│   └── GameScene.unity     # Disabled legacy/alternate scene
└── Scripts/
    ├── Audio/              # Music and sound-effect management
    ├── Combat/             # Bullet behavior and object pooling
    ├── Core/               # Game state, levels, map generation, bootstrap
    ├── Data/               # Saves, leaderboards, and tank skins
    ├── Enemies/            # Enemy types, AI, spawning, and boss logic
    ├── Map/                # Destructible walls
    ├── Pickups/            # Health, damage boost, laser, and spawning
    ├── Player/             # Movement, weapons, stamina, health, and lives
    ├── UI/                 # Menus, HUD, settings, instructions, and panels
    └── VFX/                # Particles, camera shake, shadows, floating text
```

## Architecture notes

- `GameManager` owns the high-level state machine and level transitions.
- `GameBootstrapper` locates or creates the core runtime systems.
- `MapGenerator` builds the arena and cover at runtime.
- `EnemySpawner` controls stage rosters, respawns, and boss reinforcements.
- `UIManager` reacts to game-state changes and shows the correct panels.
- `SaveSystem` stores per-level leaderboard JSON and level unlock progress.
- `SkinManager` and audio settings use Unity `PlayerPrefs`.

## Save data

On Windows, Unity normally stores this project's persistent data under:

```text
%USERPROFILE%\AppData\LocalLow\DefaultCompany\TankChaos_2D
```

The leaderboard file is named `leaderboard_v2.json`. Level unlocks, tank colors,
and audio settings are stored through `PlayerPrefs`.

## Contributing

Issues and focused pull requests are welcome. When changing gameplay:

1. Test all three levels in `MainGame.unity`.
2. Confirm player and enemy projectile behavior.
3. Verify pause, victory, game-over, saves, and level unlocking.
4. Do not commit Unity-generated `Library`, `Logs`, `Temp`, or build-output
   directories.

## License

This repository is shared for educational and portfolio use. You may study, fork,
and build on the project for those purposes. No separate license file is
currently included; contact the repository owner before commercial use or
redistribution.
