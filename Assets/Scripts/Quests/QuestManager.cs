using UnityEngine;
using System.Collections.Generic;
using PocketGarden.Core;

namespace PocketGarden.Quests
{
    [System.Serializable]
    public class Quest
    {
        public string id;
        public string requiredItemId;
        public int requiredAmount;
        public int currentAmount;
        public int coinReward;
        public int gemReward;
        public string description;
        public MergeChain? unlocksChain; // completing this quest unlocks a chain

        public bool IsComplete => currentAmount >= requiredAmount;
    }

    public class QuestManager : MonoBehaviour
    {
        private List<Quest> _activeQuests = new();
        private int _questIndex;

        public IReadOnlyList<Quest> ActiveQuests => _activeQuests;
        public static event System.Action OnQuestUpdated;
        public static event System.Action<Quest> OnQuestComplete;

        private const int TotalQuests = 100;

        private static readonly string[] GardenNames = { "Seed", "Sprout", "Flower", "Bush", "Tree", "Big Tree", "Magic Tree" };
        private static readonly string[] WoodNames   = { "Twig", "Log", "Plank", "Crate", "Furniture", "Gazebo", "House" };
        private static readonly string[] StoneNames  = { "Pebble", "Stone", "Brick", "Wall", "Pillar", "Fountain", "Castle" };

        private static readonly Quest[] AllQuests = BuildQuests();

        /// <summary>
        /// Builds a 100-quest ladder procedurally with a gentle ramp:
        ///   • Quests 1–~10 are tiny (1 low-level item) so the opening flies by and hooks the player.
        ///   • Wood unlocks at quest 10, Stone at quest 30 (same thresholds as Progression).
        ///   • Amounts / item levels / rewards scale up smoothly toward the grind.
        ///   • Early quests and every 10th quest grant gems to fuel the gem sinks.
        /// </summary>
        private static Quest[] BuildQuests()
        {
            int wu = Core.Progression.WoodUnlockQuest;   // 10
            int su = Core.Progression.StoneUnlockQuest;  // 30
            var list = new List<Quest>(TotalQuests);

            for (int i = 1; i <= TotalQuests; i++)
            {
                bool woodOpen = i > wu;
                bool stoneOpen = i > su;

                // Rotate among unlocked chains for variety.
                var chains = new List<MergeChain> { MergeChain.Garden };
                if (woodOpen) chains.Add(MergeChain.Wood);
                if (stoneOpen) chains.Add(MergeChain.Stone);
                var chain = chains[i % chains.Count];

                float t = (i - 1) / (float)(TotalQuests - 1); // 0..1

                int level;
                if (i <= 10)
                {
                    // Hook: gentle ramp Sprout(2) → Flower(3) → Bush(4) = fast, easy first quests.
                    level = Mathf.Clamp(2 + (i - 1) / 4, 2, 4); // i1-4:2, i5-8:3, i9-10:4
                }
                else
                {
                    int maxLevel = Mathf.Clamp(2 + Mathf.RoundToInt(t * 5f), 2, 7);
                    int minLevel = Mathf.Max(2, maxLevel - 2);
                    int span = Mathf.Max(1, maxLevel - minLevel + 1);
                    level = Mathf.Clamp(minLevel + (i % span), 1, 7);
                }

                int amount = i <= 5 ? 1 : i <= 10 ? 2 : Mathf.Clamp(1 + i / 18, 1, 5);

                int coin = 20 + level * amount * 15 + i * 6;

                int gem = 0;
                if (i <= 8) gem += 5;                       // early boost
                if (i % 10 == 0) gem += 20 + (i / 10) * 5;  // milestone every 10

                MergeChain? unlock = i == wu ? MergeChain.Wood
                                   : i == su ? MergeChain.Stone
                                   : (MergeChain?)null;

                string name = ItemName(chain, level);
                string desc = $"Deliver {amount} {name}" + (amount > 1 ? "s" : "");

                list.Add(new Quest
                {
                    id = $"q{i}",
                    requiredItemId = $"{Prefix(chain)}_{level}",
                    requiredAmount = amount,
                    coinReward = coin,
                    gemReward = gem,
                    description = desc,
                    unlocksChain = unlock
                });
            }
            return list.ToArray();
        }

        private static string Prefix(MergeChain c) => c switch
        {
            MergeChain.Wood => "wood",
            MergeChain.Stone => "stone",
            _ => "garden"
        };

        private static string ItemName(MergeChain c, int level)
        {
            int idx = Mathf.Clamp(level - 1, 0, 6);
            return c switch
            {
                MergeChain.Wood => WoodNames[idx],
                MergeChain.Stone => StoneNames[idx],
                _ => GardenNames[idx]
            };
        }

        private void Start()
        {
            _questIndex = PlayerPrefs.GetInt("MG_QuestIndex", 0);
            LoadNextQuests();
        }

        private void LoadNextQuests()
        {
            _activeQuests.Clear();
            // Show up to 2 quests at a time
            for (int i = 0; i < 2 && _questIndex + i < AllQuests.Length; i++)
            {
                var template = AllQuests[_questIndex + i];
                _activeQuests.Add(new Quest
                {
                    id = template.id,
                    requiredItemId = template.requiredItemId,
                    requiredAmount = template.requiredAmount,
                    currentAmount = 0,
                    coinReward = template.coinReward,
                    gemReward = template.gemReward,
                    description = template.description,
                    unlocksChain = template.unlocksChain
                });
            }
            OnQuestUpdated?.Invoke();
        }

        /// <summary>Try to deliver an item to a quest. Returns true if accepted.</summary>
        public bool TryDeliver(MergeItemData item)
        {
            foreach (var quest in _activeQuests)
            {
                if (!quest.IsComplete && quest.requiredItemId == item.id)
                {
                    quest.currentAmount++;
                    OnQuestUpdated?.Invoke();

                    if (quest.IsComplete)
                    {
                        CoinSystem.Add(quest.coinReward);
                        if (quest.gemReward > 0) GemSystem.Add(quest.gemReward);
                        if (quest.unlocksChain.HasValue)
                            Progression.UnlockChain(quest.unlocksChain.Value);
                        OnQuestComplete?.Invoke(quest);
                        _questIndex++;
                        PlayerPrefs.SetInt("MG_QuestIndex", _questIndex);
                        PlayerPrefs.Save();
                        LoadNextQuests();
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
