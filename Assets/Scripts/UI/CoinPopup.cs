using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>
    /// Floating "+N" coin reward that pops in, drifts up and fades out whenever the player's coin
    /// balance increases (e.g. after completing and delivering a quest). Driven by
    /// <see cref="CoinSystem.OnCoinsChanged"/>; lives on the Canvas (added by the scene setup).
    /// </summary>
    public class CoinPopup : MonoBehaviour
    {
        private Canvas _canvas;
        private int _lastCoins;
        private Sprite _coinIcon;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            _coinIcon = Resources.Load<Sprite>("UIButtons/icon_coin");
        }

        private void OnEnable()
        {
            _lastCoins = CoinSystem.Coins;
            CoinSystem.OnCoinsChanged += OnCoinsChanged;
        }

        private void OnDisable() => CoinSystem.OnCoinsChanged -= OnCoinsChanged;

        private void OnCoinsChanged(int newCoins)
        {
            int delta = newCoins - _lastCoins;
            _lastCoins = newCoins;
            if (delta > 0) Spawn(delta);
        }

        private void Spawn(int amount)
        {
            if (_canvas == null) return;

            var go = new GameObject("CoinPopup");
            go.transform.SetParent(_canvas.transform, false);
            go.transform.SetAsLastSibling();
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.30f, 0.46f);
            rect.anchorMax = new Vector2(0.70f, 0.56f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Coin icon (left of the text), if the art is present.
            Image icon = null;
            if (_coinIcon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(go.transform, false);
                icon = iconGo.AddComponent<Image>();
                icon.sprite = _coinIcon;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                var ir = iconGo.GetComponent<RectTransform>();
                ir.anchorMin = new Vector2(0.16f, 0.5f);
                ir.anchorMax = new Vector2(0.16f, 0.5f);
                ir.pivot = new Vector2(0.5f, 0.5f);
                ir.sizeDelta = new Vector2(56f, 56f);
            }

            var txt = UIFactory.Text(go.transform, $"+{amount}",
                new Vector2(0.28f, 0f), new Vector2(0.95f, 1f), 48, UIFactory.Gold);
            txt.fontStyle = FontStyle.Bold;
            txt.raycastTarget = false;

            StartCoroutine(Animate(go, txt, icon));
        }

        private IEnumerator Animate(GameObject go, Text txt, Image icon)
        {
            var rect = go.GetComponent<RectTransform>();
            Vector2 start = rect.anchoredPosition;
            const float duration = 1.3f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float r = t / duration;

                rect.anchoredPosition = start + Vector2.up * (r * 90f);

                // Punch in, settle.
                float scale = r < 0.18f ? Mathf.Lerp(0.5f, 1.25f, r / 0.18f)
                            : r < 0.32f ? Mathf.Lerp(1.25f, 1f, (r - 0.18f) / 0.14f) : 1f;
                rect.localScale = Vector3.one * scale;

                // Fade out the last 35%.
                if (r > 0.65f)
                {
                    float a = Mathf.Lerp(1f, 0f, (r - 0.65f) / 0.35f);
                    var c = txt.color; c.a = a; txt.color = c;
                    if (icon != null) { var ic = icon.color; ic.a = a; icon.color = ic; }
                }
                yield return null;
            }

            Destroy(go);
        }
    }
}
