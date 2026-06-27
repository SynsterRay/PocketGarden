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
        /// Builds a 100-quest ladder procedurally with variety and pacing:
        ///   • First 20 quests: only Garden items (1–4), very gentle hook
        ///   • 21–50: Wood added, 70% Garden / 30% Wood mix
        ///   • 51–100: Stone added, balanced 40/30/30 mix with more variety
        ///   • Amounts scale slowly (1→2→3→4→5), rewards scale smoothly
        ///   • Gems only on early quests (1–15) and milestones (20/40/60/80/100)
        /// </summary>
        private static Quest[] BuildQuests()
        {
            int wu = Core.Progression.WoodUnlockQuest;   // 10
            int su = Core.Progression.StoneUnlockQuest;  // 30
            var list = new List<Quest>(TotalQuests);

            for (int i = 1; i <= TotalQuests; i++)
            {
                bool woodOpen = i >= wu;
                bool stoneOpen = i >= su;

                // Determine chain based on quest tier for better variety
                MergeChain chain;
                if (i <= 20)
                {
                    chain = MergeChain.Garden; // Only garden in first 20 quests
                }
                else if (i <= 50)
                {
                    // Garden/Wood mix
                    chain = (i % 3 == 0 && woodOpen) ? MergeChain.Wood : MergeChain.Garden;
                }
                else
                {
                    // Balanced 40/30/30
                    int r = i % 10;
                    if (r == 0 || r == 1 || r == 2 || r == 3) chain = MergeChain.Garden;
                    else if (r == 4 || r == 5 || r == 6) chain = MergeChain.Wood;
                    else chain = MergeChain.Stone;
                }

                // Level ramping: slow early, faster mid, plateau late
                int level;
                if (i <= 15)
                {
                    level = 2 + (i - 1) / 4; // 2,2,2,3,3,3,3,4,4,4,4,5,5,5,5
                }
                else if (i <= 60)
                {
                    level = 5 + (i - 15) / 9; // 5→6→7→7→7→7→7→7→7
                }
                else
                {
                    level = 7; // Cap at max level
                }

                int amount;
                if (i <= 10) amount = 1;
                else if (i <= 30) amount = 2;
                else if (i <= 60) amount = 3;
                else if (i <= 80) amount = 4;
                else amount = 5;

                int coin = 25 + level * amount * 12 + i * 5;

                int gem = 0;
                if (i <= 15) gem += 5;                       // early boost
                if (i == 20 || i == 40 || i == 60 || i == 80 || i == 100) gem += 25 + (i / 20) * 10;

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
