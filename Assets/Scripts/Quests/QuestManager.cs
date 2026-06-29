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

        private const int TotalQuests = 50;

        private static readonly string[] GardenNames = { "Seed", "Sprout", "Flower", "Bush", "Tree", "Big Tree", "Magic Tree" };
        private static readonly string[] WoodNames   = { "Dead Tree", "Log", "Plank", "Crate", "Furniture", "Gazebo", "House" };
        private static readonly string[] StoneNames  = { "Pebble", "Stone", "Brick", "Wall", "Pillar", "Fountain", "Castle" };

        private static readonly Quest[] AllQuests = BuildQuests();

        /// <summary>
        /// Builds a 50-quest ladder using all 21 items with creative descriptions:
        ///   • q1-10:   Hook phase — Garden only, narrative-driven, easy hook
        ///   • q11:     Wood unlock
        ///   • q12-30:  Wood phase — mix Garden/Wood, creative building quests
        ///   • q31:     Stone unlock
        ///   • q32-50:  Stone phase + Grind — all chains, epic descriptions, gem rewards
        /// Creative quest types: harvest, craft, decorate, gather for compost, build structures, etc.
        /// </summary>
        private static Quest[] BuildQuests()
        {
            var list = new List<Quest>(TotalQuests);

            // q1-10: Hook — Garden foundation with narrative
            list.Add(Quest("q1", "garden_1", 1, 20, 0, "🌱 Plant your first seed"));
            list.Add(Quest("q2", "garden_2", 1, 30, 2, "🌿 Nurture a sprout into growth"));
            list.Add(Quest("q3", "garden_3", 2, 45, 0, "🌸 Pick 2 flowers for a bouquet"));
            list.Add(Quest("q4", "garden_4", 1, 50, 3, "🪴 Tend a bush in your garden"));
            list.Add(Quest("q5", "garden_5", 1, 60, 0, "🌳 Plant a mature tree"));
            list.Add(Quest("q6", "garden_6", 1, 75, 5, "🌲 Grow a Big Tree — lumber incoming"));
            list.Add(Quest("q7", "garden_2", 2, 80, 0, "🌿 Cultivate 2 sprouts for medicine"));
            list.Add(Quest("q8", "garden_3", 3, 90, 3, "🌸 Gather 3 flowers for the town"));
            list.Add(Quest("q9", "garden_1", 4, 100, 0, "🌱 Harvest 4 seeds for next season"));
            list.Add(Quest("q10", "garden_4", 2, 110, 2, "🪴 Shape 2 bushes into hedges", MergeChain.Wood));

            // q11-30: Wood phase — crafting and building
            list.Add(Quest("q11", "wood_1", 2, 120, 0, "🪵 Gather 2 twigs for kindling"));
            list.Add(Quest("q12", "garden_7", 1, 130, 4, "🌳 Harvest the Magic Tree's blessing"));
            list.Add(Quest("q13", "wood_2", 2, 140, 0, "🪓 Fell 2 logs for the sawmill"));
            list.Add(Quest("q14", "wood_3", 1, 150, 3, "📏 Plane a log into perfect planks"));
            list.Add(Quest("q15", "garden_1", 3, 160, 0, "🌱 Collect 3 seeds for compost pile"));
            list.Add(Quest("q16", "wood_4", 2, 170, 0, "📦 Craft 2 crates for storage"));
            list.Add(Quest("q17", "garden_5", 2, 180, 5, "🌳 Plant 2 trees to shade your home"));
            list.Add(Quest("q18", "wood_5", 2, 190, 0, "🪑 Handcraft 2 pieces of furniture"));
            list.Add(Quest("q19", "wood_1", 3, 200, 2, "🪵 Collect 3 twigs for the craftsman"));
            list.Add(Quest("q20", "wood_6", 1, 210, 0, "🏕️ Construct a gazebo for gatherings"));
            list.Add(Quest("q21", "garden_2", 3, 220, 0, "🌿 Grow 3 sprouts for the apothecary"));
            list.Add(Quest("q22", "wood_2", 3, 230, 4, "🪓 Bring 3 logs to the builder"));
            list.Add(Quest("q23", "garden_6", 1, 240, 0, "🌲 Nurture another Big Tree"));
            list.Add(Quest("q24", "wood_3", 2, 250, 0, "📏 Cut 2 planks for the roof"));
            list.Add(Quest("q25", "garden_3", 4, 260, 3, "🌸 Gather 4 flowers for decoration"));
            list.Add(Quest("q26", "wood_4", 2, 270, 0, "📦 Build 2 storage crates"));
            list.Add(Quest("q27", "wood_5", 1, 280, 5, "🪑 Craft a beautiful chair"));
            list.Add(Quest("q28", "garden_4", 2, 290, 0, "🪴 Shape 2 bushes into topiary"));
            list.Add(Quest("q29", "wood_6", 1, 300, 0, "🏕️ Complete a second gazebo", MergeChain.Stone));
            list.Add(Quest("q30", "stone_1", 2, 310, 0, "💎 Place 2 pebbles as foundation"));

            // q31-50: Stone phase + Grind — monuments and epic structures
            list.Add(Quest("q31", "stone_1", 2, 320, 0, "💎 Place 2 pebbles as foundation"));
            list.Add(Quest("q32", "wood_7", 1, 330, 8, "🏠 Finish building the Great House"));
            list.Add(Quest("q33", "stone_2", 2, 340, 0, "🪨 Lay 2 stones for the path"));
            list.Add(Quest("q34", "stone_3", 2, 350, 5, "🧱 Craft 2 bricks for the walls"));
            list.Add(Quest("q35", "garden_2", 2, 360, 0, "🌿 Grow 2 sprouts for the harvest"));
            list.Add(Quest("q36", "stone_4", 1, 370, 0, "🏰 Build a stone wall around town"));
            list.Add(Quest("q37", "wood_1", 4, 380, 3, "🪵 Gather 4 twigs for the town"));
            list.Add(Quest("q38", "stone_2", 3, 390, 0, "🪨 Lay 3 stones for the plaza"));
            list.Add(Quest("q39", "garden_3", 2, 400, 4, "🌸 Collect 2 flowers for the shrine"));
            list.Add(Quest("q40", "stone_5", 1, 410, 0, "🏛️ Raise a pillar for the temple"));
            list.Add(Quest("q41", "wood_3", 2, 420, 5, "📏 Prepare 2 planks for the bridge"));
            list.Add(Quest("q42", "stone_3", 3, 430, 0, "🧱 Stack 3 bricks for the tower"));
            list.Add(Quest("q43", "garden_1", 5, 440, 0, "🌱 Harvest 5 seeds for the archive"));
            list.Add(Quest("q44", "stone_4", 2, 450, 6, "🏰 Build 2 walls for the fortress"));
            list.Add(Quest("q45", "wood_2", 4, 460, 0, "🪓 Mill 4 logs into boards"));
            list.Add(Quest("q46", "stone_5", 2, 470, 8, "🏛️ Raise 2 pillars for the palace"));
            list.Add(Quest("q47", "garden_5", 2, 480, 0, "🌳 Plant 2 trees for the royal garden"));
            list.Add(Quest("q48", "wood_7", 1, 490, 5, "🏠 Construct the Estate"));
            list.Add(Quest("q49", "stone_6", 1, 500, 15, "⛲ Complete the Fountain — masterpiece"));
            list.Add(Quest("q50", "garden_7", 1, 510, 10, "🌳 Harness the Magic Tree's ultimate power"));

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
