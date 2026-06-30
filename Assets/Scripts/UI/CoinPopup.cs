using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>
    /// Floating "+N" coin reward that pops in, drifts up and fades out whenever the player's coin
    /// balance increases (e.g. after completing and delivering a quest). Includes a spinning gold
    /// circle that simulates a rotating coin (scaleX oscillation = 3D flip). Driven by
    /// <see cref="CoinSystem.OnCoinsChanged"/>; lives on the Canvas (added by the scene setup).
    /// </summary>
    public class CoinPopup : MonoBehaviour
    {
        private Canvas _canvas;
        private int _lastCoins;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
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

            // Spinning gold coin disc (circle whose scaleX oscillates to simulate 3D rotation).
            var coinGo = new GameObject("CoinDisc");
            coinGo.transform.SetParent(go.transform, false);
            var coinImg = coinGo.AddComponent<Image>();
            coinImg.color = new Color(1f, 0.82f, 0.1f); // gold
            coinImg.sprite = UIFactory.RoundedSprite();
            coinImg.type = Image.Type.Sliced;
            coinImg.raycastTarget = false;
            var coinRect = coinGo.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0.18f, 0.5f);
            coinRect.anchorMax = new Vector2(0.18f, 0.5f);
            coinRect.pivot = new Vector2(0.5f, 0.5f);
            coinRect.sizeDelta = new Vector2(56f, 56f);

            // Inner ring (darker gold, gives coin depth).
            var inner = new GameObject("Inner");
            inner.transform.SetParent(coinGo.transform, false);
            var innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0.85f, 0.65f, 0f);
            innerImg.sprite = UIFactory.RoundedSprite();
            innerImg.type = Image.Type.Sliced;
            innerImg.raycastTarget = false;
            var ir = inner.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.2f, 0.2f);
            ir.anchorMax = new Vector2(0.8f, 0.8f);
            ir.offsetMin = Vector2.zero;
            ir.offsetMax = Vector2.zero;

            // Amount text.
            var txt = UIFactory.Text(go.transform, $"+{amount}",
                new Vector2(0.28f, 0f), new Vector2(0.95f, 1f), 48, UIFactory.Gold);
            txt.fontStyle = FontStyle.Bold;
            txt.raycastTarget = false;

            StartCoroutine(Animate(go, txt, coinRect, coinImg, innerImg));
        }

        private IEnumerator Animate(GameObject go, Text txt, RectTransform coinRect,
            Image coinImg, Image innerImg)
        {
            var rect = go.GetComponent<RectTransform>();
            Vector2 start = rect.anchoredPosition;
            const float duration = 1.3f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float r = t / duration;

                // Drift up.
                rect.anchoredPosition = start + Vector2.up * (r * 90f);

                // Punch in, settle.
                float scale = r < 0.18f ? Mathf.Lerp(0.5f, 1.25f, r / 0.18f)
                            : r < 0.32f ? Mathf.Lerp(1.25f, 1f, (r - 0.18f) / 0.14f) : 1f;
                rect.localScale = Vector3.one * scale;

                // Spinning coin: oscillate scaleX to simulate a 3D flip.
                float spin = Mathf.Cos(t * 10f);
                coinRect.localScale = new Vector3(Mathf.Abs(spin) * 0.8f + 0.2f, 1f, 1f);

                // Fade out the last 35%.
                if (r > 0.65f)
                {
                    float a = Mathf.Lerp(1f, 0f, (r - 0.65f) / 0.35f);
                    var c = txt.color; c.a = a; txt.color = c;
                    var cc = coinImg.color; cc.a = a; coinImg.color = cc;
                    var ic = innerImg.color; ic.a = a; innerImg.color = ic;
                }
                yield return null;
            }

            Destroy(go);
        }
    }
}
