using UnityEngine;
using System.Collections.Generic;

namespace PocketGarden.Core
{
    public enum ShopItemType { Energy, GemPack, StarterPack, VIP }

    [System.Serializable]
    public class ShopItem
    {
        public string id;
        public ShopItemType type;
        public string iapProductId;
        public int energyAmount;
        public int gemAmount;
        public int coinAmount;
        public string displayName;
        public string priceLabel;
        public string tag;       // e.g. "BEST VALUE", "MOST POPULAR", "-60%"
    }

    public static class ShopCatalog
    {
        // IAP Product IDs — replace with real ones before production
        public const string IAP_ENERGY_SMALL = "com.wondermindsgames.pocketgarden.energy30";
        public const string IAP_ENERGY_BIG = "com.wondermindsgames.pocketgarden.energy200";
        public const string IAP_GEMS_100 = "com.wondermindsgames.pocketgarden.gems100";
        public const string IAP_GEMS_500 = "com.wondermindsgames.pocketgarden.gems500";
        public const string IAP_GEMS_1200 = "com.wondermindsgames.pocketgarden.gems1200";
        public const string IAP_GEMS_2800 = "com.wondermindsgames.pocketgarden.gems2800";
        public const string IAP_GEMS_7500 = "com.wondermindsgames.pocketgarden.gems7500";
        public const string IAP_STARTER = "com.wondermindsgames.pocketgarden.starter";
        public const string IAP_BUNDLE_GROWTH = "com.wondermindsgames.pocketgarden.growthbundle";
        public const string IAP_VIP = "com.wondermindsgames.pocketgarden.vip";

        // Ad Unit IDs — replace with real ones before production
        public const string AD_BANNER = "ca-app-pub-XXXXXXX/YYYYYYY";
        public const string AD_INTERSTITIAL = "ca-app-pub-XXXXXXX/ZZZZZZZ";
        public const string AD_REWARDED = "ca-app-pub-XXXXXXX/WWWWWWW";

        // Prices follow common mobile merge-game anchors: $0.99 impulse, $9.99 "best value"
        // conversion sweet spot, scaling gem bonuses, and a discounted one-time Starter.
        public static readonly List<ShopItem> Items = new()
        {
            // Energy refills
            new() { id = "energy_small", type = ShopItemType.Energy, iapProductId = IAP_ENERGY_SMALL,
                    energyAmount = 60, displayName = "Energy Refill x60", priceLabel = "$0.99" },
            new() { id = "energy_big", type = ShopItemType.Energy, iapProductId = IAP_ENERGY_BIG,
                    energyAmount = 250, gemAmount = 50, displayName = "Energy x250 + 50 Gems", priceLabel = "$4.99", tag = "POPULAR" },

            // Gem packs — bonus % grows with tier
            new() { id = "gems_100",  type = ShopItemType.GemPack, iapProductId = IAP_GEMS_100,
                    gemAmount = 100,  displayName = "100 Gems", priceLabel = "$0.99" },
            new() { id = "gems_500",  type = ShopItemType.GemPack, iapProductId = IAP_GEMS_500,
                    gemAmount = 550,  displayName = "550 Gems (+10%)", priceLabel = "$4.99" },
            new() { id = "gems_1200", type = ShopItemType.GemPack, iapProductId = IAP_GEMS_1200,
                    gemAmount = 1300, displayName = "1300 Gems (+20%)", priceLabel = "$9.99", tag = "BEST VALUE" },
            new() { id = "gems_2800", type = ShopItemType.GemPack, iapProductId = IAP_GEMS_2800,
                    gemAmount = 2800, displayName = "2800 Gems (+30%)", priceLabel = "$19.99" },
            new() { id = "gems_7500", type = ShopItemType.GemPack, iapProductId = IAP_GEMS_7500,
                    gemAmount = 7500, displayName = "7500 Gems (+40%)", priceLabel = "$49.99" },

            // Bundles
            new() { id = "starter", type = ShopItemType.StarterPack, iapProductId = IAP_STARTER,
                    energyAmount = 100, gemAmount = 100, coinAmount = 500, displayName = "Starter Pack", priceLabel = "$2.99", tag = "ONE-TIME -60%" },
            new() { id = "growth_bundle", type = ShopItemType.StarterPack, iapProductId = IAP_BUNDLE_GROWTH,
                    energyAmount = 120, gemAmount = 350, coinAmount = 1500, displayName = "Gardener's Bundle", priceLabel = "$6.99", tag = "GREAT DEAL" },

            // Subscription
            new() { id = "vip", type = ShopItemType.VIP, iapProductId = IAP_VIP,
                    displayName = "VIP Pass (Monthly)", priceLabel = "$7.99/mo" },
        };

        public static ShopItem Get(string id) => Items.Find(i => i.id == id);

        /// <summary>Grants a purchased item's contents. Shared by ShopUI and OfferManager.
        /// Replace the body with real IAP fulfilment when wiring Unity Purchasing.</summary>
        public static void GrantPurchase(ShopItem item)
        {
            if (item == null) return;
            if (item.energyAmount > 0) EnergySystem.Add(item.energyAmount);
            if (item.gemAmount > 0) GemSystem.Add(item.gemAmount);
            if (item.coinAmount > 0) CoinSystem.Add(item.coinAmount);
        }
    }
}
