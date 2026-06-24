using UnityEngine;
using System.Collections.Generic;

namespace PocketGarden.Core
{
    [CreateAssetMenu(fileName = "MergeDatabase", menuName = "PocketGarden/Merge Database")]
    public class MergeDatabase : ScriptableObject
    {
        public List<MergeItemData> items = new();

        private Dictionary<string, MergeItemData> _lookup;
        private Dictionary<(MergeChain, int), MergeItemData> _byLevel;

        public void Init()
        {
            _lookup = new Dictionary<string, MergeItemData>();
            _byLevel = new Dictionary<(MergeChain, int), MergeItemData>();
            foreach (var item in items)
            {
                _lookup[item.id] = item;
                _byLevel[(item.chain, item.level)] = item;
            }
        }

        public MergeItemData Get(string id)
        {
            if (_lookup == null) Init();
            return _lookup.TryGetValue(id, out var d) ? d : null;
        }

        public MergeItemData GetMergeResult(MergeItemData item)
        {
            if (_byLevel == null) Init();
            return _byLevel.TryGetValue((item.chain, item.level + 1), out var next) ? next : null;
        }

        public bool CanMerge(MergeItemData a, MergeItemData b)
        {
            return a.chain == b.chain && a.level == b.level && GetMergeResult(a) != null;
        }

        /// <summary>Creates default prototype data (no sprites, uses colors).</summary>
        public static List<MergeItemData> CreateDefaults()
        {
            var list = new List<MergeItemData>();

            // Garden chain - greens
            string[] gardenNames = { "Seed", "Sprout", "Bush", "Flower", "Tree", "Big Tree", "Magic Tree" };
            for (int i = 0; i < gardenNames.Length; i++)
                list.Add(new MergeItemData
                {
                    id = $"garden_{i + 1}", chain = MergeChain.Garden, level = i + 1,
                    displayName = gardenNames[i],
                    fallbackColor = Color.Lerp(new Color(0.6f, 0.9f, 0.3f), new Color(0.1f, 0.5f, 0.1f), i / 6f)
                });

            // Wood chain - browns
            string[] woodNames = { "Twig", "Log", "Plank", "Crate", "Furniture", "Gazebo", "House" };
            for (int i = 0; i < woodNames.Length; i++)
                list.Add(new MergeItemData
                {
                    id = $"wood_{i + 1}", chain = MergeChain.Wood, level = i + 1,
                    displayName = woodNames[i],
                    fallbackColor = Color.Lerp(new Color(0.8f, 0.6f, 0.3f), new Color(0.4f, 0.2f, 0.05f), i / 6f)
                });

            // Stone chain - grays
            string[] stoneNames = { "Pebble", "Stone", "Brick", "Wall", "Pillar", "Fountain", "Castle" };
            for (int i = 0; i < stoneNames.Length; i++)
                list.Add(new MergeItemData
                {
                    id = $"stone_{i + 1}", chain = MergeChain.Stone, level = i + 1,
                    displayName = stoneNames[i],
                    fallbackColor = Color.Lerp(new Color(0.7f, 0.7f, 0.7f), new Color(0.3f, 0.3f, 0.4f), i / 6f)
                });

            return list;
        }
    }
}
