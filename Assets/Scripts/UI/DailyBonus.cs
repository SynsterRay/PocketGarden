using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    public class DailyBonus : MonoBehaviour
    {
        private const string LastClaimKey = "PG_DailyLastClaim";
        private const string StreakKey = "PG_DailyStreak";

        private void Start()
        {
            Check();
        }

        private void Check()
        {
            string last = PlayerPrefs.GetString(LastClaimKey, "");
            string today = System.DateTime.Now.ToString("yyyy-MM-dd");
            if (last == today) return;

            int streak = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var diff = (System.DateTime.Now.Date - System.DateTime.Parse(last).Date).Days;
                streak = diff == 1 ? PlayerPrefs.GetInt(StreakKey, 0) + 1 : 1;
            }

            int energyReward = Mathf.Min(10 + streak * 5, 50);
            int coinReward = streak * 10;
            ShowPopup(streak, energyReward, coinReward);
        }

        private void ShowPopup(int streak, int energy, int coins)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var panel = new GameObject("DailyBonusPopup");
            panel.transform.SetParent(canvas.transform, false);
            panel.transform.SetAsLastSibling();

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.8f);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var txt = new GameObject("Text").AddComponent<Text>();
            txt.transform.SetParent(panel.transform, false);
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 30;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = $"🎁 Daily Bonus!\n\nDay {streak}\n⚡ +{energy} Energy\n🪙 +{coins} Coins\n\nTap to claim!";
            var tr = txt.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.1f, 0.3f);
            tr.anchorMax = new Vector2(0.9f, 0.7f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            var btn = panel.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                EnergySystem.Add(energy);
                CoinSystem.Add(coins);
                PlayerPrefs.SetString(LastClaimKey, System.DateTime.Now.ToString("yyyy-MM-dd"));
                PlayerPrefs.SetInt(StreakKey, streak);
                PlayerPrefs.Save();
                Destroy(panel);
            });
        }
    }
}
