using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>
    /// Shows a "Board Full!" popup with shake + suggestion when the board is full.
    /// Listens to MergeGrid.OnBoardFull event.
    /// </summary>
    public class BoardFullPopup : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _popup;
        private float _lastShown;

        private void Start()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            Grid.MergeGrid.OnBoardFull += OnBoardFull;
        }

        private void OnDestroy()
        {
            Grid.MergeGrid.OnBoardFull -= OnBoardFull;
        }

        private void OnBoardFull()
        {
            // Don't spam: 5s cooldown.
            if (Time.realtimeSinceStartup - _lastShown < 5f) return;
            _lastShown = Time.realtimeSinceStartup;

            StartCoroutine(ShakeAndPopup());
        }

        private IEnumerator ShakeAndPopup()
        {
            // Quick grid shake (world-space).
            var grid = FindAnyObjectByType<Grid.MergeGrid>();
            if (grid != null)
            {
                var originalPos = grid.transform.position;
                float t = 0f;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    float shakeX = Random.Range(-0.04f, 0.04f);
                    float shakeY = Random.Range(-0.04f, 0.04f);
                    grid.transform.position = originalPos + new Vector3(shakeX, shakeY, 0f);
                    yield return null;
                }
                grid.transform.position = originalPos;
            }

            // Show toast popup.
            if (_canvas == null) yield break;
            if (_popup != null) Destroy(_popup);

            _popup = new GameObject("BoardFullToast");
            _popup.transform.SetParent(_canvas.transform, false);
            _popup.transform.SetAsLastSibling();

            var bg = _popup.AddComponent<Image>();
            bg.color = new Color(0.85f, 0.25f, 0.2f, 0.92f);
            bg.sprite = UIFactory.RoundedSprite();
            bg.type = Image.Type.Sliced;
            bg.raycastTarget = false;

            var rt = _popup.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.15f, 0.45f);
            rt.anchorMax = new Vector2(0.85f, 0.55f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txt = UIFactory.Text(_popup.transform, "📦 Board full! Merge items to free space.",
                Vector2.zero, Vector2.one, 22, Color.white);
            txt.raycastTarget = false;

            // Fade out after 2.5s.
            yield return new WaitForSeconds(2.5f);
            float fade = 0f;
            while (fade < 0.5f)
            {
                fade += Time.deltaTime;
                float a = 1f - (fade / 0.5f);
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, a * 0.92f);
                txt.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            Destroy(_popup);
            _popup = null;
        }
    }
}
