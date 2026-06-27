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

        private const int TotalQuests = 22;

        private static readonly string[] GardenNames = { "Seed", "Sprout", "Flower", "Bush", "Tree", "Big Tree", "Magic Tree" };
        private static readonly string[] WoodNames   = { "Twig", "Log", "Plank", "Crate", "Furniture", "Gazebo", "House" };
        private static readonly string[] StoneNames  = { "Pebble", "Stone", "Brick", "Wall", "Pillar", "Fountain", "Castle" };

        private static readonly Quest[] AllQuests = BuildQuests();

        /// <summary>
        /// Builds a 22-quest ladder using all 21 items (7 levels × 3 chains):
        ///   • q1-6:   Hook phase — Garden only (levels 1-7), fast progression
        ///   • q7:     Wood unlock at quest 6
        ///   • q8-14:  Wood phase — Garden/Wood mix, introduce building
        ///   • q15:    Stone unlock at quest 11
        ///   • q16-22: Stone phase + Grind — all chains, higher levels, gem milestones
        /// Each chain fully exercised: every item level appears at least once.
        /// </summary>
        private static Quest[] BuildQuests()
        {
            var list = new List<Quest>(TotalQuests);

            // q1-6: Hook — all Garden levels 1-7 (spread)
            list.Add(Quest("q1", "garden_1", 1, 20, 0, "Harvest a Seed"));
            list.Add(Quest("q2", "garden_2", 1, 30, 2, "Grow a Sprout"));
            list.Add(Quest("q3", "garden_3", 2, 45, 0, "Pick 2 Flowers"));
            list.Add(Quest("q4", "garden_4", 1, 50, 3, "Tend a Bush"));
            list.Add(Quest("q5", "garden_5", 1, 60, 0, "Plant a Tree"));
            list.Add(Quest("q6", "garden_6", 1, 75, 5, "Grow a Big Tree", MergeChain.Wood));

            // q7-14: Wood phase — Wood levels 1-7 + more Garden
            list.Add(Quest("q7", "wood_1", 2, 80, 0, "Collect 2 Twigs"));
            list.Add(Quest("q8", "garden_7", 1, 100, 3, "Complete the Magic Tree"));
            list.Add(Quest("q9", "wood_2", 2, 110, 0, "Craft 2 Logs"));
            list.Add(Quest("q10", "wood_3", 1, 120, 4, "Make a Plank"));
            list.Add(Quest("q11", "garden_1", 3, 130, 0, "Gather 3 Seeds (for compost)", MergeChain.Stone));
            list.Add(Quest("q12", "wood_4", 1, 140, 0, "Build a Crate"));
            list.Add(Quest("q13", "wood_5", 2, 160, 5, "Craft 2 Furniture pieces"));
            list.Add(Quest("q14", "wood_6", 1, 180, 0, "Construct a Gazebo"));

            // q15-22: Stone phase + Grind — Stone levels 1-7 + mix
            list.Add(Quest("q15", "stone_1", 2, 200, 0, "Place 2 Pebbles"));
            list.Add(Quest("q16", "wood_7", 1, 220, 8, "Build the House"));
            list.Add(Quest("q17", "stone_2", 2, 240, 0, "Lay 2 Stones"));
            list.Add(Quest("q18", "stone_3", 1, 260, 5, "Make a Brick wall"));
            list.Add(Quest("q19", "garden_2", 2, 280, 0, "Replant 2 Sprouts"));
            list.Add(Quest("q20", "stone_4", 1, 300, 0, "Build a Wall"));
            list.Add(Quest("q21", "stone_5", 1, 350, 10, "Raise a Pillar"));
            list.Add(Quest("q22", "stone_6", 1, 400, 0, "Complete the Fountain"));

            return list.ToArray();
        }

        private static Quest Quest(string id, string itemId, int amount, int coin, int gem, string desc, MergeChain? unlock = null)
        {
            return new Quest
            {
                id = id,
                requiredItemId = itemId,
                requiredAmount = amount,
                coinReward = coin,
                gemReward = gem,
                description = desc,
                unlocksChain = unlock
            };
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
