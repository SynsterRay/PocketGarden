using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    public class ShopUI : MonoBehaviour
    {
        private GameObject _panel;
        private bool _visible;

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

            _panel = new GameObject("ShopPanel");
            _panel.transform.SetParent(canvas.transform, false);
            _panel.transform.SetAsLastSibling();

            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.97f);
            var rect = _panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Title
            CreateText("Shop", _panel.transform, new Vector2(0.3f, 0.88f), new Vector2(0.7f, 0.95f), 36,
                new Color(0.2f, 0.5f, 0.2f));

            // Items
            float y = 0.80f;
            foreach (var item in ShopCatalog.Items)
            {
                float yBottom = y - 0.08f;
                CreateText($"{item.displayName}  —  {item.priceLabel}", _panel.transform,
                    new Vector2(0.08f, yBottom), new Vector2(0.72f, y), 22, new Color(0.2f, 0.2f, 0.2f));

                // Buy button
                var btnGo = new GameObject($"Buy_{item.id}");
                btnGo.transform.SetParent(_panel.transform, false);
                var btnImg = btnGo.AddComponent<Image>();
                btnImg.color = new Color(0.2f, 0.7f, 0.3f);
                var btn = btnGo.AddComponent<Button>();
                var btnRect = btnGo.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.75f, yBottom);
                btnRect.anchorMax = new Vector2(0.92f, y);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;

                CreateText("Buy", btnGo.transform, Vector2.zero, Vector2.one, 20, Color.white);

                var captured = item;
                btn.onClick.AddListener(() => OnBuy(captured));

                y -= 0.10f;
            }

            // Rewarded ad button
            float adY = y - 0.02f;
            var adBtnGo = new GameObject("AdBtn");
            adBtnGo.transform.SetParent(_panel.transform, false);
            var adImg = adBtnGo.AddComponent<Image>();
            adImg.color = new Color(0.8f, 0.6f, 0.1f);
            var adBtn = adBtnGo.AddComponent<Button>();
            var adRect = adBtnGo.GetComponent<RectTransform>();
            adRect.anchorMin = new Vector2(0.2f, adY - 0.06f);
            adRect.anchorMax = new Vector2(0.8f, adY);
            adRect.offsetMin = Vector2.zero;
            adRect.offsetMax = Vector2.zero;
            CreateText("▶ Watch Ad = +5 Energy", adBtnGo.transform, Vector2.zero, Vector2.one, 22, Color.white);
            adBtn.onClick.AddListener(OnWatchAd);

            // Close button
            var closeGo = new GameObject("CloseBtn");
            closeGo.transform.SetParent(_panel.transform, false);
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.8f, 0.2f, 0.2f);
            var closeBtn = closeGo.AddComponent<Button>();
            var closeRect = closeGo.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.35f, 0.03f);
            closeRect.anchorMax = new Vector2(0.65f, 0.09f);
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

        private void OnBuy(ShopItem item)
        {
            // TODO: Wire to Unity Purchasing IAP
            // For now, grant items directly (prototype)
            Debug.Log($"[Shop] Purchase: {item.iapProductId}");
            if (item.energyAmount > 0) EnergySystem.Add(item.energyAmount);
            if (item.gemAmount > 0) GemSystem.Add(item.gemAmount);
            if (item.coinAmount > 0) CoinSystem.Add(item.coinAmount);
        }

        private void OnWatchAd()
        {
            // TODO: Wire to AdMob rewarded ad (ShopCatalog.AD_REWARDED)
            Debug.Log($"[Shop] Rewarded ad: {ShopCatalog.AD_REWARDED}");
            EnergySystem.Add(5);
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
