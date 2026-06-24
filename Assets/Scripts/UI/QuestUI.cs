using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Quests;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    public class QuestUI : MonoBehaviour
    {
        private Text _questText;
        private Text _coinText;
        private RectTransform _dropZoneRect;

        public static QuestUI Instance { get; private set; }

        private void Awake() => Instance = this;

        private void Start()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Drop zone (visible area at bottom where player drags items to deliver)
            var dropZone = new GameObject("QuestDropZone");
            dropZone.transform.SetParent(canvas.transform, false);
            var dzImg = dropZone.AddComponent<Image>();
            dzImg.color = new Color(0.3f, 0.8f, 0.4f, 0.3f);
            dzImg.raycastTarget = false;
            _dropZoneRect = dropZone.GetComponent<RectTransform>();
            _dropZoneRect.anchorMin = new Vector2(0.05f, 0.0f);
            _dropZoneRect.anchorMax = new Vector2(0.95f, 0.10f);
            _dropZoneRect.offsetMin = Vector2.zero;
            _dropZoneRect.offsetMax = Vector2.zero;

            // Label on drop zone
            var dzLabel = new GameObject("DropLabel").AddComponent<Text>();
            dzLabel.transform.SetParent(dropZone.transform, false);
            dzLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dzLabel.fontSize = 20;
            dzLabel.fontStyle = FontStyle.Bold;
            dzLabel.alignment = TextAnchor.MiddleCenter;
            dzLabel.color = new Color(0.1f, 0.4f, 0.1f);
            dzLabel.text = "⬇️ DROP HERE TO DELIVER ⬇️";
            var dlRect = dzLabel.GetComponent<RectTransform>();
            dlRect.anchorMin = Vector2.zero;
            dlRect.anchorMax = Vector2.one;
            dlRect.offsetMin = Vector2.zero;
            dlRect.offsetMax = Vector2.zero;

            // Quest display (above drop zone)
            var go = new GameObject("QuestDisplay");
            go.transform.SetParent(canvas.transform, false);
            _questText = go.AddComponent<Text>();
            _questText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _questText.fontSize = 22;
            _questText.alignment = TextAnchor.MiddleCenter;
            _questText.color = new Color(0.2f, 0.2f, 0.2f);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.10f);
            rect.anchorMax = new Vector2(0.95f, 0.17f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Coins (top right)
            var coinGo = new GameObject("CoinDisplay");
            coinGo.transform.SetParent(canvas.transform, false);
            _coinText = coinGo.AddComponent<Text>();
            _coinText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _coinText.fontSize = 28;
            _coinText.fontStyle = FontStyle.Bold;
            _coinText.alignment = TextAnchor.MiddleRight;
            _coinText.color = new Color(0.85f, 0.65f, 0.1f);
            var coinRect = coinGo.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0.6f, 0.92f);
            coinRect.anchorMax = new Vector2(0.82f, 0.98f);
            coinRect.offsetMin = Vector2.zero;
            coinRect.offsetMax = Vector2.zero;

            QuestManager.OnQuestUpdated += UpdateQuests;
            CoinSystem.OnCoinsChanged += UpdateCoins;
            UpdateCoins(CoinSystem.Coins);
        }

        private void UpdateQuests()
        {
            var qm = FindAnyObjectByType<QuestManager>();
            if (qm == null || _questText == null) return;

            var sb = new System.Text.StringBuilder();
            foreach (var q in qm.ActiveQuests)
                sb.Append($"📋 {q.description} ({q.currentAmount}/{q.requiredAmount})  ");
            _questText.text = sb.ToString();
        }

        private void UpdateCoins(int coins)
        {
            if (_coinText != null)
                _coinText.text = $"🪙 {coins}";
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
            CoinSystem.OnCoinsChanged -= UpdateCoins;
        }
    }
}
