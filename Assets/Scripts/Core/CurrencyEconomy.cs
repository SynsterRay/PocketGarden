namespace PocketGarden.Core
{
    /// <summary>
    /// Currency sinks for coins and gems:
    /// 
    /// Coins can buy:
    ///   • Energy refill (cost: 50 coins = 30 energy, ~1.67 coins/energy)
    ///   • Gems (cost: 100 coins = 10 gems, 10 coins/gem)
    /// 
    /// Gems can buy:
    ///   • Energy refill (cost: 50 gems = 30 energy)
    ///   • Generator skip (cost: 10 gems)
    /// 
    /// Pricing rationale:
    ///   • Coins are earned faster (quest rewards, daily bonus)
    ///   • Gems are premium (IAP, milestones)
    ///   • Gems have better value for energy refill (50 gems = 30 energy vs 50 coins = 30 energy)
    ///   • Gems needed for generator skip (fast pacing)
    /// </summary>
    public static class CurrencyEconomy
    {
        // --- Coin costs ---
        public const int CoinEnergyRefillCost = 50;       // 50 coins = 30 energy (1.67 coins/energy)
        public const int CoinGemBuyCost = 100;            // 100 coins = 10 gems (10 coins/gem)

        // --- Gem costs ---
        public const int EnergyRefillCost = 50;           // 50 gems = 30 energy (5 gems/energy)
        public const int GeneratorSkipCost = 10;          // 10 gems to skip cooldown

        public static bool CanAfford(int cost) => GemSystem.Gems >= cost;
        public static bool CanAffordCoins(int cost) => CoinSystem.Coins >= cost;

        /// <summary>Spend gems to fully refill energy. Returns false if already full or not enough gems.</summary>
        public static bool TryRefillEnergy()
        {
            if (EnergySystem.IsFull) return false;
            if (!GemSystem.Spend(EnergyRefillCost)) return false;
            EnergySystem.Refill();
            return true;
        }

        /// <summary>Spend gems to skip generator cooldown. Returns true if successful.</summary>
        public static bool TrySkipGenerator()
        {
            if (!GemSystem.Spend(GeneratorSkipCost)) return false;
            return true;
        }

        /// <summary>Buy gems with coins. Returns true if successful.</summary>
        public static bool BuyGemsWithCoins()
        {
            if (!CoinSystem.Spend(CoinGemBuyCost)) return false;
            GemSystem.Add(10);
            return true;
        }

        /// <summary>Buy energy with coins. Returns true if successful.</summary>
        public static bool BuyEnergyWithCoins()
        {
            if (EnergySystem.IsFull) return false;
            if (!CoinSystem.Spend(CoinEnergyRefillCost)) return false;
            EnergySystem.Add(30);
            return true;
        }
    }
}
