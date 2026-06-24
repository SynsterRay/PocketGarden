using UnityEngine;
using PocketGarden.Core;

namespace PocketGarden.Grid
{
    public class MergeGridItem : MonoBehaviour
    {
        public MergeItemData Data { get; private set; }
        public GridCell Cell { get; set; }

        private SpriteRenderer _renderer;
        private TextMesh _label;

        public void Init(MergeItemData data, GridCell cell)
        {
            Data = data;
            Cell = cell;
            cell.Item = this;
            transform.position = cell.transform.position;

            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = 10;

            if (data.icon != null)
            {
                _renderer.sprite = data.icon;
                _renderer.color = Color.white;
            }
            else
            {
                // Prototype: colored square
                _renderer.sprite = CreateSquareSprite();
                _renderer.color = data.fallbackColor;
            }

            // Level label
            if (_label == null)
            {
                var labelObj = new GameObject("Label");
                labelObj.transform.SetParent(transform, false);
                labelObj.transform.localPosition = new Vector3(0, 0f, 0f);
                _label = labelObj.AddComponent<TextMesh>();
                _label.alignment = TextAlignment.Center;
                _label.anchor = TextAnchor.MiddleCenter;
                _label.characterSize = 0.06f;
                _label.fontSize = 40;
                _label.color = Color.white;
                _label.fontStyle = FontStyle.Bold;
                // Ensure label renders in front
                var mr = labelObj.GetComponent<MeshRenderer>();
                mr.sortingOrder = 50;
            }
            _label.text = data.displayName;
        }

        public void UpdateVisual(MergeItemData newData)
        {
            Data = newData;
            if (newData.icon != null)
            {
                _renderer.sprite = newData.icon;
                _renderer.color = Color.white;
            }
            else
            {
                _renderer.color = newData.fallbackColor;
            }
            _label.text = newData.displayName;

            // Punch scale feedback
            transform.localScale = Vector3.one * 1.3f;
        }

        private void Update()
        {
            // Smooth scale back to 1
            if (transform.localScale.x > 1.01f)
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 8f);
        }

        private static Sprite _squareSprite;
        public static Sprite CreateSquareSprite_Static()
        {
            if (_squareSprite != null) return _squareSprite;
            var tex = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _squareSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            return _squareSprite;
        }
        private static Sprite CreateSquareSprite() => CreateSquareSprite_Static();
    }
}
