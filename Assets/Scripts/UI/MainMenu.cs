using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>Settings dialog (dimmed background + centered card). Consistent spacing, no overlap.</summary>
    public class MainMenu : MonoBehaviour
    {
        private GameObject _panel;
        private bool _visible;
        private Text _musicText;
        private Text _sfxText;

        private static bool _musicOn = true;
        private static bool _sfxOn = true;

        public void Toggle() { if (_visible) Hide(); else Show(); }

        public void Show()
        {
            if (_panel != null) { _panel.SetActive(true); _panel.transform.SetAsLastSibling(); _visible = true; return; }

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Dim background
            _panel = new GameObject("MenuPanel");
            _panel.transform.SetParent(canvas.transform, false);
            _panel.transform.SetAsLastSibling();
            var dim = _panel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            UIFactory.Stretch(_panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

            // Card
            var card = UIFactory.Panel(_panel.transform, new Vector2(0.08f, 0.16f),
                new Vector2(0.92f, 0.88f), UIFactory.Cream, "Card");

            UIFactory.Text(card.transform, "⚙  Settings", new Vector2(0.05f, 0.88f),
                new Vector2(0.95f, 0.98f), 36, UIFactory.LeafDark);

            float y = 0.78f;
            const float h = 0.085f, step = 0.105f;

            _musicText = AddRow(card.transform, "🎵  Music: ON", ref y, h, step, () =>
            {
                _musicOn = !_musicOn;
                _musicText.text = _musicOn ? "🎵  Music: ON" : "🎵  Music: OFF";
                AudioListener.volume = (_musicOn || _sfxOn) ? 1f : 0f; // TODO: split music/SFX buses
            });

            _sfxText = AddRow(card.transform, "🔊  SFX: ON", ref y, h, step, () =>
            {
                _sfxOn = !_sfxOn;
                _sfxText.text = _sfxOn ? "🔊  SFX: ON" : "🔊  SFX: OFF";
            });

            AddRow(card.transform, "🌐  Language: English", ref y, h, step, () =>
            {
                Debug.Log("[Menu] Language toggle"); // TODO: localization
            });

            AddRow(card.transform, "⭐  Rate Us", ref y, h, step, () =>
            {
                Application.OpenURL("https://play.google.com/store/apps/details?id=com.WonderMindsGames.PocketGarden");
            });

            AddRow(card.transform, "❓  How to Play", ref y, h, step, () =>
            {
                Hide();
                TutorialOverlay.ShowTutorial();
            });

            AddRow(card.transform, "ℹ️  Credits: Wonder Minds Games", ref y, h, step, () => { });

            AddRow(card.transform, "🗑️  Reset Progress", ref y, h, step, () =>
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Menu] Progress reset!");
            });

            // Close
            var close = UIFactory.Button(card.transform, "Close", new Vector2(0.30f, 0.03f),
                new Vector2(0.70f, 0.12f), UIFactory.Danger, 24);
            close.onClick.AddListener(Hide);

            _visible = true;
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            _visible = false;
        }

        private Text AddRow(Transform parent, string label, ref float y, float h, float step, System.Action onClick)
        {
            var btn = UIFactory.Button(parent, label, new Vector2(0.08f, y - h),
                new Vector2(0.92f, y), new Color(0.93f, 0.95f, 0.88f), 24, UIFactory.Ink);
            btn.onClick.AddListener(() => onClick());
            y -= step;
            return btn.GetComponentInChildren<Text>();
        }
    }
}
