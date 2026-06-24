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
    }
}
