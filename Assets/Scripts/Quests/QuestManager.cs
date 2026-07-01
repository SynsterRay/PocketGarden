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

        private const int TotalQuests = 60;

        private static readonly string[] GardenNames = { "Seed", "Sprout", "Flower", "Bush", "Tree", "Big Tree", "Magic Tree" };
        private static readonly string[] WoodNames   = { "Dead Tree", "Log", "Plank", "Crate", "Furniture", "Gazebo", "House" };
        private static readonly string[] StoneNames  = { "Pebble", "Stone", "Brick", "Wall", "Pillar", "Fountain", "Castle" };

        private static readonly Quest[] AllQuests = BuildQuests();

        /// <summary>
        /// Builds a 60-quest ladder using all 21 items with creative descriptions:
        ///   • q1-12:   Hook phase — Garden only, narrative-driven, easy hook
        ///   • q10:     Wood unlock (completing q10)
        ///   • q13-35:  Wood phase — mix Garden/Wood, creative building quests
        ///   • q35:     Stone unlock (completing q35)
        ///   • q36-60:  Stone phase + Grind — all chains, epic descriptions, gem rewards
        /// Creative quest types: harvest, craft, decorate, gather for compost, build structures,
        /// prepare festivals, brew potions, arrange gardens, construct arches, etc.
        /// </summary>
        private static Quest[] BuildQuests()
        {
            var list = new List<Quest>(TotalQuests);

            // q1-12: Hook — Garden foundation with narrative
            list.Add(Quest("q1",  "garden_1", 1,  20, 0, "🌱 Plant your first seed"));
            list.Add(Quest("q2",  "garden_2", 1,  30, 2, "🌿 Nurture a sprout into growth"));
            list.Add(Quest("q3",  "garden_3", 2,  45, 0, "🌸 Pick 2 flowers for a bouquet"));
            list.Add(Quest("q4",  "garden_4", 1,  50, 3, "🪴 Tend a bush in your garden"));
            list.Add(Quest("q5",  "garden_5", 1,  60, 0, "🌳 Plant a mature tree"));
            list.Add(Quest("q6",  "garden_6", 1,  75, 5, "🌲 Grow a Big Tree — lumber incoming"));
            list.Add(Quest("q7",  "garden_2", 2,  80, 0, "🌿 Cultivate 2 sprouts for medicine"));
            list.Add(Quest("q8",  "garden_3", 3,  90, 3, "🌸 Gather 3 flowers for the town festival"));
            list.Add(Quest("q9",  "garden_1", 4, 100, 0, "🌱 Harvest 4 seeds for next season"));
            list.Add(Quest("q10", "garden_4", 2, 110, 2, "🪴 Shape 2 bushes into hedges", MergeChain.Wood));
            list.Add(Quest("q11", "garden_3", 2, 115, 0, "💐 Prepare a flower garland for the festival"));
            list.Add(Quest("q12", "garden_1", 3, 118, 2, "🌱 Arrange a herb garden with 3 seeds"));

            // q13-35: Wood phase — crafting, building, and creative projects
            list.Add(Quest("q13", "wood_1",   2, 125, 0, "🪵 Gather 2 dead trees for kindling"));
            list.Add(Quest("q14", "garden_7", 1, 135, 4, "🌳 Harvest the Magic Tree's blessing"));
            list.Add(Quest("q15", "wood_2",   2, 145, 0, "🪓 Fell 2 logs for the sawmill"));
            list.Add(Quest("q16", "wood_3",   1, 155, 3, "📏 Plane a log into perfect planks"));
            list.Add(Quest("q17", "garden_1", 3, 165, 0, "🌱 Collect 3 seeds for compost pile"));
            list.Add(Quest("q18", "wood_4",   2, 175, 0, "📦 Craft 2 crates for storage"));
            list.Add(Quest("q19", "garden_5", 2, 185, 5, "🌳 Plant 2 trees to shade your home"));
            list.Add(Quest("q20", "wood_5",   2, 195, 0, "🪑 Handcraft 2 pieces of furniture"));
            list.Add(Quest("q21", "wood_1",   3, 205, 2, "🪵 Collect 3 dead trees for the craftsman"));
            list.Add(Quest("q22", "wood_3",   2, 212, 3, "🐦 Craft a birdhouse from 2 planks"));
            list.Add(Quest("q23", "wood_6",   1, 220, 0, "🏕️ Construct a gazebo for gatherings"));
            list.Add(Quest("q24", "garden_2", 3, 230, 0, "🌿 Grow 3 sprouts for the apothecary"));
            list.Add(Quest("q25", "wood_2",   3, 240, 4, "🪓 Bring 3 logs to the builder"));
            list.Add(Quest("q26", "garden_6", 1, 250, 0, "🌲 Nurture another Big Tree"));
            list.Add(Quest("q27", "wood_3",   2, 260, 0, "📏 Cut 2 planks for the roof"));
            list.Add(Quest("q28", "garden_3", 4, 270, 3, "🌸 Gather 4 flowers for decoration"));
            list.Add(Quest("q29", "wood_4",   2, 280, 0, "📦 Build 2 storage crates"));
            list.Add(Quest("q30", "wood_5",   1, 290, 5, "🪑 Craft a beautiful chair"));
            list.Add(Quest("q31", "wood_1",   4, 298, 0, "🔥 Gather 4 dead trees for a bonfire"));
            list.Add(Quest("q32", "garden_4", 2, 305, 0, "🪴 Shape 2 bushes into topiary"));
            list.Add(Quest("q33", "wood_2",   3, 312, 3, "🪵 Collect wood for the workshop bench"));
            list.Add(Quest("q34", "wood_6",   1, 320, 0, "🏕️ Complete a second gazebo"));
            list.Add(Quest("q35", "garden_3", 3, 330, 4, "🌺 Build garden arches for the wedding", MergeChain.Stone));

            // q36-60: Stone phase + Grind — monuments, epic structures, and grand goals
            list.Add(Quest("q36", "stone_1",  2, 340, 0, "💎 Place 2 pebbles as foundation"));
            list.Add(Quest("q37", "stone_1",  3, 350, 0, "🪨 Stack stones for a meditation garden"));
            list.Add(Quest("q38", "wood_7",   1, 360, 8, "🏠 Finish building the Great House"));
            list.Add(Quest("q39", "stone_2",  2, 370, 0, "🪨 Lay 2 stones for the path"));
            list.Add(Quest("q40", "stone_3",  2, 380, 5, "🧱 Craft 2 bricks for the walls"));
            list.Add(Quest("q41", "garden_2", 2, 390, 0, "🌿 Grow 2 sprouts for the harvest"));
            list.Add(Quest("q42", "stone_4",  1, 400, 0, "🏰 Build a stone wall around town"));
            list.Add(Quest("q43", "wood_1",   4, 410, 3, "🪵 Gather 4 dead trees for the town"));
            list.Add(Quest("q44", "stone_2",  4, 420, 0, "🪨 Lay 4 stones for the plaza"));
            list.Add(Quest("q45", "garden_3", 2, 430, 4, "🌸 Collect 2 flowers for the shrine"));
            list.Add(Quest("q46", "stone_3",  3, 438, 0, "🧱 Prepare mortar — stack 3 bricks for the mason"));
            list.Add(Quest("q47", "stone_5",  1, 445, 0, "🏛️ Raise a pillar for the temple"));
            list.Add(Quest("q48", "wood_3",   2, 455, 5, "📏 Prepare 2 planks for the bridge"));
            list.Add(Quest("q49", "wood_3",   3, 462, 4, "🪜 Build scaffolding — deliver 3 planks for the tower"));
            list.Add(Quest("q50", "stone_3",  4, 470, 0, "🧱 Stack 4 bricks for the tower"));
            list.Add(Quest("q51", "garden_1", 5, 480, 0, "🌱 Harvest 5 seeds for the archive"));
            list.Add(Quest("q52", "stone_4",  3, 490, 6, "🏰 Build 3 walls for the fortress"));
            list.Add(Quest("q53", "wood_2",   4, 500, 0, "🪓 Mill 4 logs into boards"));
            list.Add(Quest("q54", "stone_5",  3, 510, 8, "🏛️ Raise 3 pillars for the palace"));
            list.Add(Quest("q55", "garden_5", 2, 520, 0, "🌳 Plant 2 trees for the royal garden"));
            list.Add(Quest("q56", "wood_7",   1, 530, 5, "🏠 Construct the Estate"));
            list.Add(Quest("q57", "stone_6",  1, 545, 10, "⛲ Complete the Fountain — masterpiece"));
            list.Add(Quest("q58", "garden_7", 1, 560, 8, "🌳 Harness the Magic Tree's ultimate power"));
            list.Add(Quest("q59", "stone_7",  1, 580, 12, "🏰 Raise the Castle — crowning glory"));
            list.Add(Quest("q60", "garden_7", 1, 600, 20, "✨ The Garden of Eden blooms eternal"));

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
            // Fill active list up to 2 quests without resetting already-tracked ones.
            while (_activeQuests.Count < 2)
            {
                int idx = _questIndex + _activeQuests.Count;
                if (idx >= AllQuests.Length) break;
                var template = AllQuests[idx];
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

                        // Remove completed quest, advance index for each leading completed quest.
                        _activeQuests.Remove(quest);
                        _questIndex++;
                        // If the new first quest is also complete, advance past it too.
                        while (_activeQuests.Count > 0 && _activeQuests[0].IsComplete)
                        {
                            _activeQuests.RemoveAt(0);
                            _questIndex++;
                        }
                        PlayerPrefs.SetInt("MG_QuestIndex", _questIndex);
                        PlayerPrefs.Save();
                        // Fill remaining slots (preserves the other quest's progress).
                        LoadNextQuests();
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
