# RPGDragon - 2D Top-Down RPG

Game 2D RPG góc nhìn top-down (Zelda-like) được xây dựng bằng Unity va C#.  
Nhan vat chinh danh can chien, nhan nhiem vu tu NPC, di qua 3 map va danh boss cuoi la Rong.

## Gameplay

- **Di chuyen top-down** - WASD / Phim mui ten, xoay theo 4 huong
- **Danh can chien** - Space/Z de tan cong, co cooldown, knockback
- **He thong nhiem vu** - Nhan quest tu NPC, theo doi tien trinh, bao cao de nhan thuong
- **3 Map** - Rung (Forest) -> Ham ngo (Dungeon) -> Thanh castle (Boss Castle)
- **Danh Boss** - Rong 2 phase: vuot + phun lua (phase 1), nhanh hon + trieu hoi (phase 2)
- **NPC hoi thoai** - Hieu ung typewriter, lua chon chap nhan/tu choi quest

## Kien truc

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

- **Singleton** - GameManager, EventBus, AudioManager, QuestManager, DialogueSystem, SceneLoader
- **Observer (EventBus)** - He thong su kien ket noi cac thanh phan: `EventBus.Raise<T>()` / `Register<T>()`
- **State Machine (enum+switch)** - Player: Idle/Walk/Attack/Hurt/Dead; Enemy: Idle/Patrol/Chase/Attack/Hurt/Dead
- **ScriptableObject** - Dinh nghia quest va hoi thoai trong Inspector (data-driven)
- **Object Pooling** - DamagePopup, projectile, hieu ung hit

## Danh sach Map

| Scene | Mo ta | Enemy | NPC |
|---|---|---|---|
| MainMenu | Man hinh title, Play/Load/Quit | - | - |
| Map1_Forest | Lang bat dau + khu rung | Slime, Bat | OldMan (nhiem vu) |
| Map2_Dungeon | Hang dong ngam | Skeleton, Slime | Sage (co truyen) |
| Map3_BossCastle | Phong Boss | Minion waves, Dragon Boss | - |

## Huong dan Setup

1. Cai **Unity Hub** va **Unity Editor 2022.3 LTS**
2. Clone repo nay ve may
3. Mo Unity Hub -> **Open Project** -> chon thu muc game
4. Trong Unity Editor, mo scene `Scenes/MainMenu` va an **Play**

> **Luu y:** Repo nay chi chua C# scripts. Ban can tu import sprite/tilemap assets va dung scene trong Unity Editor.

## Trang thai hien tai

Giai doan MVP - da hoan thanh gameplay co ban:
- Da co: Di chuyen player, danh can chien, enemy AI, quest system, NPC hoi thoai, chuyen map, boss 2 phase, UI (health bar, quest log, pause menu)
- Thieu: Setup scene Unity, prefab, tilemap, asset 2D

## Assets de nghi

- **Nhan vat:** 2D Pixel Art RPG spritesheet (4 huong di chuyen, danh)
- **Map:** Top-down tilemap (grassland, dungeon, castle)
- **Enemy:** Slime, Skeleton, Bat, Dragon 2D pixel art
- **UI:** RPG UI pack (health bar, buttons, dialogue box)
- **Am thanh:** Free RPG SFX pack (kiem, hit, chet, BGM)

## Giao dich

MIT
