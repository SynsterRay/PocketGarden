# Pocket Garden — TODO

Outstanding work, grouped by priority. Checked items are done; unchecked remain.

## 🔴 Blockers before a build/test
- [ ] Open in Unity and **verify compile** (changes made outside the editor).
- [ ] Run **PocketGarden → Generate Placeholder Art** so HUD shows icons (else emoji fallback).
- [ ] Quick Play test: hook phase (Garden), Wood unlock at quest 6, Stone at quest 11, offers fire.

## 🟠 Monetization / store
- [x] IAP wired via Unity Purchasing (`IAPManager`, products from `ShopCatalog`).
- [x] Contextual offer panels (`OfferManager`) at stall points.
- [x] Market-rate pricing + value tags in `ShopCatalog`.
- [ ] Create all products in **Google Play Console** with exact IDs (see `STORE_SETUP.md`).
- [ ] Make Play **entitlement** the source of truth for one-time `starter` and `vip`
      (currently PlayerPrefs; use `IAPManager.IsOwned`). Survive reinstalls.
- [ ] Implement **restore purchases** flow (non-consumable + subscription).
- [ ] Implement **VIP** entitlements: +max energy, faster regen, no interstitials, exclusive décor.

## 🟠 Ads
- [x] `AdManager` with banner/interstitial/rewarded, guarded by `POCKETGARDEN_ADS`.
- [ ] Import **Google Mobile Ads SDK**, add `POCKETGARDEN_ADS` define.
- [ ] Replace placeholder **Ad Unit IDs** in `ShopCatalog` (use test IDs in dev).
- [ ] Verify banner safe-area / it doesn't overlap the quest drop zone.

## 🟡 Economy gaps
- [x] **Gem sinks (first two)**: energy refill (HUD energy chip, Shop, energy offer) and
      generator cooldown skip (tap a not-ready generator) — `GemEconomy`, `GemConfirmPopup`.
- [ ] More gem sinks: buy/refresh a generator, skip a chain-unlock requirement, premium décor.
- [ ] **Generator replacement**: generators have limited uses; add buy-new-generator
      (coins/gems) when exhausted (GDD §3.3).
- [ ] Balance pass on numbers (regen seconds, generator cooldowns, quest requirements,
      pack contents) once playtested / with analytics.

## 🟡 Art (see `ART_ASSETS.md`)
- [ ] 21 merge item sprites → `Resources/Items/{id}` (auto-loaded).
- [ ] Final HUD icons (overwrite generated placeholders).
- [ ] Board + menu backgrounds, logo, app icon.
- [ ] Optional generator sprites.

## 🟢 Features not yet built
- [ ] **Garden decoration** meta-game (separate screen) — core retention loop per GDD §5.
- [ ] **Localization** (PL/EN/ES/PT/DE/FR) — UI strings are hardcoded English now.
- [ ] **Sound / music** (merge pop, quest fanfare, generator ready, UI clicks).
- [ ] **Push notifications** (generator ready, daily bonus) — `com.unity.mobile.notifications`.
- [ ] **Splash screen** (Wonder Minds Games logo).
- [ ] Analytics (Firebase / Unity Analytics) for funnel + balance.

## 🟢 Polish / UX
- [ ] Replace emoji in labels/menus with sprite icons (some devices show boxes).
- [ ] Merge feedback: pop/particle on merge, level-up sparkle.
- [ ] Quest complete celebration + reward fly-to-HUD animation.
- [ ] Settings: actually wire Music/SFX to audio buses (currently a global volume stub).
- [ ] "Board full" hint nudging the player to merge/deliver.
- [ ] Tutorial: highlight generators + drop zone interactively (currently static pages).

## ✅ Recently completed (2026-06-27)
- [x] Garden chain reorder (Flower before Bush).
- [x] Producer items (mature trees produce Logs) — gated on Wood unlock.
- [x] Progression curve (Hook → Wood → Stone → Grind) + phase-based energy regen.
- [x] Chain gating + 22-mission quest ladder with gem/unlock rewards.
- [x] HUD with Energy + Coins + **Gems**; consolidated, non-overlapping top bar.
- [x] `UIFactory` shared builders; scrollable Shop; tidy Settings dialog.
- [x] Item icons auto-load from `Resources/Items/{id}`; placeholders reserve final footprint.
- [x] Docs: `ART_ASSETS.md`, `STORE_SETUP.md`.
