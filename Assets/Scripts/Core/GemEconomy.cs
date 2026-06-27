namespace PocketGarden.Core
{
    /// <summary>
    /// Gem spending sinks (gives gems a purpose so the soft currency is two-sided):
    ///   • Energy refill — instantly top energy to max.
    ///   • Generator skip — finish a generator's cooldown immediately (spent in the tap handler).
    /// </summary>
    public static class GemEconomy
    {
        public const int EnergyRefillCost = 50;
        public const int GeneratorSkipCost = 10;

        public static bool CanAfford(int cost) => GemSystem.Gems >= cost;

        /// <summary>Spend gems to fully refill energy. Returns false if already full or not enough gems.</summary>
        public static bool TryRefillEnergy()
        {
            if (EnergySystem.IsFull) return false;
            if (!GemSystem.Spend(EnergyRefillCost)) return false;
            EnergySystem.Refill();
            return true;
        }
    }
}
