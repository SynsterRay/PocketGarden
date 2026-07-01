using UnityEngine;

namespace PocketGarden.Core
{
    public static class GemSystem
    {
        private const string Key = "PG_Gems";
        private static int _gems = -1;

        public static event System.Action<int> OnGemsChanged;

        public static int Gems
        {
            get
            {
                if (_gems < 0) _gems = PlayerPrefs.GetInt(Key, 0);
                return _gems;
            }
            private set
            {
                _gems = Mathf.Max(0, value);
                PlayerPrefs.SetInt(Key, _gems);
                PlayerPrefs.Save();
                OnGemsChanged?.Invoke(_gems);
            }
        }

        public static void Add(int amount) { if (amount > 0) Gems += amount; }
        public static bool Spend(int amount) { if (Gems < amount) return false; Gems -= amount; return true; }
    }
}
