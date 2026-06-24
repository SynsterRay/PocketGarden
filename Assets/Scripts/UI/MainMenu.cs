using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    public class MainMenu : MonoBehaviour
    {
        private GameObject _panel;
        private bool _visible;
        private Text _musicText;
        private Text _sfxText;

        private static bool _musicOn = true;
        private static bool _sfxOn = true;

        public void Toggle()
        {
            if (_visible) Hide();
            else Show();
        }

        public void Show()
        {
            if (_panel != null) { _panel.SetActive(true); _visible = true; return; }

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _panel = new GameObject("MenuPanel");
            _panel.transform.SetParent(canvas.transform, false);
            _panel.transform.SetAsLastSibling();

            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.98f, 0.92f, 0.98f);
            var rect = _panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Title
            CreateText("⚙️ Settings", _panel.transform,
                new Vector2(0.2f, 0.85f), new Vector2(0.8f, 0.93f), 36, new Color(0.2f, 0.4f, 0.2f));

            float y = 0.75f;

            // Music toggle
            _musicText = CreateButton("🎵 Music: ON", _panel.transform, y, () =>
            {
                _musicOn = !_musicOn;
                _musicText.text = _musicOn ? "🎵 Music: ON" : "🎵 Music: OFF";
                // TODO: wire to AudioListener / MusicManager
            });
            y -= 0.10f;

            // SFX toggle
            _sfxText = CreateButton("🔊 SFX: ON", _panel.transform, y, () =>
            {
                _sfxOn = !_sfxOn;
                _sfxText.text = _sfxOn ? "🔊 SFX: ON" : "🔊 SFX: OFF";
                // TODO: wire to SFXManager
            });
            y -= 0.10f;

            // Language
            CreateButton("🌐 Language: English", _panel.transform, y, () =>
            {
                // TODO: wire to localization system
                Debug.Log("[Menu] Language toggle");
            });
            y -= 0.10f;

            // Reset energy (debug)
            CreateButton("⚡ Refill Energy (Debug)", _panel.transform, y, () =>
            {
                EnergySystem.Add(30);
            });
            y -= 0.10f;

            // Reset progress
            CreateButton("🗑️ Reset Progress", _panel.transform, y, () =>
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Menu] Progress reset!");
            });
            y -= 0.10f;

            // Rate Us
            CreateButton("⭐ Rate Us", _panel.transform, y, () =>
            {
                Application.OpenURL("https://play.google.com/store/apps/details?id=com.WonderMindsGames.PocketGarden");
            });
            y -= 0.10f;

            // Credits
            CreateButton("ℹ️ Credits: Wonder Minds Games", _panel.transform, y, () => { });
            y -= 0.10f;

            // Close
            var closeGo = new GameObject("CloseBtn");
            closeGo.transform.SetParent(_panel.transform, false);
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.7f, 0.3f, 0.3f);
            var closeBtn = closeGo.AddComponent<Button>();
            var closeRect = closeGo.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.3f, 0.03f);
            closeRect.anchorMax = new Vector2(0.7f, 0.09f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            CreateText("Close", closeGo.transform, Vector2.zero, Vector2.one, 24, Color.white);
            closeBtn.onClick.AddListener(Hide);

            _visible = true;
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            _visible = false;
        }

        private Text CreateButton(string label, Transform parent, float yCenter, System.Action onClick)
        {
            float h = 0.07f;
            var go = new GameObject("Btn");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.95f, 0.95f, 0.90f);
            var btn = go.AddComponent<Button>();
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.1f, yCenter - h * 0.5f);
            r.anchorMax = new Vector2(0.9f, yCenter + h * 0.5f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            btn.onClick.AddListener(() => onClick());

            var txt = CreateText(label, go.transform, Vector2.zero, Vector2.one, 24, new Color(0.2f, 0.2f, 0.2f));
            return txt;
        }

        private Text CreateText(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.text = text;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            return txt;
        }
    }
}
