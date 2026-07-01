using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PocketGarden.UI
{
    /// <summary>
    /// Animates item delivery: the item sprite flies in an arc toward the quest card,
    /// shrinks, then spawns a particle burst + checkmark. Triggered from DragDropHandler
    /// on successful delivery instead of instant destroy.
    /// </summary>
    public class DeliveryAnimation : MonoBehaviour
    {
        private static DeliveryAnimation _instance;
        public static DeliveryAnimation Instance => _instance;

        private Canvas _canvas;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(this); return; }
            _instance = this;
            _canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
        }

        /// <summary>
        /// Plays the delivery arc animation. Call this instead of immediately destroying the item.
        /// The item GameObject is destroyed at the end of the animation.
        /// </summary>
        /// <param name="itemGo">The item being delivered (will be destroyed).</param>
        /// <param name="startScreen">Screen position where the drag ended.</param>
        /// <param name="targetScreen">Screen position of the quest card (delivery target).</param>
        /// <param name="itemSprite">The sprite to show during the flight.</param>
        public void Play(GameObject itemGo, Vector2 startScreen, Vector2 targetScreen, Sprite itemSprite)
        {
            if (_canvas == null || itemSprite == null)
            {
                // Fallback: just destroy immediately.
                Destroy(itemGo);
                return;
            }

            // Hide the world-space item immediately.
            var sr = itemGo.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            StartCoroutine(FlyRoutine(itemGo, startScreen, targetScreen, itemSprite));
        }

        private IEnumerator FlyRoutine(GameObject itemGo, Vector2 startScreen, Vector2 targetScreen, Sprite sprite)
        {
            // Create a UI image to fly across the canvas.
            var flyGo = new GameObject("DeliveryFly");
            flyGo.transform.SetParent(_canvas.transform, false);
            flyGo.transform.SetAsLastSibling();
            var img = flyGo.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;

            var rt = flyGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80f, 80f);

            var canvasRect = _canvas.GetComponent<RectTransform>();

            const float duration = 0.45f;
            float t = 0f;

            // Arc height (pixels above the midpoint).
            float arcHeight = 120f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float r = Mathf.Clamp01(t / duration);

                // Lerp position with parabolic arc.
                Vector2 pos = Vector2.Lerp(startScreen, targetScreen, r);
                float arc = 4f * arcHeight * r * (1f - r); // parabola peak at midpoint
                pos.y += arc;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pos, null, out var local);
                rt.anchoredPosition = local;

                // Shrink as it approaches target.
                float scale = Mathf.Lerp(1f, 0.3f, r);
                rt.localScale = Vector3.one * scale;

                // Slight rotation for liveliness.
                rt.localEulerAngles = new Vector3(0f, 0f, r * 360f);

                yield return null;
            }

            // Burst particle at target.
            SpawnCheckmark(targetScreen, canvasRect);

            // Haptic feedback on delivery.
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif

            Destroy(flyGo);
            Destroy(itemGo);
        }

        private void SpawnCheckmark(Vector2 screenPos, RectTransform canvasRect)
        {
            var go = new GameObject("Checkmark");
            go.transform.SetParent(_canvas.transform, false);
            go.transform.SetAsLastSibling();

            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 52;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = UIFactory.Leaf;
            txt.text = "✓";
            txt.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80f, 80f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var local);
            rt.anchoredPosition = local;

            StartCoroutine(FadeCheckmark(go));
        }

        private IEnumerator FadeCheckmark(GameObject go)
        {
            var txt = go.GetComponent<Text>();
            var rt = go.GetComponent<RectTransform>();
            float t = 0f;
            const float dur = 0.7f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float r = t / dur;
                rt.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, r);
                rt.anchoredPosition += Vector2.up * Time.deltaTime * 40f;
                var c = txt.color;
                c.a = 1f - r;
                txt.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
