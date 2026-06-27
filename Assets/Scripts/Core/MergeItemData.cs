using UnityEngine;

namespace PocketGarden.Core
{
    public enum MergeChain { Garden, Wood, Stone }

    [System.Serializable]
    public class MergeItemData
    {
        public string id;
        public MergeChain chain;
        public int level;
        public string displayName;
        public Sprite icon;
        public Color fallbackColor = Color.white; // prototype placeholder

        // Producer (Option A): mature items (e.g. Tree) generate another chain's item.
        public string producesItemId;   // null/empty = not a producer
        public float produceCooldown;   // seconds between productions

        public bool IsProducer => !string.IsNullOrEmpty(producesItemId) && produceCooldown > 0f;
    }
}
