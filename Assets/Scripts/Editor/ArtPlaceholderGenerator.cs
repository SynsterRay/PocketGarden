using System.IO;
using UnityEngine;
using UnityEditor;

namespace PocketGarden.Editor
{
    /// <summary>
    /// Generates simple, recognizable placeholder PNG icons into Resources so the HUD shows real
    /// art instead of emoji. Run: <b>PocketGarden → Generate Placeholder Art</b>. Replace with
    /// final sprites later (same filenames/sizes → no layout change).
    ///
    /// Outputs (128×128, transparent):
    ///   Resources/UIButtons/icon_energy, icon_coin, icon_gem, icon_shop, icon_settings
    /// </summary>
    public static class ArtPlaceholderGenerator
    {
        private const string Folder = "Assets/Resources/UIButtons";
        private const int S = 128;

        [MenuItem("PocketGarden/Generate Placeholder Art")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder);

            Save("icon_energy",   BuildEnergy());
            Save("icon_coin",     BuildCoin());
            Save("icon_gem",      BuildGem());
            Save("icon_shop",     BuildShop());
            Save("icon_settings", BuildSettings());

            AssetDatabase.Refresh();
            Debug.Log("[PocketGarden] Placeholder icons generated in Resources/UIButtons.");
        }

        // --- Icon builders ---------------------------------------------------

        private static Texture2D BuildEnergy()
        {
            var tex = NewTex();
            var teal = new Color(0.20f, 0.70f, 0.55f);
            // Lightning bolt polygon (normalized, y-up)
            var pts = Norm(new[]
            {
                (0.55f,0.95f),(0.30f,0.50f),(0.47f,0.50f),(0.40f,0.05f),(0.72f,0.55f),(0.53f,0.55f)
            });
            FillPolygon(tex, pts, teal);
            Outline(tex, teal * 0.7f);
            tex.Apply();
            return tex;
        }

        private static Texture2D BuildCoin()
        {
            var tex = NewTex();
            var gold = new Color(0.95f, 0.78f, 0.20f);
            var goldDark = new Color(0.80f, 0.60f, 0.10f);
            FillCircle(tex, S * 0.5f, S * 0.5f, S * 0.42f, goldDark);
            FillCircle(tex, S * 0.5f, S * 0.5f, S * 0.34f, gold);
            tex.Apply();
            return tex;
        }

        private static Texture2D BuildGem()
        {
            var tex = NewTex();
            var gem = new Color(0.26f, 0.62f, 0.86f);
            var gemLight = new Color(0.55f, 0.80f, 0.95f);
            var pts = Norm(new[] { (0.50f,0.92f),(0.92f,0.58f),(0.50f,0.10f),(0.08f,0.58f) });
            FillPolygon(tex, pts, gem);
            // top facet highlight
            var facet = Norm(new[] { (0.50f,0.92f),(0.92f,0.58f),(0.50f,0.58f) });
            FillPolygon(tex, facet, gemLight);
            tex.Apply();
            return tex;
        }

        private static Texture2D BuildShop()
        {
            var tex = NewTex();
            var green = new Color(0.30f, 0.62f, 0.30f);
            // basket body (trapezoid, wider at top)
            var body = Norm(new[] { (0.22f,0.62f),(0.78f,0.62f),(0.68f,0.18f),(0.32f,0.18f) });
            FillPolygon(tex, body, green);
            // handle (arch) — ring segment approximated by two circles difference
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = x - S * 0.5f, dy = y - S * 0.62f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dy > 0 && d < S * 0.28f && d > S * 0.20f)
                        tex.SetPixel(x, y, green);
                }
            tex.Apply();
            return tex;
        }

        private static Texture2D BuildSettings()
        {
            var tex = NewTex();
            var gray = new Color(0.55f, 0.55f, 0.55f);
            float cx = S * 0.5f, cy = S * 0.5f;
            FillCircle(tex, cx, cy, S * 0.34f, gray);
            // 8 teeth
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                float tx = cx + Mathf.Cos(a) * S * 0.40f;
                float ty = cy + Mathf.Sin(a) * S * 0.40f;
                FillCircle(tex, tx, ty, S * 0.09f, gray);
            }
            // center hole
            FillCircle(tex, cx, cy, S * 0.13f, new Color(0, 0, 0, 0), true);
            tex.Apply();
            return tex;
        }

        // --- Pixel helpers ---------------------------------------------------

        private static Texture2D NewTex()
        {
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var clear = new Color(0, 0, 0, 0);
            var px = new Color[S * S];
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            tex.SetPixels(px);
            return tex;
        }

        private static Vector2[] Norm((float x, float y)[] pts)
        {
            var r = new Vector2[pts.Length];
            for (int i = 0; i < pts.Length; i++) r[i] = new Vector2(pts[i].x * S, pts[i].y * S);
            return r;
        }

        private static void FillCircle(Texture2D tex, float cx, float cy, float radius, Color color, bool erase = false)
        {
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                        tex.SetPixel(x, y, color);
                }
        }

        private static void FillPolygon(Texture2D tex, Vector2[] poly, Color color)
        {
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                    if (PointInPoly(x + 0.5f, y + 0.5f, poly))
                        tex.SetPixel(x, y, color);
        }

        private static bool PointInPoly(float px, float py, Vector2[] poly)
        {
            bool inside = false;
            int n = poly.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((poly[i].y > py) != (poly[j].y > py)) &&
                    (px < (poly[j].x - poly[i].x) * (py - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                    inside = !inside;
            }
            return inside;
        }

        private static void Outline(Texture2D tex, Color color)
        {
            // simple 1px darker rim where an opaque pixel borders a transparent one
            var copy = tex.GetPixels();
            for (int y = 1; y < S - 1; y++)
                for (int x = 1; x < S - 1; x++)
                {
                    if (copy[y * S + x].a < 0.5f) continue;
                    if (copy[y * S + x + 1].a < 0.5f || copy[y * S + x - 1].a < 0.5f ||
                        copy[(y + 1) * S + x].a < 0.5f || copy[(y - 1) * S + x].a < 0.5f)
                        tex.SetPixel(x, y, color);
                }
        }

        private static void Save(string name, Texture2D tex)
        {
            string path = $"{Folder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                imp.filterMode = FilterMode.Bilinear;
                var settings = new TextureImporterSettings();
                imp.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                imp.SetTextureSettings(settings);
                imp.SaveAndReimport();
            }
            Object.DestroyImmediate(tex);
        }
    }
}
