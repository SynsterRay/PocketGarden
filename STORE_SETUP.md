# Pocket Garden — Store & Monetization Object Setup

Everything you must create **outside** the codebase (Google Play Console, AdMob) for the shop
and offers to work in production. Product IDs below **must match** `Core/ShopCatalog.cs` exactly.

> Status: **IAP is wired** via Unity Purchasing (`Core/IAPManager.cs`) — purchases are declared
> from `ShopCatalog` and granted in `ProcessPurchase`. In the Editor / when the store is
> unavailable, the Shop and Offer panels fall back to granting directly so you can test.
> **Ads** (`Ads/AdManager.cs`) are guarded by the scripting define **`POCKETGARDEN_ADS`**: add the
> Google Mobile Ads SDK, then add that define (Player → Scripting Define Symbols) to activate
> them. Without it, ad calls are safe no-ops. Replace the placeholder Ad Unit IDs in `ShopCatalog`.

---

## 1. In-App Products (Google Play Console → Monetize → Products)

### 1a. In-app products — **Consumables** (repeatable; player can buy many times)
Create each as an **In-app product**. They are consumed on grant (call `ConfirmPendingPurchase`).

| Product ID | Price (USD anchor) | Store title | Contents granted | Store description |
|------------|--------------------|-------------|------------------|-------------------|
| `com.wondermindsgames.pocketgarden.energy30` | $0.99 | Energy Refill | ⚡60 energy | "Refill your energy and keep merging right away." |
| `com.wondermindsgames.pocketgarden.energy200` | $4.99 | Big Energy + Gems | ⚡250 energy, 💎50 gems | "A big energy boost plus 50 gems — power through a long session." |
| `com.wondermindsgames.pocketgarden.gems100` | $0.99 | Pocket of Gems | 💎100 gems | "A handy pocket of gems for speed-ups and treats." |
| `com.wondermindsgames.pocketgarden.gems500` | $4.99 | Bag of Gems | 💎550 gems (+10%) | "550 gems — 10% extra value." |
| `com.wondermindsgames.pocketgarden.gems1200` | $9.99 | Chest of Gems | 💎1300 gems (+20%) | "Best value! 1300 gems with a 20% bonus." |
| `com.wondermindsgames.pocketgarden.gems2800` | $19.99 | Crate of Gems | 💎2800 gems (+30%) | "2800 gems — 30% bonus for serious gardeners." |
| `com.wondermindsgames.pocketgarden.gems7500` | $49.99 | Vault of Gems | 💎7500 gems (+40%) | "Our biggest gem pack — 40% bonus." |
| `com.wondermindsgames.pocketgarden.growthbundle` | $6.99 | Gardener's Bundle | ⚡120, 💎350, 🪙1500 | "Keep your pace! Energy, gems and coins to power the big builds." |

### 1b. In-app product — **One-time / non-consumable** (purchasable once per account)
Create as an **In-app product**, then **do not consume** it; check entitlement on launch.

| Product ID | Price | Store title | Contents | Store description |
|------------|-------|-------------|----------|-------------------|
| `com.wondermindsgames.pocketgarden.starter` | $2.99 | Starter Pack (One-Time) | ⚡100, 💎100, 🪙500 | "A one-time welcome deal — 60% off. Best start for your garden!" |

**One-time enforcement checklist:**
- In Console it is a normal managed product; "one-time" is enforced by **not consuming** it and by
  hiding the offer once owned.
- Client: query owned products on launch; if `starter` is owned, never show the Starter offer again
  (the offer code already guards with `PlayerPrefs "PG_StarterOffered"`, but the **source of truth
  must be the Play entitlement**, not PlayerPrefs, to survive reinstalls).

### 1c. Subscription (Google Play Console → Monetize → Subscriptions)
| Product ID | Base plan | Price | Title | Description |
|------------|-----------|-------|-------|-------------|
| `com.wondermindsgames.pocketgarden.vip` | `vip-monthly` (auto-renew, 1 month) | $7.99/mo | VIP Pass | "VIP Pass: +max energy, faster regen, exclusive décor, and no interstitial ads." |

**VIP entitlements to implement** (not yet in code): higher max energy, faster regen multiplier,
suppress interstitials, exclusive decorations. Gate via an active-subscription check.

---

## 2. Pricing & localization
- Set the USD anchors above; let Google auto-convert, or use **price templates** per country.
- Translate titles/descriptions to the launch languages from the GDD: **PL, EN, ES, PT, DE, FR**.
- The $9.99 "Chest of Gems" is the intended **conversion sweet spot** — keep the "Best value" badge.

## 3. AdMob (replace placeholders in `Core/ShopCatalog.cs`)
Create one app + three ad units in AdMob, then paste the IDs over these constants:

| Constant | Ad format | Used for |
|----------|-----------|----------|
| `AD_BANNER` | Banner (adaptive, bottom) | Persistent banner on the board |
| `AD_INTERSTITIAL` | Interstitial | After every 3rd completed quest |
| `AD_REWARDED` | Rewarded | "Watch Ad = +10 Energy" (Shop) and energy top-ups |

> Use Google **test ad unit IDs** during development; swap to real IDs before release.

---

## 4. Where offers appear (already implemented)
`UI/OfferManager.cs` shows a single contextual pack at stall points:
- **Out of energy** → `energy30` (or `growthbundle` in the grind phase).
- **Reaching quest 11** (Stone unlock) → one-time `starter`.
- **Reaching quest 17** (grind) → `growthbundle`.

All offers have a clear **"No thanks"**, a 150s cooldown, and the Starter is shown once.

## 5. Release checklist
- [ ] Create all products above with **exact IDs** (they already match `ShopCatalog`).
- [x] Unity Purchasing integrated (`IAPManager`) — fulfilment behind `ProcessPurchase`.
- [ ] Implement Play entitlement check for `starter` (one-time) and `vip` (subscription) — see `IAPManager.IsOwned`; make it the source of truth over PlayerPrefs.
- [ ] Import Google Mobile Ads SDK, add scripting define **`POCKETGARDEN_ADS`**, replace placeholder Ad Unit IDs.
- [ ] Localize store listings (PL/EN/ES/PT/DE/FR).
- [ ] Verify restore-purchases flow (non-consumable + subscription).
- [ ] Run **PocketGarden → Generate Placeholder Art** (or drop final art) so HUD icons render.
