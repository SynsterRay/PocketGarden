using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>
    /// Small reusable confirmation popup for spending gems (energy refill, generator skip, …).
    /// If the player can't afford it, the confirm button turns into "Get Gems" → opens the Shop.
    /// Usage: <c>GemConfirmPopup.Show("Refill energy?", cost, () => GemEconomy.TryRefillEnergy());</c>
    /// </summary>
    public class GemConfirmPopup : MonoBehaviour
    {
        private GameObject _panel;

        public static void Show(string title, int gemCost, System.Action onConfirm)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
            var host = FindAnyObjectByType<GemConfirmPopup>() ?? canvas.gameObject.AddComponent<GemConfirmPopup>();
            host.Build(canvas, title, gemCost, onConfirm);
        }

        private void Build(Canvas canvas, string title, int gemCost, System.Action onConfirm)
        {
            if (_panel != null) Destroy(_panel);

            _panel = new GameObject("GemConfirmPopup");
            _panel.transform.SetParent(canvas.transform, false);
            _panel.transform.SetAsLastSibling();
            var dim = _panel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.7f);
            UIFactory.Stretch(_panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

            var card = UIFactory.Panel(_panel.transform, new Vector2(0.15f, 0.38f),
                new Vector2(0.85f, 0.62f), UIFactory.Cream, "Card");

            UIFactory.Text(card.transform, title, new Vector2(0.06f, 0.55f),
                new Vector2(0.94f, 0.92f), 28, UIFactory.Ink);

            bool affordable = GemEconomy.CanAfford(gemCost);
            string confirmLabel = affordable ? $"💎 {gemCost}" : "Get Gems";
            var confirm = UIFactory.Button(card.transform, confirmLabel, new Vector2(0.52f, 0.12f),
                new Vector2(0.92f, 0.42f), affordable ? UIFactory.Gem : UIFactory.Gold, 26);
            confirm.onClick.AddListener(() =>
            {
                if (GemEconomy.CanAfford(gemCost))
                {
                    onConfirm?.Invoke();
                    Close();
                }
                else
                {
                    Close();
                    var shop = FindAnyObjectByType<ShopUI>() ?? canvas.gameObject.AddComponent<ShopUI>();
                    shop.Show();
                }
            });

            var cancel = UIFactory.Button(card.transform, "Cancel", new Vector2(0.08f, 0.12f),
                new Vector2(0.48f, 0.42f), new Color(0.75f, 0.75f, 0.72f), 26);
            cancel.onClick.AddListener(Close);
        }

        private void Close()
        {
            if (_panel != null) Destroy(_panel);
            _panel = null;
        }
    }
}
