# Pocket Garden — Project Documentation

## Overview

A cozy merge puzzle mobile game — Unity 6 with URP 2D, targeting Android/iOS. Merge items, complete quests, and decorate your garden.

## Project Status (2026-06-24)

### Implemented

| System | Status | Description |
|--------|--------|-------------|
| Merge Grid | ✅ | 5×7 grid with drag & drop |
| Merge Logic | ✅ | 3 chains × 7 levels (Garden, Wood, Stone) |
| Merge Database | ✅ | ScriptableObject with all item definitions |
| Drag & Drop | ✅ | Touch + mouse input (Input System) |
| Energy System | ✅ | 30 max, +1/3min, offline regen |
| Coin System | ✅ | Currency earned from quests |
| Quest System | ✅ | 8 quests, deliver items for coins |
| Generators | ✅ | Produce items on timer (cooldown-based) |
| Energy UI | ✅ | Top-left display with tick regen |
| Quest UI | ✅ | Bottom bar with active quests |
| Coin UI | ✅ | Top-right display |
| Editor Setup | ✅ | PocketGarden → Setup Scene |

### Not Yet Implemented

- [ ] Garden decoration (meta-game)
- [ ] Shop + IAP
- [ ] Ads (AdMob)
- [ ] Daily Bonus
- [ ] Push Notifications
- [ ] Sound Effects / Music
- [ ] Save/Load (full state persistence)
- [ ] Sprite assets (using colored squares as prototype)
- [ ] Seasonal events
- [ ] VIP Pass

## Architecture

### Namespaces

```
PocketGarden.Core       — GameManager, EnergySystem, CoinSystem, MergeDatabase, MergeItemData
PocketGarden.Grid       — MergeGrid, GridCell, MergeGridItem, DragDropHandler, Generator
PocketGarden.Quests     — QuestManager, Quest
PocketGarden.UI         — EnergyUI, QuestUI
PocketGarden.Editor     — SceneSetup
```

### Merge Chains

| Chain | Lv1 | Lv2 | Lv3 | Lv4 | Lv5 | Lv6 | Lv7 |
|-------|-----|-----|-----|-----|-----|-----|-----|
| Garden | Seed | Sprout | Bush | Flower | Tree | Big Tree | Magic Tree |
| Wood | Twig | Log | Plank | Crate | Furniture | Gazebo | House |
| Stone | Pebble | Stone | Brick | Wall | Pillar | Fountain | Castle |

### Core Loop

```
Merge items on grid (5×7)
  → Get higher level item
    → Deliver to quest
      → Earn coins + unlock garden area
        → Place decoration
          → Repeat
```

## Tech Stack

| Component | Technology |
|-----------|-------------|
| Engine | Unity 6 URP 2D |
| Input | Unity Input System |
| Platform | Android (primary), iOS (future) |
| Ads | Google AdMob (planned) |
| IAP | Unity Purchasing (planned) |
| Notifications | com.unity.mobile.notifications (planned) |

## Configuration

| Parameter | Value |
|-----------|-------|
| Company | Wonder Minds Games |
| Package | com.WonderMindsGames.PocketGarden |
| Orientation | Portrait |
| Grid Size | 5×7 (35 cells) |
| Max Energy | 30 |
| Energy Regen | +1 per 3 minutes |
| Merge Cost | 1 energy |

## How to Run

1. Open project in Unity 6
2. Platform: Android (File → Build Profiles)
3. Open Assets/Scenes/SampleScene
4. **PocketGarden → Setup Scene**
5. Play

## Changelog

### 2026-06-24
- Project created (Unity 6, Universal 2D)
- Core merge mechanic: 5×7 grid, drag & drop, 3 chains × 7 levels
- Energy system with offline regeneration
- Coin system
- Quest system (8 sequential quests)
- Generator system (produces items on cooldown)
- UI: energy, coins, quest display
- Editor setup tool (one-click scene setup)
- Prototype visuals (colored squares with level labels)
- Input System integration (touch + mouse)
- GDD and art prompts documented
