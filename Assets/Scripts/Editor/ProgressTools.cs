using UnityEngine;
using UnityEditor;

namespace PocketGarden.Editor
{
    /// <summary>Editor tools for testing progression.</summary>
    public static class ProgressTools
    {
        [MenuItem("PocketGarden/Reset Progress")]
        public static void ResetProgress()
        {
            bool ok = EditorUtility.DisplayDialog(
                "Reset Pocket Garden progress?",
                "This clears the board, quests, energy, coins, gems, chain unlocks and all saved " +
                "flags (PlayerPrefs). Starting gems will be granted again on next Play. Continue?",
                "Reset", "Cancel");

            if (!ok) return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[PocketGarden] Progress reset — all PlayerPrefs cleared.");
        }
    }
}
