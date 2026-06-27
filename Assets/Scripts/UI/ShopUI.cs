using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>
    /// Scrollable shop. Uses a ScrollRect so any number of SKUs fits without overflowing the
    /// screen. Each row is a rounded card: name + contents on the left, price button on the
    /// right, optional value tag. Purchases route through IAPManager (with an Editor fallback).
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        private GameObject _panel;
        private bool _visible;

        public void Toggle() { if (_visible) Hide(); else Show(); }

        public void Show()
        {
            if (_panel != null) { _panel.SetActive(true); _panel.transform.SetAsLastSibling(); _visible = true; return; }

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _panel = new GameObject("ShopPanel");
            _panel.transform.SetParent(canvas.transform, false);
            _panel.transform.SetAsLastSibling();
            var bg = _panel.AddComponent<Image>();
            bg.color = UIFactory.Cream;
            UIFactory.Stretch(_panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

            // Header
            UIFactory.Text(_panel.transform, "🛒 Shop", new Vector2(0.08f, 0.90f),
                new Vector2(0.7f, 0.97f), 40, UIFactory.LeafDark, TextAnchor.MiddleLeft);
            var close = UIFactory.Button(_panel.transform, "✕", new Vector2(0.82f, 0.905f),
                new Vector2(0.94f, 0.965f), UIFactory.Danger, 28);
            close.onClick.AddListener(Hide);

            // Scroll view (between header and the ad/close footer)
            BuildScrollList(_panel.transform, new Vector2(0.04f, 0.17f), new Vector2(0.96f, 0.89f));

            // Rewarded ad (footer, fixed)
            var ad = UIFactory.Button(_panel.transform, "▶  Watch Ad  =  +10 Energy",
                new Vector2(0.12f, 0.05f), new Vector2(0.88f, 0.13f), UIFactory.Gold, 24, UIFactory.Ink);
            ad.onClick.AddListener(OnWatchAd);

            _visible = true;
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            _visible = false;
        }

        private void BuildScrollList(Transform parent, Vector2 min, Vector2 max)
        {
            // Scroll root
            var scrollGo = new GameObject("Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(parent, false);
            UIFactory.Stretch(scrollGo.GetComponent<RectTransform>(), min, max);
            scrollGo.AddComponent<RectMask2D>();
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 25f;

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scrollGo.transform, false);
            var cRect = content.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(0.5f, 1f);
            cRect.sizeDelta = Vector2.zero;
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = cRect;

            foreach (var item in ShopCatalog.Items)
                CreateRow(content.transform, item);
        }

        private void CreateRow(Transform parent, ShopItem item)
        {
            var row = UIFactory.Panel(parent, Vector2.zero, Vector2.one, new Color(1f, 1f, 1f, 0.95f), $"Row_{item.id}");
            row.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 150f;

            // Name (top-left)
            UIFactory.Text(row.transform, item.displayName, new Vector2(0.04f, 0.50f),
                new Vector2(0.66f, 0.92f), 26, UIFactory.Ink, TextAnchor.LowerLeft);

            // Contents summary (bottom-left)
            var sb = new System.Text.StringBuilder();
            if (item.energyAmount > 0) sb.Append($"⚡{item.energyAmount}  ");
            if (item.gemAmount > 0) sb.Append($"💎{item.gemAmount}  ");
            if (item.coinAmount > 0) sb.Append($"🪙{item.coinAmount}");
            if (sb.Length > 0)
                UIFactory.Text(row.transform, sb.ToString(), new Vector2(0.04f, 0.12f),
                    new Vector2(0.66f, 0.48f), 20, new Color(0.4f, 0.4f, 0.4f), TextAnchor.UpperLeft);

            // Value tag badge
            if (!string.IsNullOrEmpty(item.tag))
                UIFactory.Text(row.transform, item.tag, new Vector2(0.04f, 0.0f),
                    new Vector2(0.4f, 0.12f), 16, new Color(0.85f, 0.45f, 0.1f), TextAnchor.LowerLeft);

            // Price button (right)
            var buy = UIFactory.Button(row.transform, item.priceLabel, new Vector2(0.70f, 0.18f),
                new Vector2(0.97f, 0.82f), UIFactory.Leaf, 26);
            var captured = item;
            buy.onClick.AddListener(() => OnBuy(captured));
        }

        private void OnBuy(ShopItem item)
        {
            var iap = IAPManager.Instance;
            if (iap != null && iap.IsInitialized)
                iap.BuyProduct(item.iapProductId, _ => { /* UI auto-updates via currency events */ });
            else
                ShopCatalog.GrantPurchase(item); // editor / store-unavailable fallback
        }

        private void OnWatchAd()
        {
            var ads = PocketGarden.Ads.AdManager.Instance;
            if (ads != null)
                ads.ShowRewarded(() => EnergySystem.Add(10));
            else
                EnergySystem.Add(10); // editor fallback
        }
    }
}
