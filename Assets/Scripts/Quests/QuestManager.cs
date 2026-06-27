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

        private static readonly Quest[] AllQuests =
        {
            // ── Phase 1: HOOK (Garden only) — small asks, fast generous rewards ──
            new() { id = "q1",  requiredItemId = "garden_3", requiredAmount = 2, coinReward = 50,  description = "Deliver 2 Flowers" },
            new() { id = "q2",  requiredItemId = "garden_4", requiredAmount = 1, coinReward = 60,  description = "Deliver 1 Bush" },
            new() { id = "q3",  requiredItemId = "garden_3", requiredAmount = 3, coinReward = 80,  description = "Deliver 3 Flowers" },
            new() { id = "q4",  requiredItemId = "garden_5", requiredAmount = 1, coinReward = 120, gemReward = 10, description = "Grow 1 Tree" },
            new() { id = "q5",  requiredItemId = "garden_4", requiredAmount = 2, coinReward = 120, description = "Deliver 2 Bushes" },
            new() { id = "q6",  requiredItemId = "garden_6", requiredAmount = 1, coinReward = 200, description = "Grow 1 Big Tree", unlocksChain = MergeChain.Wood },

            // ── Phase 2: WOOD (trees now produce Logs) ──
            new() { id = "q7",  requiredItemId = "wood_2", requiredAmount = 3, coinReward = 150, description = "Deliver 3 Logs" },
            new() { id = "q8",  requiredItemId = "wood_3", requiredAmount = 2, coinReward = 200, description = "Deliver 2 Planks" },
            new() { id = "q9",  requiredItemId = "wood_4", requiredAmount = 1, coinReward = 250, gemReward = 15, description = "Build 1 Crate" },
            new() { id = "q10", requiredItemId = "garden_7", requiredAmount = 1, coinReward = 400, description = "Grow 1 Magic Tree" },
            new() { id = "q11", requiredItemId = "wood_5", requiredAmount = 2, coinReward = 400, description = "Build 2 Furniture", unlocksChain = MergeChain.Stone },

            // ── Phase 3: STONE + all chains — escalating, regen slows ──
            new() { id = "q12", requiredItemId = "stone_3", requiredAmount = 3, coinReward = 300, description = "Deliver 3 Bricks" },
            new() { id = "q13", requiredItemId = "stone_4", requiredAmount = 1, coinReward = 400, gemReward = 20, description = "Build 1 Wall" },
            new() { id = "q14", requiredItemId = "wood_6", requiredAmount = 1, coinReward = 500, description = "Build 1 Gazebo" },
            new() { id = "q15", requiredItemId = "stone_5", requiredAmount = 1, coinReward = 600, description = "Raise 1 Pillar" },
            new() { id = "q16", requiredItemId = "wood_7", requiredAmount = 1, coinReward = 800, description = "Build 1 House" },

            // ── Phase 4: GRIND / endgame — heavy asks (energy & packs matter) ──
            new() { id = "q17", requiredItemId = "stone_6", requiredAmount = 1, coinReward = 800,  description = "Build 1 Fountain" },
            new() { id = "q18", requiredItemId = "garden_7", requiredAmount = 2, coinReward = 1000, description = "Grow 2 Magic Trees" },
            new() { id = "q19", requiredItemId = "wood_7", requiredAmount = 2, coinReward = 1200, description = "Build 2 Houses" },
            new() { id = "q20", requiredItemId = "stone_7", requiredAmount = 1, coinReward = 1500, gemReward = 50, description = "Build 1 Castle" },
            new() { id = "q21", requiredItemId = "stone_6", requiredAmount = 2, coinReward = 1800, description = "Build 2 Fountains" },
            new() { id = "q22", requiredItemId = "stone_7", requiredAmount = 2, coinReward = 2500, description = "Build 2 Castles" },
        };

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
