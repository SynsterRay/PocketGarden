using UnityEngine;

namespace PocketGarden.Core
{
    public static class CoinSystem
    {
        private const string Key = "MG_Coins";
        private static int _coins = -1;

        public static event System.Action<int> OnCoinsChanged;

        public static int Coins
        {
            get
            {
                if (_coins < 0) _coins = PlayerPrefs.GetInt(Key, 0);
                return _coins;
            }
            private set
            {
                _coins = Mathf.Max(0, value);
                PlayerPrefs.SetInt(Key, _coins);
                PlayerPrefs.Save();
                OnCoinsChanged?.Invoke(_coins);
            }
        }

        public static void Add(int amount) { if (amount > 0) Coins += amount; }
        public static bool Spend(int amount) { if (Coins < amount) return false; Coins -= amount; return true; }
    }
}
