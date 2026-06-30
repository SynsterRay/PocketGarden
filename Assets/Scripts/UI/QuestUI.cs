using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Quests;

namespace PocketGarden.UI
{
    /// <summary>
    /// Bottom-of-screen quest area: a card listing active quests + the delivery drop zone.
    /// Currency counters live in HudBar now, so this only owns quests + the drop target.
    /// </summary>
    public class QuestUI : MonoBehaviour
    {
        private Text _questText;
        private RectTransform _dropZoneRect;

        public static QuestUI Instance { get; private set; }

        private void Awake() => Instance = this;

        private void Start()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // --- Ad banner placeholder (very bottom strip, full width) -------------------
            // Reserves a standard mobile banner slot below the gameplay panels.
            var banner = UIFactory.BorderedPanel(canvas.transform, new Vector2(0.04f, 0.012f),
                new Vector2(0.96f, 0.078f), new Color(0.96f, 0.97f, 0.93f, 1f), UIFactory.Border, "AdBannerPlaceholder");
            banner.GetComponent<Image>().raycastTarget = false;
            var adLabel = UIFactory.Text(banner.transform, "Ad Banner", Vector2.zero, Vector2.one,
                20, new Color(0.6f, 0.62f, 0.58f));
            adLabel.raycastTarget = false;

            // --- Quest + delivery band (side by side, level with each other) -------------
            // Quest card on the LEFT, the drag-to-deliver zone on the RIGHT.
            const float bandBottom = 0.088f, bandTop = 0.215f;

            // Quest card (white, light rounded border).
            var card = UIFactory.BorderedPanel(canvas.transform, new Vector2(0.04f, bandBottom),
                new Vector2(0.55f, bandTop), UIFactory.White, UIFactory.Border, "QuestCard");
            _questText = UIFactory.Text(card.transform, "", new Vector2(0.06f, 0.06f),
                new Vector2(0.96f, 0.94f), 18, UIFactory.Ink);
            _questText.alignment = TextAnchor.MiddleLeft;

            // Delivery / drag zone (white with leaf-green border so it reads as a drop target).
            var dropFill = UIFactory.BorderedPanel(canvas.transform, new Vector2(0.57f, bandBottom),
                new Vector2(0.96f, bandTop), new Color(0.90f, 0.97f, 0.90f, 1f), UIFactory.Leaf, "QuestDropZone");
            // Use the OUTER panel rect for hit-testing the full footprint.
            _dropZoneRect = dropFill.transform.parent.GetComponent<RectTransform>();
            dropFill.GetComponent<Image>().raycastTarget = false;
            dropFill.transform.parent.GetComponent<Image>().raycastTarget = false;
            var dzLabel = UIFactory.Text(dropFill.transform, "Drag items\nhere to deliver",
                Vector2.zero, Vector2.one, 18, UIFactory.LeafDark);
            dzLabel.raycastTarget = false;

            QuestManager.OnQuestUpdated += UpdateQuests;
            UpdateQuests();
        }

        private void UpdateQuests()
        {
            var qm = FindAnyObjectByType<QuestManager>();
            if (qm == null || _questText == null) return;

            var sb = new System.Text.StringBuilder();
            foreach (var q in qm.ActiveQuests)
            {
                string reward = q.gemReward > 0
                    ? $"🪙{q.coinReward}  💎{q.gemReward}"
                    : $"🪙{q.coinReward}";
                sb.AppendLine($"📋 {q.description}  ({q.currentAmount}/{q.requiredAmount})   {reward}");
            }
            _questText.text = sb.ToString().TrimEnd();
        }

        /// <summary>Check if screen position is inside the drop zone.</summary>
        public bool IsInDropZone(Vector2 screenPos)
        {
            if (_dropZoneRect == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(_dropZoneRect, screenPos, null);
        }

        private void OnDestroy()
        {
            QuestManager.OnQuestUpdated -= UpdateQuests;
        }
    }
}
