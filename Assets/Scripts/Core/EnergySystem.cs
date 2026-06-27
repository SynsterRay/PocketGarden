using UnityEngine;

namespace PocketGarden.Core
{
    public static class EnergySystem
    {
        private const string EnergyKey = "MG_Energy";
        private const string LastTimeKey = "MG_EnergyTime";
        private const int MaxEnergy = 30;
        // Regen interval is sourced from Progression.EnergyRegenSeconds (phase-based).

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

        /// <summary>Refills energy to the maximum (used by gem refill).</summary>
        public static void Refill() => Energy = MaxEnergy;

        public static bool IsFull => Energy >= MaxEnergy;

        public static void Add(int amount)
        {
            Energy += amount;
        }

        public static void Load()
        {
            _energy = PlayerPrefs.GetInt(EnergyKey, MaxEnergy);
            int regenSeconds = Progression.EnergyRegenSeconds;
            // Calculate offline regen
            string timeStr = PlayerPrefs.GetString(LastTimeKey, "");
            if (!string.IsNullOrEmpty(timeStr) && long.TryParse(timeStr, out long binary))
            {
                var lastTime = System.DateTime.FromBinary(binary);
                int elapsed = (int)(System.DateTime.UtcNow - lastTime).TotalSeconds;
                int regen = elapsed / Mathf.Max(1, regenSeconds);
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
            if (_regenTimer >= Progression.EnergyRegenSeconds)
            {
                _regenTimer = 0f;
                Energy++;
            }
        }
    }
}
