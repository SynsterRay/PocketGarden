using UnityEngine;

namespace PocketGarden.Core
{
    /// <summary>
    /// Central pacing controller. Drives the "fast hook → slowdown → paywall" curve and
    /// tracks which merge chains the player has unlocked through quest progression.
    ///
    /// Phases (by completed-quest index, key "MG_QuestIndex"):
    ///   Hook   (0-5)   : Garden only. Fast generators, fast energy regen, generous rewards.
    ///   Wood   (6-10)  : Wood chain unlocked. Mature trees start producing Logs.
    ///   Stone  (11-16) : Stone chain unlocked. Requirements escalate, regen slows.
    ///   Grind  (17+)   : Endgame. High-level deliveries gate progress behind energy / packs.
    /// </summary>
    public static class Progression
    {
        public enum Phase { Hook, Wood, Stone, Grind }

        private const string QuestIndexKey = "MG_QuestIndex";
        private const string UnlockWoodKey = "PG_Unlock_Wood";
        private const string UnlockStoneKey = "PG_Unlock_Stone";

        /// <summary>Fired when a chain is unlocked so generators/producers can react.</summary>
        public static event System.Action<MergeChain> OnChainUnlocked;

        public static int CompletedQuests => PlayerPrefs.GetInt(QuestIndexKey, 0);

        public static Phase CurrentPhase
        {
            get
            {
                int q = CompletedQuests;
                if (q >= 17) return Phase.Grind;
                if (q >= 11) return Phase.Stone;
                if (q >= 6) return Phase.Wood;
                return Phase.Hook;
            }
        }

        // --- Chain unlocking -------------------------------------------------

        public static bool IsChainUnlocked(MergeChain chain)
        {
            return chain switch
            {
                MergeChain.Garden => true, // always available
                MergeChain.Wood => PlayerPrefs.GetInt(UnlockWoodKey, 0) == 1,
                MergeChain.Stone => PlayerPrefs.GetInt(UnlockStoneKey, 0) == 1,
                _ => false
            };
        }

        public static void UnlockChain(MergeChain chain)
        {
            if (IsChainUnlocked(chain)) return;
            switch (chain)
            {
                case MergeChain.Wood: PlayerPrefs.SetInt(UnlockWoodKey, 1); break;
                case MergeChain.Stone: PlayerPrefs.SetInt(UnlockStoneKey, 1); break;
                default: return;
            }
            PlayerPrefs.Save();
            OnChainUnlocked?.Invoke(chain);
        }

        /// <summary>Maps an item id (e.g. "wood_2") to its chain.</summary>
        public static MergeChain ChainOf(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return MergeChain.Garden;
            if (itemId.StartsWith("wood")) return MergeChain.Wood;
            if (itemId.StartsWith("stone")) return MergeChain.Stone;
            return MergeChain.Garden;
        }

        // --- Energy pacing ---------------------------------------------------

        /// <summary>Seconds to regenerate 1 energy. Fast early to hook, slower in the grind.</summary>
        public static int EnergyRegenSeconds => CurrentPhase switch
        {
            Phase.Hook => 45,   // ~80/hr — generous, keeps new players merging
            Phase.Wood => 90,   // ~40/hr
            Phase.Stone => 150, // ~24/hr
            _ => 180            // ~20/hr — the grind, where packs become attractive
        };

        // --- Generator pacing ------------------------------------------------

        /// <summary>Per-chain generator cooldown (seconds). Garden is snappy so seeds never stall early.</summary>
        public static float GeneratorCooldown(MergeChain chain) => chain switch
        {
            MergeChain.Garden => 8f,
            MergeChain.Wood => 16f,
            MergeChain.Stone => 26f,
            _ => 30f
        };

        /// <summary>Generator use budget before it must be replaced (kept generous to avoid hard walls).</summary>
        public static int GeneratorUses(MergeChain chain) => chain switch
        {
            MergeChain.Garden => 400,
            MergeChain.Wood => 250,
            MergeChain.Stone => 200,
            _ => 200
        };
    }
}
