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
            foreach (var file in Directory.GetFiles(Source, "*.png"))
            {
                string fileName = Path.GetFileName(file);
                string dest = $"{DestDir}/{fileName}";

                // Copy the PNG into Resources and let Unity create/refresh its asset entry.
                File.Copy(file, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);

                // Read the real pixel dimensions from the freshly imported texture.
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dest);
                int maxDim = tex != null ? Mathf.Max(tex.width, tex.height) : 256;

                var imp = (TextureImporter)AssetImporter.GetAtPath(dest);
                imp.textureType        = TextureImporterType.Sprite;
                imp.spriteImportMode   = SpriteImportMode.Single;            // fix: NULL load in Multiple mode
                imp.spritePixelsPerUnit = maxDim / TargetWorldSize;          // fix: unified size across rows
                imp.spritePivot        = new Vector2(0.5f, 0.5f);
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled      = false;
                imp.filterMode         = FilterMode.Bilinear;
                imp.SaveAndReimport();
                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ItemSpriteImporter] Imported {count} item sprites into {DestDir} " +
                      $"(Single mode, PPU normalized to {TargetWorldSize} world unit).");
        }
    }
}
