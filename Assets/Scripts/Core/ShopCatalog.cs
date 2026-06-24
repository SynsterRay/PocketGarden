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
    }

    public static class ShopCatalog
    {
        // IAP Product IDs — replace with real ones before production
        public const string IAP_ENERGY_SMALL = "com.wondermindsgames.pocketgarden.energy30";
        public const string IAP_ENERGY_BIG = "com.wondermindsgames.pocketgarden.energy200";
        public const string IAP_GEMS_100 = "com.wondermindsgames.pocketgarden.gems100";
        public const string IAP_GEMS_500 = "com.wondermindsgames.pocketgarden.gems500";
        public const string IAP_GEMS_1500 = "com.wondermindsgames.pocketgarden.gems1500";
        public const string IAP_STARTER = "com.wondermindsgames.pocketgarden.starter";
        public const string IAP_VIP = "com.wondermindsgames.pocketgarden.vip";

        // Ad Unit IDs — replace with real ones before production
        public const string AD_BANNER = "ca-app-pub-XXXXXXX/YYYYYYY";
        public const string AD_INTERSTITIAL = "ca-app-pub-XXXXXXX/ZZZZZZZ";
        public const string AD_REWARDED = "ca-app-pub-XXXXXXX/WWWWWWW";

        public static readonly List<ShopItem> Items = new()
        {
            new() { id = "energy_small", type = ShopItemType.Energy, iapProductId = IAP_ENERGY_SMALL,
                    energyAmount = 30, displayName = "Energy x30", priceLabel = "$0.99" },
            new() { id = "energy_big", type = ShopItemType.Energy, iapProductId = IAP_ENERGY_BIG,
                    energyAmount = 200, gemAmount = 50, displayName = "Energy x200 + 50 Gems", priceLabel = "$4.99" },
            new() { id = "gems_100", type = ShopItemType.GemPack, iapProductId = IAP_GEMS_100,
                    gemAmount = 100, displayName = "100 Gems", priceLabel = "$1.99" },
            new() { id = "gems_500", type = ShopItemType.GemPack, iapProductId = IAP_GEMS_500,
                    gemAmount = 500, displayName = "500 Gems", priceLabel = "$4.99" },
            new() { id = "gems_1500", type = ShopItemType.GemPack, iapProductId = IAP_GEMS_1500,
                    gemAmount = 1500, displayName = "1500 Gems", priceLabel = "$9.99" },
            new() { id = "starter", type = ShopItemType.StarterPack, iapProductId = IAP_STARTER,
                    energyAmount = 100, gemAmount = 100, coinAmount = 500, displayName = "Starter Pack", priceLabel = "$2.99" },
            new() { id = "vip", type = ShopItemType.VIP, iapProductId = IAP_VIP,
                    displayName = "VIP Pass (Monthly)", priceLabel = "$4.99/mo" },
        };
    }
}
