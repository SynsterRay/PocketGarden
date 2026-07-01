using UnityEngine;
using UnityEngine.UI;

namespace PocketGarden.UI
{
    public class TutorialOverlay : MonoBehaviour
    {
        private GameObject _panel;
        private int _page;

        private const string TutorialSeenKey = "PG_TutorialSeen";

        private static readonly string[] Pages =
        {
            "🔀 Drag identical items onto each other to merge them into a higher level!",
            "⚡ Each merge costs 1 energy. Energy regenerates over time.",
            "📋 Complete quests by creating the required items. Deliver them for coins!",
            "💡 Tap the yellow dot on a generator to produce new items when it's ready."
        };

        private void Start()
        {
            // Show tutorial only on first run (or when manually triggered via Settings).
            if (PlayerPrefs.GetInt(TutorialSeenKey, 0) == 0)
                Show();
            else
                Destroy(this);
        }

        /// <summary>Public so Settings can re-trigger the tutorial ("How to Play" button).</summary>
        public static void ShowTutorial()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
            var existing = Object.FindAnyObjectByType<TutorialOverlay>();
            if (existing != null) return; // already showing
            canvas.gameObject.AddComponent<TutorialOverlay>().Show();
        }

        private void Show()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _page = 0;
            _panel = new GameObject("TutorialPanel");
            _panel.transform.SetParent(canvas.transform, false);
            _panel.transform.SetAsLastSibling();

            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            var rect = _panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Text
            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(_panel.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 32;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = Pages[0];
            var tr = txtGo.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.1f, 0.4f);
            tr.anchorMax = new Vector2(0.9f, 0.7f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            // Page indicator
            var dotGo = new GameObject("Dots");
            dotGo.transform.SetParent(_panel.transform, false);
            var dotTxt = dotGo.AddComponent<Text>();
            dotTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dotTxt.fontSize = 24;
            dotTxt.alignment = TextAnchor.MiddleCenter;
            dotTxt.color = new Color(1f, 1f, 1f, 0.6f);
            dotTxt.text = $"1 / {Pages.Length}";
            var dr = dotGo.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0.3f, 0.25f);
            dr.anchorMax = new Vector2(0.7f, 0.32f);
            dr.offsetMin = Vector2.zero;
            dr.offsetMax = Vector2.zero;

            // Tap to continue
            var hintGo = new GameObject("Hint");
            hintGo.transform.SetParent(_panel.transform, false);
            var hintTxt = hintGo.AddComponent<Text>();
            hintTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintTxt.fontSize = 22;
            hintTxt.alignment = TextAnchor.MiddleCenter;
            hintTxt.color = new Color(1f, 1f, 1f, 0.5f);
            hintTxt.text = "Tap anywhere to continue";
            var hr = hintGo.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0.2f, 0.12f);
            hr.anchorMax = new Vector2(0.8f, 0.18f);
            hr.offsetMin = Vector2.zero;
            hr.offsetMax = Vector2.zero;

            // Button (full screen tap)
            var btn = _panel.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(NextPage);
        }

        private void NextPage()
        {
            _page++;
            if (_page >= Pages.Length)
            {
                // Mark as seen so it doesn't show again.
                PlayerPrefs.SetInt(TutorialSeenKey, 1);
                PlayerPrefs.Save();
                Destroy(_panel);
                Destroy(this);
                return;
            }

            var txt = _panel.GetComponentInChildren<Text>();
            if (txt != null) txt.text = Pages[_page];

            // Update dots
            var dots = _panel.transform.Find("Dots");
            if (dots != null)
            {
                var dotTxt = dots.GetComponent<Text>();
                if (dotTxt != null) dotTxt.text = $"{_page + 1} / {Pages.Length}";
            }
        }
    }
}
