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
        public string description;

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
            new() { id = "q1", requiredItemId = "garden_4", requiredAmount = 2, coinReward = 50, description = "Deliver 2 Flowers" },
            new() { id = "q2", requiredItemId = "garden_5", requiredAmount = 1, coinReward = 100, description = "Deliver 1 Tree" },
            new() { id = "q3", requiredItemId = "wood_3", requiredAmount = 3, coinReward = 150, description = "Deliver 3 Planks" },
            new() { id = "q4", requiredItemId = "wood_5", requiredAmount = 2, coinReward = 200, description = "Deliver 2 Furniture" },
            new() { id = "q5", requiredItemId = "stone_6", requiredAmount = 1, coinReward = 300, description = "Deliver 1 Fountain" },
            new() { id = "q6", requiredItemId = "garden_6", requiredAmount = 2, coinReward = 400, description = "Deliver 2 Big Trees" },
            new() { id = "q7", requiredItemId = "stone_7", requiredAmount = 1, coinReward = 500, description = "Deliver 1 Castle" },
            new() { id = "q8", requiredItemId = "garden_7", requiredAmount = 1, coinReward = 600, description = "Deliver 1 Magic Tree" },
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
                    description = template.description
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
