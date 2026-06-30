using UnityEngine;
using UnityEngine.UI;

namespace PocketGarden.UI
{
    /// <summary>
    /// Central UI builder so every screen shares the same look (rounded panels/buttons,
    /// one font, consistent spacing). Also provides placeholder "art tiles" that reserve the
    /// exact on-screen footprint the final generated sprites will occupy, so dropping real
    /// art in later does not shift the layout.
    ///
    /// Palette (cozy garden):
    ///   Cream bg, leaf green accents, honey/gold coins, sky-blue gems, soft shadows.
    /// </summary>
    public static class UIFactory
    {
        // Shared palette
        public static readonly Color Cream      = new(0.98f, 0.99f, 0.94f);
        public static readonly Color Leaf       = new(0.30f, 0.62f, 0.30f);
        public static readonly Color LeafDark   = new(0.18f, 0.45f, 0.20f);
        public static readonly Color Ink        = new(0.22f, 0.24f, 0.22f);
        public static readonly Color Gold       = new(0.92f, 0.72f, 0.16f);
        public static readonly Color Gem        = new(0.26f, 0.62f, 0.86f);
        public static readonly Color EnergyTeal = new(0.20f, 0.70f, 0.55f);
        public static readonly Color Danger     = new(0.82f, 0.32f, 0.30f);
        public static readonly Color PanelBg    = new(1f, 1f, 1f, 0.98f);
        public static readonly Color White      = new(1f, 1f, 1f, 1f);
        public static readonly Color Border     = new(0.80f, 0.84f, 0.74f, 1f); // soft leaf-tinted rim

        private static Font _font;
        public static Font GetFont()
        {
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }

        // --- Rounded sprite (procedural, cached, sliced) ---------------------

        private static Sprite _rounded;
        public static Sprite RoundedSprite()
        {
            if (_rounded != null) return _rounded;
            const int size = 48, radius = 14;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, RoundedAlpha(x, y, size, size, radius)));
            tex.Apply();
            _rounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            return _rounded;
        }

        private static float RoundedAlpha(int x, int y, int w, int h, int r)
        {
            float dx = 0f, dy = 0f;
            if (x < r) dx = r - x; else if (x > w - 1 - r) dx = x - (w - 1 - r);
            if (y < r) dy = r - y; else if (y > h - 1 - r) dy = y - (h - 1 - r);
            if (dx > 0f && dy > 0f)
            {
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d > r) return 0f;
                if (d > r - 1.5f) return Mathf.Clamp01((r - d) / 1.5f);
            }
            return 1f;
        }

        // --- Builders --------------------------------------------------------

        public static RectTransform Stretch(RectTransform r, Vector2 min, Vector2 max)
        {
            r.anchorMin = min; r.anchorMax = max;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            return r;
        }

        public static Text Text(Transform parent, string text, Vector2 min, Vector2 max,
            int fontSize, Color color, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = GetFont();
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.alignment = align;
            t.color = color;
            t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Max(10, fontSize / 2);
            t.resizeTextMaxSize = fontSize;
            Stretch(go.GetComponent<RectTransform>(), min, max);
            return t;
        }

        public static GameObject Panel(Transform parent, Vector2 min, Vector2 max, Color color, string name = "Panel")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.sprite = RoundedSprite();
            img.type = Image.Type.Sliced;
            Stretch(go.GetComponent<RectTransform>(), min, max);
            return go;
        }

        /// <summary>
        /// A white (or tinted) panel with a thin rounded border — the standard gameplay panel look:
        /// a rectangle with slightly rounded corners and a soft outline. Returns the inner FILL
        /// object (parent your content to it); the border is drawn by an outer image behind it.
        /// </summary>
        public static GameObject BorderedPanel(Transform parent, Vector2 min, Vector2 max,
            Color? fill = null, Color? border = null, string name = "Panel", float thickness = 4f)
        {
            var outer = new GameObject(name);
            outer.transform.SetParent(parent, false);
            var bimg = outer.AddComponent<Image>();
            bimg.color = border ?? Border;
            bimg.sprite = RoundedSprite();
            bimg.type = Image.Type.Sliced;
            Stretch(outer.GetComponent<RectTransform>(), min, max);

            var inner = new GameObject("Fill");
            inner.transform.SetParent(outer.transform, false);
            var fimg = inner.AddComponent<Image>();
            fimg.color = fill ?? White;
            fimg.sprite = RoundedSprite();
            fimg.type = Image.Type.Sliced;
            var r = inner.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(thickness, thickness);
            r.offsetMax = new Vector2(-thickness, -thickness);
            return inner;
        }

        public static Button Button(Transform parent, string label, Vector2 min, Vector2 max,
            Color color, int fontSize = 24, Color? textColor = null)
        {
            var go = new GameObject("Btn");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.sprite = RoundedSprite();
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();
            Stretch(go.GetComponent<RectTransform>(), min, max);
            var t = Text(go.transform, label, Vector2.zero, Vector2.one, fontSize, textColor ?? Color.white);
            var tr = t.GetComponent<RectTransform>();
            tr.offsetMin = new Vector2(8f, 4f);
            tr.offsetMax = new Vector2(-8f, -4f);
            return btn;
        }

        /// <summary>
        /// A placeholder art tile that reserves the footprint of a future sprite. Shows a soft
        /// rounded block + short label so designers can see where art goes. Replace by assigning
        /// a real Sprite to the returned Image (keep the same RectTransform to preserve layout).
        /// </summary>
        public static Image ArtTile(Transform parent, string label, Vector2 min, Vector2 max, Color tint)
        {
            var go = new GameObject($"Art_{label}");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = tint;
            img.sprite = RoundedSprite();
            img.type = Image.Type.Sliced;
            img.preserveAspect = false; // fills reserved footprint exactly
            Stretch(go.GetComponent<RectTransform>(), min, max);
            if (!string.IsNullOrEmpty(label))
            {
                var t = Text(go.transform, label, Vector2.zero, Vector2.one, 16, new Color(1f, 1f, 1f, 0.9f));
                t.raycastTarget = false;
            }
            return img;
        }
    }
}
