# RPGDragon - 2D Top-Down RPG

Game 2D RPG góc nhìn top-down (phong cách Zelda) được xây dựng bằng Unity và C#.
Nhân vật chính đánh cận chiến, nhận nhiệm vụ từ NPC, đi qua 3 map và đánh boss cuối là Rồng.

## Gameplay

- **Di chuyển top-down** - WASD / Phím mũi tên, xoay theo 4 hướng
- **Đánh cận chiến** - Space/Z để tấn công, có cooldown, knockback
- **Hệ thống nhiệm vụ** - Nhận quest từ NPC, theo dõi tiến trình, báo cáo để nhận thưởng
- **3 Map** - Rừng (Forest) -> Hầm ngục (Dungeon) -> Thành castle (Boss Castle)
- **Đánh Boss** - Rồng 2 phase: vồ + phun lửa (phase 1), nhanh hơn + triệu hồi (phase 2)
- **NPC hội thoại** - Hiệu ứng typewriter, lựa chọn chấp nhận/từ chối quest

## Kiến trúc

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
- **Observer (EventBus)** - Hệ thống sự kiện kết nối các thành phần: `EventBus.Raise<T>()` / `Register<T>()`
- **State Machine (enum+switch)** - Player: Idle/Walk/Attack/Hurt/Dead; Enemy: Idle/Patrol/Chase/Attack/Hurt/Dead
- **ScriptableObject** - Định nghĩa quest và hội thoại trong Inspector (data-driven)
- **Object Pooling** - DamagePopup, projectile, hiệu ứng hit

## Danh sách Map

| Scene | Mô tả | Enemy | NPC |
|---|---|---|---|
| MainMenu | Màn hình title, Play/Load/Quit | - | - |
| Map1_Forest | Làng bắt đầu + khu rừng | Slime, Bat | OldMan (giao nhiệm vụ) |
| Map2_Dungeon | Hang động ngầm | Skeleton, Slime | Sage (cốt truyện) |
| Map3_BossCastle | Phòng Boss | Minion waves, Dragon Boss | - |

## Hướng dẫn Setup

1. Cài **Unity Hub** và **Unity Editor 2022.3 LTS**
2. Clone repo này về máy
3. Mở Unity Hub -> **Open Project** -> chọn thư mục game
4. Trong Unity Editor, mở scene `Scenes/MainMenu` và nhấn **Play**

> **Lưu ý:** Repo này chỉ chứa C# scripts.Cần tự import sprite/tilemap assets và dựng scene trong Unity Editor.

## Thiết lập sau khi mở project lần đầu

Sau khi mở project trong Unity Editor lần đầu:

1. **Window -> RPGDragon -> Project Setup** -> chạy từng bước
2. Vào **Edit -> Project Settings -> Tags and Layers** kiểm tra đã có layer `Player` (layer 8) và `Enemy` (layer 9)
3. Import asset packs (sprite, tilemap, audio) vào thư mục `Assets/Art/` và `Assets/Audio/`
4. **Build Settings**: thêm các scene theo thứ tự: MainMenu, Map1_Forest, Map2_Dungeon, Map3_BossCastle

### Scene hiện có

Các scene đã được tạo sẵn với cấu trúc cơ bản:

| Scene | Nội dung cần thêm |
|---|---|
| MainMenu | Canvas menu (Play, Load, Quit), EventSystem |
| Map1_Forest | Tilemap nền rừng, Player spawn, NPC OldMan, slime spawner, exit zone sang Map2 |
| Map2_Dungeon | Tilemap nền hang động, NPC Sage, skeleton spawner, exit zone sang Map3 |
| Map3_BossCastle | Tilemap nền castle, Dragon boss prefab, VictoryController, exit zone về làng |

### Setup từng scene chi tiết

**MainMenu:**
- Tạo Canvas với Text "RPGDragon" (title) và 3 Button: Play, Load, Quit
- Play: `SceneManager.LoadScene("Map1_Forest")`
- Quit: `Application.Quit()`

**Map1_Forest:**
- Tạo Grid + Tilemap cho nền đất, tường, cây
- Đặt SpawnPoint_Default ở đầu map
- Đặt Player prefab trong scene (hoặc để GameManager tự spawn)
- Đặt NPC OldMan + dialogue data
- Đặt enemy spawner (slime, bat)
- Đặt ExitZone trigger ở cuối map -> targetScene="Map2_Dungeon"

**Map2_Dungeon:**
- Tương tự Map1: tilemap hang động, NPC Sage, skeleton spawner
- ExitZone -> targetScene="Map3_BossCastle", requiredQuestId="quest_map1"

**Map3_BossCastle:**
- Tilemap castle
- Boss room trigger + EnemyDragon prefab
- VictoryController script trong scene

### Prefab cần tạo

Sau khi import sprite assets, tạo các prefab sau trong Unity Editor:

- **Player.prefab** — SpriteRenderer + Rigidbody2D + Collider2D + Animator + PlayerController/Stats/Combat/Upgrade
- **Enemy/Slime.prefab** — EnemyMelee với sprite slime, waypoints
- **Enemy/DragonBoss.prefab** — EnemyDragon với sprite rồng, fire breath prefab
- **NPC/OldMan.prefab** — NPCController + collider trigger
- **UI/HUDCanvas.prefab** — Canvas với HealthBar, BossHealthBar, DialogueUI, QuestLogUI
- **Effects/HitSpark.prefab** — ParticleSystem
- **Effects/FireBreathProjectile.prefab** — Projectile cho rồng phun lửa

> Có thể dùng tool **Window -> RPGDragon -> Project Setup** để tạo prefab Player, EventSystem, HUDCanvas tự động.

## Trạng thái hiện tại

Giai đoạn MVP - đã hoàn thành gameplay cơ bản:
- Đã có: Di chuyển player, đánh cận chiến, enemy AI, quest system, NPC hội thoại, chuyển map, boss 2 phase, nâng cấp đồ (mảnh vũ khí/giáp), UI (health bar, quest log, pause menu), victory (boss chết về làng)
- Cần làm: Import sprite assets, tạo tilemap, set up scene, gán prefab

- **Nhân vật:** 2D Pixel Art RPG spritesheet (4 hướng di chuyển, đánh)
- **Map:** Top-down tilemap (grassland, dungeon, castle)
- **Enemy:** Slime, Skeleton, Bat, Dragon 2D pixel art
- **UI:** RPG UI pack (health bar, buttons, dialogue box)
- **Âm thanh:** Free RPG SFX pack (kiếm, hit, chết, BGM)

## Giấy phép

MIT
