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
    /// Two settings are critical (they were the source of earlier bugs):
    ///   • <see cref="SpriteImportMode.Single"/> — a Multiple-mode sprite is stored as a named
    ///     sub-asset (e.g. "seed_0"), so <c>Resources.Load&lt;Sprite&gt;("Items/seed")</c> returns
    ///     NULL. Single mode makes the sprite the main asset, so runtime loading works.
    ///   • <b>Per-file Pixels-Per-Unit</b> = maxPixelDimension / <see cref="TargetWorldSize"/>.
    ///     With a fixed PPU, higher-resolution art renders larger in world space (that is why the
    ///     first row looked oversized). Normalizing PPU per file makes every item the same on-screen
    ///     size at <c>localScale = 1</c>, so the merge/drag scale animations never distort it.
    /// </summary>
    public static class ItemSpriteImporter
    {
        private const string Source  = @"C:\Users\mbaja\source\pocket_garden_assets";
        private const string DestDir = "Assets/Resources/Items";

        /// <summary>World-space size (max dimension) every item sprite should occupy. The grid cell
        /// is 1.1 units, so 1.0 leaves a small margin and keeps all rows visually consistent.</summary>
        private const float TargetWorldSize = 1.0f;

        [MenuItem("PocketGarden/Import Item Sprites")]
        public static void Import()
        {
            if (!Directory.Exists(Source))
            {
                Debug.LogError($"[ItemSpriteImporter] Source folder not found: {Source}");
                return;
            }

            Directory.CreateDirectory(DestDir);

            int count = 0;

            // 1) Copy the external item art PNGs into Resources/Items (top level) and configure them.
            foreach (var file in Directory.GetFiles(Source, "*.png"))
            {
                string fileName = Path.GetFileName(file);
                string dest = $"{DestDir}/{fileName}";
                File.Copy(file, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                if (ConfigureSprite(dest)) count++;
            }

            // 2) Configure every PNG already under Resources/Items (incl. subfolders such as
            //    seed_sprout/ animation frames) so they load as Single sprites at a unified size.
            foreach (var dest in Directory.GetFiles(DestDir, "*.png", SearchOption.AllDirectories))
            {
                string assetPath = dest.Replace('\\', '/');
                int idx = assetPath.IndexOf("Assets/");
                if (idx > 0) assetPath = assetPath.Substring(idx);
                if (ConfigureSprite(assetPath)) count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ItemSpriteImporter] Configured {count} item sprites under {DestDir} " +
                      $"(Single mode, PPU normalized to {TargetWorldSize} world unit).");
        }

        /// <summary>Imports a single PNG as a properly sized Single sprite. Returns true on success.</summary>
        private static bool ConfigureSprite(string assetPath)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            int maxDim = tex != null ? Mathf.Max(tex.width, tex.height) : 256;

            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) return false;

            imp.textureType         = TextureImporterType.Sprite;
            imp.spriteImportMode    = SpriteImportMode.Single;          // fix: NULL load in Multiple mode
            imp.spritePixelsPerUnit = maxDim / TargetWorldSize;         // fix: unified size across rows
            imp.spritePivot         = new Vector2(0.5f, 0.5f);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled       = false;
            imp.filterMode          = FilterMode.Bilinear;
            imp.SaveAndReimport();
            return true;
        }
    }
}
