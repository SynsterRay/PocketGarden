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
                if (q >= 60) return Phase.Grind;
                if (q >= StoneUnlockQuest) return Phase.Stone;
                if (q >= WoodUnlockQuest) return Phase.Wood;
                return Phase.Hook;
            }
        }

        // --- Chain unlocking -------------------------------------------------

        // Quest counts at which a chain becomes available (also used as a retroactive fallback
        // so existing saves whose quest index is already past the unlock quest aren't stuck).
        // Public so QuestManager builds the ladder against the same thresholds.
        public const int WoodUnlockQuest = 10;
        public const int StoneUnlockQuest = 29;

        public static bool IsChainUnlocked(MergeChain chain)
        {
            return chain switch
            {
                MergeChain.Garden => true, // always available
                MergeChain.Wood => PlayerPrefs.GetInt(UnlockWoodKey, 0) == 1 || CompletedQuests >= WoodUnlockQuest,
                MergeChain.Stone => PlayerPrefs.GetInt(UnlockStoneKey, 0) == 1 || CompletedQuests >= StoneUnlockQuest,
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
            Phase.Hook => 30,   // ~120/hr — very generous early so the player never stalls
            Phase.Wood => 60,   // ~60/hr
            Phase.Stone => 120, // ~30/hr
            _ => 180            // ~20/hr — the grind, where packs/gems become attractive
        };

        // --- Generator pacing ------------------------------------------------

        /// <summary>Per-chain generator cooldown (seconds). Garden is snappy so seeds never stall early.</summary>
        public static float GeneratorCooldown(MergeChain chain) => chain switch
        {
            MergeChain.Garden => 8f,
            MergeChain.Wood => 15f,
            MergeChain.Stone => 22f,
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
