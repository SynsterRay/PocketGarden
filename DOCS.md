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
| Drag & Drop | ✅ | Touch + mouse input (Input System), priority handling |
| Energy System | ✅ | 30 max, +1/3min, offline regen |
| Coin System | ✅ | Currency earned from quests |
| Gem System | ✅ | Premium currency (IAP) |
| Quest System | ✅ | 8 quests, deliver by dragging to drop zone |
| Quest Drop Zone | ✅ | Green bar at bottom — drag items to deliver |
| Generators | ✅ | 3 generators (Garden/Wood/Stone) with cooldown timers |
| Generator Tap | ✅ | Tap yellow indicator to produce items |
| Save/Load | ✅ | Board state persists (PlayerPrefs + JSON, auto-save) |
| Daily Bonus | ✅ | Streak-based energy + coins popup |
| Shop UI | ✅ | IAP placeholders, rewarded ad button, buy buttons |
| Shop Catalog | ✅ | 7 IAP products + 3 Ad Unit ID placeholders |
| Settings Menu | ✅ | Music/SFX toggle, language, reset, rate us, credits |
| Tutorial | ✅ | 4-page overlay on every start |
| Energy UI | ✅ | Top-left display with tick regen |
| Quest UI | ✅ | Bottom bar with active quests + drop zone |
| Coin UI | ✅ | Top-right display |
| Item Labels | ✅ | Item names displayed on merge grid items |
| Editor Setup | ✅ | PocketGarden → Setup Scene (with EventSystem) |

### Not Yet Implemented

- [ ] Garden decoration (meta-game, separate screen)
- [ ] Ads integration (AdMob — banner, interstitial, rewarded)
- [ ] IAP integration (Unity Purchasing)
- [ ] Push Notifications
- [ ] Sound Effects / Music
- [ ] Localization (multi-language)
- [ ] Sprite assets (using colored squares as prototype)
- [ ] Seasonal events
- [ ] VIP Pass
- [ ] Analytics (Firebase)

## Repository

GitHub: https://github.com/SynsterRay/PocketGarden

## Architecture

### Namespaces

```
PocketGarden.Core       — GameManager, EnergySystem, CoinSystem, GemSystem, Progression, MergeDatabase, MergeItemData, ShopCatalog, SaveSystem
PocketGarden.Grid       — MergeGrid, GridCell, MergeGridItem, DragDropHandler, Generator
PocketGarden.Quests     — QuestManager, Quest
PocketGarden.UI         — UIFactory, HudBar, QuestUI, ShopUI, MainMenu, OfferManager, DailyBonus, TutorialOverlay
PocketGarden.Editor     — SceneSetup
```

### Merge Chains

| Chain | Lv1 | Lv2 | Lv3 | Lv4 | Lv5 | Lv6 | Lv7 |
|-------|-----|-----|-----|-----|-----|-----|-----|
| Garden | Seed | Sprout | Flower | Bush | Tree | Big Tree | Magic Tree |
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

### 2026-06-27
- **Gem sinks** added (`GemEconomy`, `GemConfirmPopup`): spend 💎 to **refill energy** (tap the HUD energy chip, Shop button, or the energy offer) and to **skip a generator's cooldown** (tap a not-ready generator). Gives gems a real use; short-on-gems routes to the Shop.
- UI/UX overhaul: new shared `UIFactory` (rounded sprites, one font, consistent builders); consolidated top `HudBar` showing Energy + Coins + **Gems** with Shop/Settings in one evenly-spaced, non-overlapping row.
- Gem counter now on the HUD (was previously invisible despite quests granting gems).
- `QuestUI` slimmed to a quest card + delivery drop zone (currencies moved to HudBar); quests show coin/gem rewards.
- `ShopUI` rebuilt as a **scrollable** list (9 SKUs no longer overflow), rounded cards with contents + value tags.
- `MainMenu` settings rebuilt as a tidy dimmed dialog card.
- Merge item icons auto-load from `Resources/Items/{id}`; colored placeholders reserve the exact final footprint so real art drops in without layout shift.
- Removed `EnergyUI` (folded into `HudBar`).
- New docs: `ART_ASSETS.md` (art prompts + dimensions) and `STORE_SETUP.md` (Google Play IAP products: consumable/one-time/subscription, prices, descriptions, AdMob units).
- **IAP wired** via Unity Purchasing (`Core/IAPManager.cs`): products declared from `ShopCatalog`, granted in `ProcessPurchase`; Shop/Offer panels fall back to direct grant in the Editor. Consumable / one-time (`starter`) / subscription (`vip`) types.
- **Ads** (`Ads/AdManager.cs`): banner/interstitial/rewarded, guarded by the `POCKETGARDEN_ADS` define (safe no-ops until the Google Mobile Ads SDK + define are added). Interstitial every 3 completed quests; rewarded `+10` energy in Shop.
- **Placeholder icon generator** (`Editor/ArtPlaceholderGenerator.cs`, menu *PocketGarden → Generate Placeholder Art*): procedurally creates `icon_energy/coin/gem/shop/settings` PNGs in `Resources/UIButtons`. HudBar loads them, with emoji fallback.
- Garden chain reorder: Seed → Sprout → **Flower → Bush** → Tree → Big Tree → Magic Tree
- Producer items (Option A): mature trees generate Logs — Tree (Lv5) 60s, Big Tree (Lv6) 45s, Magic Tree (Lv7) 30s; tap the ready tree to produce. Only active once the Wood chain is unlocked.
- New `Progression` module — central "fast hook → slowdown → paywall" curve with 4 phases (Hook/Wood/Stone/Grind) driven by completed-quest count.
- Phase-based energy regen: 45s (Hook) → 90s (Wood) → 150s (Stone) → 180s (Grind). Early game is generous; late game is the natural paywall.
- Chain gating: Garden unlocked at start; **Wood unlocks at quest 6**, **Stone unlocks at quest 11**. Generators spawn only for unlocked chains (incl. runtime unlock).
- Quest ladder expanded 8 → **22 missions** across phases, with coin + gem rewards and chain-unlock rewards. Garden-only, front-loaded starter board for an instant-progress hook.
- `OfferManager`: contextual single-pack offers at stall points — out of energy (Energy refill / Gardener's Bundle in grind), entering Stone (one-time Starter), entering Grind (Gardener's Bundle). Honest framing: clear "No thanks", one-time Starter shown once, 150s cooldown.
- Shop pricing aligned to market anchors: gems $0.99 / $4.99 / $9.99 (BEST VALUE) / $19.99 / $49.99 with scaling bonus %, Energy x60 $0.99, Energy x250+50💎 $4.99, one-time Starter $2.99 (-60%), Gardener's Bundle $6.99, VIP $7.99/mo. Shared `ShopCatalog.GrantPurchase`/`Get` (IAP fulfilment still TODO).

### 2026-06-24
- Project created (Unity 6, Universal 2D)
- Core merge mechanic: 5×7 grid, drag & drop, 3 chains × 7 levels
- Energy system with offline regeneration
- Coin + Gem currency systems
- Quest system (8 sequential quests) with drag-to-deliver drop zone
- Generator system (3 generators with cooldown timers, tap to produce)
- Save/Load board state (auto-save on merge, pause, quit)
- Daily Bonus popup (streak-based rewards)
- Shop UI with 7 IAP product placeholders + rewarded ad button
- Settings menu (music, sfx, language, reset progress, rate us, credits)
- Tutorial overlay (4 pages, shown every start)
- Item name labels on grid items (sortingOrder fix)
- Input priority: items > generators (OverlapPointAll)
- EventSystem for UI interaction
- Editor setup tool (one-click scene setup)
- GitHub repo: https://github.com/SynsterRay/PocketGarden
