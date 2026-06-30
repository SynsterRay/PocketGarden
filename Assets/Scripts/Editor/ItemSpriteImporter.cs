using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace PocketGarden.Editor
{
    /// <summary>
    /// Copies the item art PNGs from the external asset folder into Resources/Items and imports
    /// each one as a properly configured <b>Single</b> sprite. Run:
    /// <b>PocketGarden → Import Item Sprites</b>.
    ///
    /// Critical settings (each fixed a real bug):
    ///   • <see cref="SpriteImportMode.Single"/> — a Multiple-mode sprite returns NULL on load.
    ///   • <b>CONTENT-box PPU</b> — normalized by the sprite's opaque content bounding box so
    ///     every item renders at the same visible size regardless of canvas margins.
    ///   • <b>Content-center pivot</b> — the sprite pivot is placed at the center of the opaque
    ///     content (not canvas center) so the item's visible art sits at the cell center.
    ///     Animation folders share one average pivot so frames don't jitter.
    /// </summary>
    public static class ItemSpriteImporter
    {
        private const string Source  = @"C:\Users\mbaja\source\pocket_garden_assets";
        private const string DestDir = "Assets/Resources/Items";
        private const float TargetContentWorldSize = 0.72f;
        private const byte AlphaThreshold = 10;

        private struct SpriteInfo
        {
            public int contentMaxDim;
            public float pivotX; // Unity pivot: 0-1, bottom-left origin
            public float pivotY;
        }

        [MenuItem("PocketGarden/Import Item Sprites")]
        public static void Import()
        {
            if (!Directory.Exists(Source))
            {
                Debug.LogError($"[ItemSpriteImporter] Source folder not found: {Source}");
                return;
            }

            Directory.CreateDirectory(DestDir);

            foreach (var file in Directory.GetFiles(Source, "*.png"))
            {
                string dest = $"{DestDir}/{Path.GetFileName(file)}";
                File.Copy(file, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
            }

            // Measure content box + pivot for every sprite.
            var diskPaths = Directory.GetFiles(DestDir, "*.png", SearchOption.AllDirectories);
            var info = new Dictionary<string, SpriteInfo>();
            var folderMaxDim = new Dictionary<string, int>();
            var folderPivotSum = new Dictionary<string, (float sx, float sy, int n)>();

            foreach (var disk in diskPaths)
            {
                var si = MeasureContent(disk);
                info[disk] = si;
                string folder = Path.GetDirectoryName(disk);
                folderMaxDim[folder] = folderMaxDim.TryGetValue(folder, out var cur)
                    ? Mathf.Max(cur, si.contentMaxDim) : si.contentMaxDim;
                if (!folderPivotSum.ContainsKey(folder))
                    folderPivotSum[folder] = (0f, 0f, 0);
                var (sx, sy, n) = folderPivotSum[folder];
                folderPivotSum[folder] = (sx + si.pivotX, sy + si.pivotY, n + 1);
            }

            int count = 0;
            foreach (var disk in diskPaths)
            {
                string assetPath = disk.Replace('\\', '/');
                int idx = assetPath.IndexOf("Assets/");
                if (idx > 0) assetPath = assetPath.Substring(idx);

                string folder = Path.GetDirectoryName(disk);
                bool isTopLevel = string.Equals(
                    Path.GetFullPath(folder), Path.GetFullPath(DestDir),
                    System.StringComparison.OrdinalIgnoreCase);

                int normDim = isTopLevel ? info[disk].contentMaxDim : folderMaxDim[folder];
                if (normDim <= 0) normDim = 256;
                float ppu = normDim / TargetContentWorldSize;

                float px, py;
                if (isTopLevel)
                {
                    px = info[disk].pivotX;
                    py = info[disk].pivotY;
                }
                else
                {
                    var (sx, sy, n) = folderPivotSum[folder];
                    px = sx / n;
                    py = sy / n;
                }

                if (ConfigureSprite(assetPath, ppu, new Vector2(px, py))) count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ItemSpriteImporter] Configured {count} sprites (Single mode, " +
                      $"content-box normalized to {TargetContentWorldSize} world units, content-centered pivot).");
        }

        private static SpriteInfo MeasureContent(string diskPath)
        {
            byte[] bytes;
            try { bytes = File.ReadAllBytes(diskPath); }
            catch { return new SpriteInfo { contentMaxDim = 0, pivotX = 0.5f, pivotY = 0.5f }; }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, bytes))
            {
                Object.DestroyImmediate(tex);
                return new SpriteInfo { contentMaxDim = 0, pivotX = 0.5f, pivotY = 0.5f };
            }

            int w = tex.width, h = tex.height;
            var px = tex.GetPixels32();
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    if (px[row + x].a > AlphaThreshold)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }
            Object.DestroyImmediate(tex);

            if (maxX < 0)
                return new SpriteInfo { contentMaxDim = Mathf.Max(w, h), pivotX = 0.5f, pivotY = 0.5f };

            int dim = Mathf.Max(maxX - minX + 1, maxY - minY + 1);
            // Content center in pixel coords (Unity texture: y=0 is bottom).
            // GetPixels32 returns bottom-to-top, so minY/maxY are already in Unity coords.
            float cx = (minX + maxX) / 2f;
            float cy = (minY + maxY) / 2f;
            return new SpriteInfo
            {
                contentMaxDim = dim,
                pivotX = cx / w,
                pivotY = cy / h
            };
        }

        private static bool ConfigureSprite(string assetPath, float ppu, Vector2 pivot)
        {
            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) return false;

            imp.textureType         = TextureImporterType.Sprite;
            imp.spriteImportMode    = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = Mathf.Max(1f, ppu);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled       = false;
            imp.filterMode          = FilterMode.Bilinear;

            // A custom spritePivot is only honored when alignment == Custom (9); otherwise Unity
            // uses the canvas center and items with off-center content render displaced.
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot     = pivot;
            imp.SetTextureSettings(settings);

            imp.SaveAndReimport();
            return true;
        }
    }
}
