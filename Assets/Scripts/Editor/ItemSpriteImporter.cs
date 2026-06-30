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
    ///   • <see cref="SpriteImportMode.Single"/> — a Multiple-mode sprite is stored as a named
    ///     sub-asset, so <c>Resources.Load&lt;Sprite&gt;("Items/seed")</c> returns NULL. Single
    ///     mode makes the sprite the main asset, so runtime loading works.
    ///   • <b>CONTENT-box Pixels-Per-Unit</b> — PPU is normalized by the sprite's *opaque content*
    ///     bounding box (alpha trimmed), not the full canvas. The Garden art is cropped tight
    ///     (content fills 100% of the canvas) while Wood/Stone art has big transparent margins
    ///     (content ≈ 55–69% of the canvas). Normalizing by the canvas made Garden render much
    ///     larger than Wood/Stone. Normalizing by the *content* box makes every item's visible
    ///     size identical at <c>localScale = 1</c>, regardless of how the PNG was cropped.
    ///
    /// Animation frame folders are normalized by the folder's LARGEST content box (one PPU for the
    /// whole folder) so frames keep their relative growth (seed → sprout grows on screen) and the
    /// final frame matches the canonical static sprite — no size pop when the merge settles.
    /// </summary>
    public static class ItemSpriteImporter
    {
        private const string Source  = @"C:\Users\mbaja\source\pocket_garden_assets";
        private const string DestDir = "Assets/Resources/Items";

        /// <summary>Visible world-space size (max content dimension) every item should occupy.
        /// The grid cell is 1.1 units; 0.72 keeps a clear margin and matches the old Wood/Stone
        /// footprint, shrinking the over-sized Garden art down to the same level.</summary>
        private const float TargetContentWorldSize = 0.72f;

        /// <summary>Alpha threshold (0–255) for what counts as "content" when trimming.</summary>
        private const byte AlphaThreshold = 10;

        [MenuItem("PocketGarden/Import Item Sprites")]
        public static void Import()
        {
            if (!Directory.Exists(Source))
            {
                Debug.LogError($"[ItemSpriteImporter] Source folder not found: {Source}");
                return;
            }

            Directory.CreateDirectory(DestDir);

            // 1) Copy the external item art PNGs into Resources/Items (top level).
            foreach (var file in Directory.GetFiles(Source, "*.png"))
            {
                string dest = $"{DestDir}/{Path.GetFileName(file)}";
                File.Copy(file, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
            }

            // 2) Measure the opaque content box of every PNG and group by folder so a whole
            //    animation folder shares one PPU (keeps relative frame scale + matches statics).
            var diskPaths = Directory.GetFiles(DestDir, "*.png", SearchOption.AllDirectories);
            var contentDim = new Dictionary<string, int>();       // diskPath -> content max dim
            var folderMax  = new Dictionary<string, int>();        // folder   -> max content dim

            foreach (var disk in diskPaths)
            {
                int dim = MeasureContentMaxDim(disk);
                contentDim[disk] = dim;
                string folder = Path.GetDirectoryName(disk);
                folderMax[folder] = folderMax.TryGetValue(folder, out var cur) ? Mathf.Max(cur, dim) : dim;
            }

            // 3) Configure each sprite with a content-normalized PPU.
            int count = 0;
            foreach (var disk in diskPaths)
            {
                string assetPath = disk.Replace('\\', '/');
                int idx = assetPath.IndexOf("Assets/");
                if (idx > 0) assetPath = assetPath.Substring(idx);

                // Top-level statics normalize to their own content; folder frames share folderMax.
                string folder = Path.GetDirectoryName(disk);
                bool isTopLevel = string.Equals(
                    Path.GetFullPath(folder), Path.GetFullPath(DestDir),
                    System.StringComparison.OrdinalIgnoreCase);
                int normDim = isTopLevel ? contentDim[disk] : folderMax[folder];
                if (normDim <= 0) normDim = 256;

                if (ConfigureSprite(assetPath, normDim / TargetContentWorldSize)) count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ItemSpriteImporter] Configured {count} sprites (Single mode, " +
                      $"content-box normalized to {TargetContentWorldSize} world units).");
        }

        /// <summary>Decodes the PNG off disk and returns the max dimension of its opaque
        /// (alpha-trimmed) content box, in pixels. Falls back to the full texture size.</summary>
        private static int MeasureContentMaxDim(string diskPath)
        {
            byte[] bytes;
            try { bytes = File.ReadAllBytes(diskPath); }
            catch { return 0; }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, bytes))
            {
                Object.DestroyImmediate(tex);
                return 0;
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

            if (maxX < 0) return Mathf.Max(w, h);            // fully transparent
            return Mathf.Max(maxX - minX + 1, maxY - minY + 1);
        }

        /// <summary>Imports a PNG as a Single sprite at the given pixels-per-unit. Returns true on success.</summary>
        private static bool ConfigureSprite(string assetPath, float ppu)
        {
            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) return false;

            imp.textureType         = TextureImporterType.Sprite;
            imp.spriteImportMode    = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = Mathf.Max(1f, ppu);
            imp.spritePivot         = new Vector2(0.5f, 0.5f);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled       = false;
            imp.filterMode          = FilterMode.Bilinear;
            imp.SaveAndReimport();
            return true;
        }
    }
}
