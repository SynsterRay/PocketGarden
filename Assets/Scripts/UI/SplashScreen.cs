using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PocketGarden.UI
{
    /// <summary>
    /// Shows the studio logo on a white screen at launch (zoom-in → hold → fade-out), then
    /// removes itself — mirrors the Magic Pairs splash. The logo PNG (<c>developer_logo</c>)
    /// lives in <c>Resources/UIButtons</c>. Added to the Canvas by the scene setup so it draws
    /// on top of everything for its short duration.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        private GameObject _panel;
        private Image _logoImage;
        private Canvas _canvas;

        private void Start()
        {
            _canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (_canvas == null) { Destroy(this); return; }
            ShowSplash();
        }

        private void ShowSplash()
        {
            _panel = new GameObject("SplashPanel");
            _panel.transform.SetParent(_canvas.transform, false);
            var bg = _panel.AddComponent<Image>();
            bg.color = Color.white;
            var rect = _panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _panel.transform.SetAsLastSibling();

            var logoObj = new GameObject("Logo");
            logoObj.transform.SetParent(_panel.transform, false);
            _logoImage = logoObj.AddComponent<Image>();
            _logoImage.sprite = Resources.Load<Sprite>("UIButtons/developer_logo");
            _logoImage.preserveAspect = true;
            _logoImage.raycastTarget = false;
            var logoRect = logoObj.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.18f, 0.32f);
            logoRect.anchorMax = new Vector2(0.82f, 0.68f);
            logoRect.offsetMin = Vector2.zero;
            logoRect.offsetMax = Vector2.zero;

            // If the logo art is missing, show the studio name so the splash still reads.
            if (_logoImage.sprite == null)
            {
                _logoImage.color = new Color(1f, 1f, 1f, 0f);
                UIFactory.Text(_panel.transform, "Wonder Minds Games",
                    new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f), 40, UIFactory.LeafDark);
            }

            StartCoroutine(AnimateSplash());
        }

        private IEnumerator AnimateSplash()
        {
            var logoRect = _logoImage.GetComponent<RectTransform>();
            const float duration = 1.4f, holdTime = 0.5f, fadeTime = 0.4f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = Mathf.Clamp01(t / duration);
                logoRect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, Mathf.SmoothStep(0f, 1f, ratio));
                _logoImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t / 0.3f));
                yield return null;
            }

            yield return new WaitForSeconds(holdTime);

            t = 0f;
            var bg = _panel.GetComponent<Image>();
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(t / fadeTime);
                _logoImage.color = new Color(1f, 1f, 1f, alpha);
                bg.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            Destroy(_panel);
            Destroy(this);
        }
    }
}
