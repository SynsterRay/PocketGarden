using UnityEngine;

namespace PocketGarden.Core
{
    public static class EnergySystem
    {
        private const string EnergyKey = "MG_Energy";
        private const string LastTimeKey = "MG_EnergyTime";
        private const int MaxEnergy = 30;
        private const int RegenSeconds = 180; // 3 minutes per 1 energy

        public static event System.Action<int> OnEnergyChanged;

        private static int _energy = -1;

        public static int Energy
        {
            get
            {
                if (_energy < 0) Load();
                return _energy;
            }
            private set
            {
                _energy = Mathf.Clamp(value, 0, MaxEnergy);
                PlayerPrefs.SetInt(EnergyKey, _energy);
                PlayerPrefs.SetString(LastTimeKey, System.DateTime.UtcNow.ToBinary().ToString());
                PlayerPrefs.Save();
                OnEnergyChanged?.Invoke(_energy);
            }
        }

        public static int Max => MaxEnergy;
        public static bool HasEnergy => Energy > 0;

        public static bool Spend(int amount = 1)
        {
            if (Energy < amount) return false;
            Energy -= amount;
            return true;
        }

        public static void Add(int amount)
        {
            Energy += amount;
        }

        public static void Load()
        {
            _energy = PlayerPrefs.GetInt(EnergyKey, MaxEnergy);
            // Calculate offline regen
            string timeStr = PlayerPrefs.GetString(LastTimeKey, "");
            if (!string.IsNullOrEmpty(timeStr) && long.TryParse(timeStr, out long binary))
            {
                var lastTime = System.DateTime.FromBinary(binary);
                int elapsed = (int)(System.DateTime.UtcNow - lastTime).TotalSeconds;
                int regen = elapsed / RegenSeconds;
                if (regen > 0)
                    _energy = Mathf.Min(MaxEnergy, _energy + regen);
            }
            PlayerPrefs.SetInt(EnergyKey, _energy);
            PlayerPrefs.Save();
        }

        /// <summary>Call from MonoBehaviour Update to tick regen.</summary>
        private static float _regenTimer;
        public static void Tick()
        {
            if (_energy >= MaxEnergy) return;
            _regenTimer += Time.deltaTime;
            if (_regenTimer >= RegenSeconds)
            {
                _regenTimer -= RegenSeconds;
                Energy++;
            }
        }
    }
}
