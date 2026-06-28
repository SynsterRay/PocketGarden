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
        private bool _hasSprite;

        // Producer state (Option A)
        private MergeGrid _grid;
        private SpriteRenderer _indicator;
        private float _produceTimer;
        private bool _produceReady;

        public bool IsProducerReady => _produceReady;

        public void Init(MergeItemData data, GridCell cell)
        {
            Data = data;
            Cell = cell;
            cell.Item = this;
            transform.position = cell.transform.position;

            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = 10;

            // Load real art from Resources/Items (imported via "PocketGarden → Import Item Sprites").
            // PPU is normalized per file, so every sprite is the same world size at localScale = 1 —
            // that keeps all rows consistent and lets the merge/drag punch animations work untouched.
            ApplyVisual(data);

            // Level label (only shown when there is no sprite — fallback prototype mode).
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
            UpdateLabel();

            if (_grid == null) _grid = GetComponentInParent<MergeGrid>();
            RefreshProducer();
        }

        public void UpdateVisual(MergeItemData newData)
        {
            Data = newData;
            ApplyVisual(newData);
            UpdateLabel();

            // Re-evaluate producer state (e.g. just merged into a Tree)
            RefreshProducer();

            // Punch scale feedback (base scale is 1 because PPU is normalized per sprite)
            transform.localScale = Vector3.one * 1.3f;
        }

        /// <summary>Loads the item's sprite from Resources/Items, falling back to a tinted square.</summary>
        private void ApplyVisual(MergeItemData data)
        {
            var sprite = LoadItemSprite(data.id);
            if (sprite != null)
            {
                _renderer.sprite = sprite;
                _renderer.color = Color.white;
                _hasSprite = true;
            }
            else
            {
                _renderer.sprite = CreateSquareSprite();
                _renderer.color = data.fallbackColor;
                _hasSprite = false;
            }
        }

        /// <summary>Shows the name label only as a fallback when there is no sprite art.</summary>
        private void UpdateLabel()
        {
            if (_label == null) return;
            _label.text = _hasSprite ? "" : Data.displayName;
            _label.gameObject.SetActive(!_hasSprite);
        }

        // Maps a merge-item id to its PNG filename in Resources/Items.
        private static readonly System.Collections.Generic.Dictionary<string, string> FileMap = new()
        {
            { "garden_1", "seed" },
            { "garden_2", "sprout" },
            { "garden_3", "flower" },
            { "garden_4", "bush" },
            { "garden_5", "three" },
            { "garden_6", "big_three" },
            { "garden_7", "magical_three" },
        };

        private static Sprite LoadItemSprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!FileMap.TryGetValue(id, out var file)) return null;
            return Resources.Load<Sprite>($"Items/{file}");
        }

        private void Update()
        {
            // Smooth scale back to 1
            if (transform.localScale.x > 1.01f)
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 8f);

            // Producer cooldown tick (only while the produced chain is unlocked)
            if (Data != null && Data.IsProducer && !_produceReady && ProducerChainUnlocked())
            {
                _produceTimer -= Time.deltaTime;
                if (_produceTimer <= 0f)
                {
                    _produceReady = true;
                    if (_indicator != null) _indicator.enabled = true;
                }
            }
        }

        private bool ProducerChainUnlocked()
        {
            return Data != null && Data.IsProducer
                && PocketGarden.Core.Progression.IsChainUnlocked(
                    PocketGarden.Core.Progression.ChainOf(Data.producesItemId));
        }

        /// <summary>Sets up / tears down producer state based on current Data.</summary>
        private void RefreshProducer()
        {
            if (Data != null && Data.IsProducer)
            {
                EnsureIndicator();
                _produceTimer = Data.produceCooldown;
                _produceReady = false;
                if (_indicator != null) _indicator.enabled = false;
            }
            else
            {
                _produceReady = false;
                if (_indicator != null) _indicator.enabled = false;
            }
        }

        private void EnsureIndicator()
        {
            if (_indicator != null) return;
            var go = new GameObject("ProduceIndicator");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0.3f, 0.3f, -0.1f);
            go.transform.localScale = Vector3.one * 0.2f;
            _indicator = go.AddComponent<SpriteRenderer>();
            _indicator.sprite = CreateSquareSprite_Static();
            _indicator.color = Color.yellow;
            _indicator.sortingOrder = 30;
            _indicator.enabled = false;
        }

        /// <summary>Tap a ready producer to spawn its output on an empty cell. Returns true if produced.</summary>
        public bool TryProduce()
        {
            if (Data == null || !Data.IsProducer || !_produceReady) return false;
            if (!ProducerChainUnlocked()) return false;
            if (_grid == null) _grid = GetComponentInParent<MergeGrid>();
            if (_grid == null) return false;

            var emptyCell = _grid.FindEmptyCell();
            if (emptyCell == null) return false;

            _grid.SpawnItem(Data.producesItemId, emptyCell.Row, emptyCell.Col);
            _produceReady = false;
            _produceTimer = Data.produceCooldown;
            if (_indicator != null) _indicator.enabled = false;
            return true;
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
