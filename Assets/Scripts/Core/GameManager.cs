using UnityEngine;

namespace PocketGarden.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private const string StartGemsKey = "PG_StartGemsGranted";
        private const int StartingGems = 50000; // TEMPORARY: generous test budget (revert to 100 before release)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Screen.orientation = ScreenOrientation.Portrait;

            // One-time welcome gems so new players can immediately use the gem sinks.
            if (PlayerPrefs.GetInt(StartGemsKey, 0) == 0)
            {
                GemSystem.Add(StartingGems);
                PlayerPrefs.SetInt(StartGemsKey, 1);
                PlayerPrefs.Save();
            }
        }
    }
}
