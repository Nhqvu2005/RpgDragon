# 🐉 RPGDragon — 2D Top-Down RPG

A 2D top-down RPG built with Unity & C#, inspired by classic Zelda-like games.  
The hero fights with melee combat, takes quests from NPCs, explores 3 maps, and defeats the final Dragon boss.

## 🎮 Gameplay

- **Top-down movement** — WASD / Arrow keys, 4-direction facing
- **Melee combat** — Attack with Space/Z, combo cooldown, knockback
- **Quest system** — Accept quests from NPCs, track progress, turn in for rewards
- **3 Maps** — Forest → Dungeon → Boss Castle
- **Boss fight** — 2-phase Dragon boss with fire breath, claw attacks, and minion summoning
- **NPC dialogue** — Typewriter effect, quest accept/decline choices

## 🏗️ Architecture

```
Assets/Scripts/
├── Core/          GameManager, EventBus, SceneLoader, AudioManager, SaveManager
├── Player/        PlayerController, PlayerStats, PlayerCombat
├── Enemy/         EnemyBase, EnemyMelee, EnemyDragon (boss), EnemySpawner
├── Quest/         QuestData, QuestObjective, QuestManager
├── NPC/           NPCController, DialogueData, DialogueSystem
└── UI/            HealthBar, BossHealthBar, DialogueUI, QuestLogUI, GameMenuUI, DamagePopup
```

### Design Patterns
- **Singleton** — GameManager, EventBus, AudioManager, QuestManager, DialogueSystem, SceneLoader
- **Observer (EventBus)** — Decoupled event system: `EventBus.Raise<T>()` / `Register<T>()`
- **State Machine (enum+switch)** — Player states (Idle/Walk/Attack/Hurt/Dead), Enemy states (Idle/Patrol/Chase/Attack/Hurt/Dead)
- **ScriptableObject** — Data-driven quest definitions, dialogue data
- **Object Pooling** — Damage popups, projectiles, hit effects

## 🗺️ Maps

| Scene | Description | Enemies | NPC |
|---|---|---|---|
| `MainMenu` | Title screen, Play/Load/Quit | — | — |
| `Map1_Forest` | Starting village + forest | Slime, Bat | OldMan (quest giver) |
| `Map2_Dungeon` | Underground cave | Skeleton, Slime | Sage (story) |
| `Map3_BossCastle` | Boss room | Minion waves, Dragon Boss | — |

## 🛠️ Setup

1. Install **Unity Hub** and **Unity Editor 2022.3 LTS**
2. Clone this repo
3. Open Unity Hub → **Open Project** → select the folder
4. In Unity Editor, open `Scenes/MainMenu` and press **Play**

> **Note:** This repo contains only C# scripts. You need to import 2D sprite/tilemap assets and build the scenes in the Unity Editor.

## 🎯 Current Status

MVP phase — core gameplay implemented:
- ✅ Player movement & state machine
- ✅ Melee combat with hitbox detection
- ✅ Enemy AI (patrol, chase, attack)
- ✅ Quest system (accept, progress, complete)
- ✅ NPC dialogue system
- ✅ Scene transition with fade effects
- ✅ Boss fight (2-phase Dragon)
- ✅ UI (health bars, quest log, dialogue, pause menu)
- ⬜ Unity scene setup & prefab assembly required

## 📦 Recommended Assets

- **Characters:** 2D Pixel Art RPG spritesheets (4-direction)
- **Environment:** Top-down tilemap sets (grassland, dungeon, castle)
- **Enemies:** Slime, Skeleton, Bat, Dragon 2D pixel art
- **UI:** RPG UI pack (health bar, buttons, dialogue box)
- **Audio:** Free RPG SFX pack (sword, hit, death, BGM)

## 📄 License

MIT
