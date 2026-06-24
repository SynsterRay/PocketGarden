using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    public class EnergyUI : MonoBehaviour
    {
        private Text _energyText;

        private void Start()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var go = new GameObject("EnergyDisplay");
            go.transform.SetParent(canvas.transform, false);
            _energyText = go.AddComponent<Text>();
            _energyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _energyText.fontSize = 28;
            _energyText.fontStyle = FontStyle.Bold;
            _energyText.alignment = TextAnchor.MiddleLeft;
            _energyText.color = new Color(0.2f, 0.6f, 0.1f);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.92f);
            rect.anchorMax = new Vector2(0.4f, 0.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Shop button
            var shopGo = new GameObject("ShopBtn");
            shopGo.transform.SetParent(canvas.transform, false);
            var shopImg = shopGo.AddComponent<Image>();
            shopImg.color = new Color(0.2f, 0.7f, 0.3f);
            var shopBtn = shopGo.AddComponent<Button>();
            var shopRect = shopGo.GetComponent<RectTransform>();
            shopRect.anchorMin = new Vector2(0.4f, 0.92f);
            shopRect.anchorMax = new Vector2(0.6f, 0.98f);
            shopRect.offsetMin = Vector2.zero;
            shopRect.offsetMax = Vector2.zero;
            var shopTxt = new GameObject("Text").AddComponent<Text>();
            shopTxt.transform.SetParent(shopGo.transform, false);
            shopTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            shopTxt.fontSize = 22;
            shopTxt.fontStyle = FontStyle.Bold;
            shopTxt.alignment = TextAnchor.MiddleCenter;
            shopTxt.color = Color.white;
            shopTxt.text = "Shop";
            var stRect = shopTxt.GetComponent<RectTransform>();
            stRect.anchorMin = Vector2.zero;
            stRect.anchorMax = Vector2.one;
            stRect.offsetMin = Vector2.zero;
            stRect.offsetMax = Vector2.zero;
            shopBtn.onClick.AddListener(() =>
            {
                var shop = FindAnyObjectByType<ShopUI>();
                if (shop == null) shop = canvas.gameObject.AddComponent<ShopUI>();
                shop.Toggle();
            });

            // Settings button
            var setGo = new GameObject("SettingsBtn");
            setGo.transform.SetParent(canvas.transform, false);
            var setImg = setGo.AddComponent<Image>();
            setImg.color = new Color(0.5f, 0.5f, 0.5f);
            var setBtn = setGo.AddComponent<Button>();
            var setRect = setGo.GetComponent<RectTransform>();
            setRect.anchorMin = new Vector2(0.85f, 0.92f);
            setRect.anchorMax = new Vector2(0.98f, 0.98f);
            setRect.offsetMin = Vector2.zero;
            setRect.offsetMax = Vector2.zero;
            var setTxt = new GameObject("Text").AddComponent<Text>();
            setTxt.transform.SetParent(setGo.transform, false);
            setTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            setTxt.fontSize = 22;
            setTxt.fontStyle = FontStyle.Bold;
            setTxt.alignment = TextAnchor.MiddleCenter;
            setTxt.color = Color.white;
            setTxt.text = "⚙️";
            var setTxtRect = setTxt.GetComponent<RectTransform>();
            setTxtRect.anchorMin = Vector2.zero;
            setTxtRect.anchorMax = Vector2.one;
            setTxtRect.offsetMin = Vector2.zero;
            setTxtRect.offsetMax = Vector2.zero;
            setBtn.onClick.AddListener(() =>
            {
                var menu = FindAnyObjectByType<MainMenu>();
                if (menu == null) menu = canvas.gameObject.AddComponent<MainMenu>();
                menu.Toggle();
            });

            EnergySystem.OnEnergyChanged += UpdateDisplay;
            UpdateDisplay(EnergySystem.Energy);
        }

        private void Update()
        {
            EnergySystem.Tick();
        }

        private void UpdateDisplay(int energy)
        {
            if (_energyText != null)
                _energyText.text = $"⚡ {energy}/{EnergySystem.Max}";
        }

        private void OnDestroy()
        {
            EnergySystem.OnEnergyChanged -= UpdateDisplay;
        }
    }
}
