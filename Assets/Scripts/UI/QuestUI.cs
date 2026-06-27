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

            // Drop zone (bottom strip).
            var dropZone = UIFactory.Panel(canvas.transform, new Vector2(0.04f, 0.015f),
                new Vector2(0.96f, 0.085f), new Color(0.30f, 0.78f, 0.42f, 0.92f), "QuestDropZone");
            _dropZoneRect = dropZone.GetComponent<RectTransform>();
            dropZone.GetComponent<Image>().raycastTarget = false;
            var dzLabel = UIFactory.Text(dropZone.transform, "Drag items here to deliver",
                Vector2.zero, Vector2.one, 22, Color.white);
            dzLabel.raycastTarget = false;

            // Quest card (above drop zone).
            var card = UIFactory.Panel(canvas.transform, new Vector2(0.04f, 0.095f),
                new Vector2(0.96f, 0.205f), UIFactory.PanelBg, "QuestCard");
            _questText = UIFactory.Text(card.transform, "", new Vector2(0.04f, 0.05f),
                new Vector2(0.96f, 0.95f), 22, UIFactory.Ink);
            _questText.alignment = TextAnchor.MiddleLeft;

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
