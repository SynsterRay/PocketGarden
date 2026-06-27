using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;
using PocketGarden.Quests;

namespace PocketGarden.UI
{
    /// <summary>
    /// Shows contextual, single-pack offers at natural stall points so the player can keep
    /// their development pace:
    ///   • Out of energy while a quest is active  -> Energy refill offer.
    ///   • Entering the Stone phase (q11 done)     -> one-time Starter Pack.
    ///   • Entering the Grind phase (q17 done)     -> Gardener's Bundle.
    /// Offers are honest: every panel has a clear "No thanks", the one-time Starter is shown
    /// once, and there is a real-time cooldown so prompts are never spammy.
    /// </summary>
    public class OfferManager : MonoBehaviour
    {
        private const float OfferCooldown = 150f;        // min seconds between any two offers
        private const int EnergyRearmThreshold = 5;      // energy must climb above this to re-arm the energy offer
        private const string StarterOfferedKey = "PG_StarterOffered";

        private Canvas _canvas;
        private GameObject _panel;
        private float _lastOfferTime = -999f;
        private bool _energyArmed = true;

        private void Start()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            EnergySystem.OnEnergyChanged += OnEnergyChanged;
            QuestManager.OnQuestComplete += OnQuestComplete;
        }

        private void OnDestroy()
        {
            EnergySystem.OnEnergyChanged -= OnEnergyChanged;
            QuestManager.OnQuestComplete -= OnQuestComplete;
        }

        // --- Triggers --------------------------------------------------------

        private void OnEnergyChanged(int energy)
        {
            if (energy > EnergyRearmThreshold) { _energyArmed = true; return; }

            if (energy == 0 && _energyArmed && CanShow())
            {
                _energyArmed = false; // wait until energy recovers before offering again
                // In later phases, energy stalls are deeper - offer the better-value bundle.
                string id = Progression.CurrentPhase == Progression.Phase.Grind ? "growth_bundle" : "energy_small";
                ShowOffer(ShopCatalog.Get(id),
                    "Out of energy?",
                    "Top up and keep your garden growing!",
                    allowGemRefill: true);
            }
        }

        private void OnQuestComplete(Quest q)
        {
            if (!CanShow()) return;

            // Entering Stone phase - offer the one-time Starter once.
            if (Progression.CompletedQuests == 11 && PlayerPrefs.GetInt(StarterOfferedKey, 0) == 0)
            {
                PlayerPrefs.SetInt(StarterOfferedKey, 1);
                PlayerPrefs.Save();
                ShowOffer(ShopCatalog.Get("starter"),
                    "New chains, new challenges!",
                    "Grab the one-time Starter Pack to power through.");
                return;
            }

            // Entering the grind - offer the value bundle to maintain pace.
            if (Progression.CompletedQuests == 17)
            {
                ShowOffer(ShopCatalog.Get("growth_bundle"),
                    "The big builds begin!",
                    "The Gardener's Bundle keeps your pace up.");
            }
        }

        private bool CanShow()
        {
            return _panel == null && Time.realtimeSinceStartup - _lastOfferTime >= OfferCooldown;
        }

        // --- Panel -----------------------------------------------------------

        private void ShowOffer(ShopItem item, string title, string subtitle, bool allowGemRefill = false)
        {
            if (item == null || _canvas == null) return;
            _lastOfferTime = Time.realtimeSinceStartup;

            _panel = new GameObject("OfferPanel");
            _panel.transform.SetParent(_canvas.transform, false);
            _panel.transform.SetAsLastSibling();
            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.82f);
            Stretch(_panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

            // Card
            var card = new GameObject("Card");
            card.transform.SetParent(_panel.transform, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.98f, 0.99f, 0.94f, 1f);
            Stretch(card.GetComponent<RectTransform>(), new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.72f));

            MakeText(card.transform, title, new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.95f), 30, new Color(0.2f, 0.45f, 0.2f));
            MakeText(card.transform, subtitle, new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.80f), 20, new Color(0.3f, 0.3f, 0.3f));

            // Contents summary
            var sb = new System.Text.StringBuilder();
            if (item.energyAmount > 0) sb.Append($"E {item.energyAmount}   ");
            if (item.gemAmount > 0) sb.Append($"Gems {item.gemAmount}   ");
            if (item.coinAmount > 0) sb.Append($"Coins {item.coinAmount}");
            MakeText(card.transform, sb.ToString(), new Vector2(0.05f, 0.46f), new Vector2(0.95f, 0.64f), 26, new Color(0.2f, 0.2f, 0.2f));

            if (!string.IsNullOrEmpty(item.tag))
                MakeText(card.transform, item.tag, new Vector2(0.30f, 0.36f), new Vector2(0.70f, 0.45f), 20, new Color(0.85f, 0.45f, 0.1f));

            // Buy button (price)
            var buy = MakeButton(card.transform, $"Get it - {item.priceLabel}",
                new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.30f), new Color(0.2f, 0.7f, 0.3f));
            buy.onClick.AddListener(() =>
            {
                var iap = IAPManager.Instance;
                if (iap != null && iap.IsInitialized)
                    iap.BuyProduct(item.iapProductId, _ => { });
                else
                    ShopCatalog.GrantPurchase(item); // editor / store-unavailable fallback
                Close();
            });

            // Bottom row: optional gem-refill alternative + No thanks.
            if (allowGemRefill && !EnergySystem.IsFull)
            {
                var refill = MakeButton(card.transform, $"Refill 💎{GemEconomy.EnergyRefillCost}",
                    new Vector2(0.10f, 0.04f), new Vector2(0.49f, 0.14f), UIFactory.Gem);
                refill.onClick.AddListener(() =>
                {
                    if (GemEconomy.TryRefillEnergy()) Close();
                    else { Close(); (FindAnyObjectByType<ShopUI>() ?? gameObject.AddComponent<ShopUI>()).Show(); }
                });

                var no2 = MakeButton(card.transform, "No thanks",
                    new Vector2(0.51f, 0.04f), new Vector2(0.90f, 0.14f), new Color(0.75f, 0.75f, 0.72f));
                no2.onClick.AddListener(Close);
            }
            else
            {
                var no = MakeButton(card.transform, "No thanks",
                    new Vector2(0.30f, 0.04f), new Vector2(0.70f, 0.14f), new Color(0.75f, 0.75f, 0.72f));
                no.onClick.AddListener(Close);
            }
        }

        private void Close()
        {
            if (_panel != null) Destroy(_panel);
            _panel = null;
        }

        // --- Helpers ---------------------------------------------------------

        private static void Stretch(RectTransform r, Vector2 min, Vector2 max)
        {
            r.anchorMin = min; r.anchorMax = max;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        private Text MakeText(Transform parent, string text, Vector2 min, Vector2 max, int size, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.text = text;
            Stretch(go.GetComponent<RectTransform>(), min, max);
            return t;
        }

        private Button MakeButton(Transform parent, string label, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject("Btn");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            Stretch(go.GetComponent<RectTransform>(), min, max);
            MakeText(go.transform, label, Vector2.zero, Vector2.one, 22, Color.white);
            return btn;
        }
    }
}
