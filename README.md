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

> **Lưu ý:** Repo này chỉ chứa C# scripts. Bạn cần tự import sprite/tilemap assets và dựng scene trong Unity Editor.

## Trạng thái hiện tại

Giai đoạn MVP - đã hoàn thành gameplay cơ bản:
- Đã có: Di chuyển player, đánh cận chiến, enemy AI, quest system, NPC hội thoại, chuyển map, boss 2 phase, UI (health bar, quest log, pause menu)
- Thiếu: Setup scene Unity, prefab, tilemap, asset 2D

## Assets đề nghị

- **Nhân vật:** 2D Pixel Art RPG spritesheet (4 hướng di chuyển, đánh)
- **Map:** Top-down tilemap (grassland, dungeon, castle)
- **Enemy:** Slime, Skeleton, Bat, Dragon 2D pixel art
- **UI:** RPG UI pack (health bar, buttons, dialogue box)
- **Âm thanh:** Free RPG SFX pack (kiếm, hit, chết, BGM)

## Giấy phép

MIT
