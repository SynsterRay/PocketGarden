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

            // If this is already a Magic Tree (e.g. loaded from save), run its idle loop.
            MaybeStartIdle(data);
        }

        public void UpdateVisual(MergeItemData newData)
        {
            Data = newData;
            RefreshProducer();

            // Garden merges (sprout → … → magic tree) play a frame-by-frame growth animation
            // instead of an instant swap. The folder is chosen by the merge RESULT id.
            if (TransitionFolder.TryGetValue(newData.id, out var folder))
            {
                var frames = LoadFrames(folder);
                if (frames != null) { PlayTransition(frames, newData); return; }
            }

            ApplyVisual(newData);
            UpdateLabel();
            MaybeStartIdle(newData);

            // Punch scale feedback (base scale is 1 because PPU is normalized per sprite)
            transform.localScale = Vector3.one * 1.3f;
        }

        // --- Garden growth animations ---------------------------------------

        private const float FrameTime = 0.06f; // seconds per transition frame
        private Coroutine _animRoutine;
        private static readonly System.Collections.Generic.Dictionary<string, Sprite[]> _frameCache = new();

        // Merge RESULT id → frames folder under Resources/Items (top row first, left→right).
        private static readonly System.Collections.Generic.Dictionary<string, string> TransitionFolder = new()
        {
            { "garden_2", "seed_sprout" },
            { "garden_3", "sprout_flower" },
            { "garden_4", "flower_bush" },
            { "garden_5", "bush_three" },
            { "garden_6", "three_big_three" },
            { "garden_7", "three_magical_three" },
            // Wood chain (Dead Tree onward).
            { "wood_2", "twig_three_to_log" },
            { "wood_3", "log_to_plank" },
            { "wood_4", "plank_to_crate" },
            { "wood_5", "crate_to_furniture" },
            { "wood_6", "furniture_to_gazebo" },
            { "wood_7", "gazebo_to_cottage_house" },
            // Stone chain.
            { "stone_2", "pebble_to_stone" },
            { "stone_3", "stone_to_brick" },
            { "stone_4", "brick_to_wall" },
            { "stone_5", "wall_to_pillar" },
            { "stone_6", "pillar_to_fountain" },
            { "stone_7", "fountain_to_castle" },
        };

        // Looping idle animation played once an item reaches its final form (Magic Tree).
        private const string MagicIdleFolder = "magical_three_animation";
        private const string CastleIdleFolder = "castle_animation";

        /// <summary>Loads frame_0..N from a Resources/Items subfolder. Cached; null if none found.</summary>
        private static Sprite[] LoadFrames(string folder)
        {
            if (_frameCache.TryGetValue(folder, out var cached)) return cached;
            var list = new System.Collections.Generic.List<Sprite>();
            for (int i = 0; ; i++)
            {
                var s = Resources.Load<Sprite>($"Items/{folder}/frame_{i}");
                if (s == null) break;
                list.Add(s);
            }
            var arr = list.Count > 0 ? list.ToArray() : null;
            _frameCache[folder] = arr;
            return arr;
        }

        private void PlayTransition(Sprite[] frames, MergeItemData data)
        {
            if (_label != null) { _label.text = ""; _label.gameObject.SetActive(false); }
            _hasSprite = true;
            _renderer.color = Color.white;

            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(TransitionRoutine(frames, data));
        }

        private System.Collections.IEnumerator TransitionRoutine(Sprite[] frames, MergeItemData data)
        {
            transform.localScale = Vector3.one; // hold steady while frames play
            for (int i = 0; i < frames.Length; i++)
            {
                _renderer.sprite = frames[i];
                yield return new WaitForSeconds(FrameTime);
            }
            // Settle on the canonical sprite so it matches other items of this level, with a punch.
            ApplyVisual(data);
            UpdateLabel();
            transform.localScale = Vector3.one * 1.2f;
            _animRoutine = null;

            // Magic Tree keeps a gentle looping idle animation afterwards.
            MaybeStartIdle(data);
        }

        /// <summary>Starts the looping Magic Tree idle animation for final-form items.</summary>
        private void MaybeStartIdle(MergeItemData data)
        {
            if (data == null) return;
            string folder = null;
            if (data.id == "garden_7") folder = MagicIdleFolder;
            else if (data.id == "stone_7") folder = CastleIdleFolder;
            if (folder == null) return;

            var frames = LoadFrames(folder);
            if (frames == null) return;

            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _hasSprite = true;
            _renderer.color = Color.white;
            if (_label != null) { _label.text = ""; _label.gameObject.SetActive(false); }
            _animRoutine = StartCoroutine(IdleLoop(frames));
        }

        private System.Collections.IEnumerator IdleLoop(Sprite[] frames)
        {
            int i = 0;
            while (true)
            {
                _renderer.sprite = frames[i];
                i = (i + 1) % frames.Length;
                yield return new WaitForSeconds(FrameTime * 2f); // slower, calmer idle
            }
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
            // Wood chain. wood_1 (Dead Tree) uses the twig_three art.
            { "wood_1", "twig_three" },
            { "wood_2", "log" },
            { "wood_3", "plank" },
            { "wood_4", "crate" },
            { "wood_5", "furniture" },
            { "wood_6", "gazebo" },
            { "wood_7", "cottage_house" },
            // Stone chain.
            { "stone_1", "pebble" },
            { "stone_2", "stone" },
            { "stone_3", "brick" },
            { "stone_4", "wall" },
            { "stone_5", "pillar" },
            { "stone_6", "fountain" },
            { "stone_7", "castle" },
        };

        private static Sprite LoadItemSprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!FileMap.TryGetValue(id, out var file)) return null;
            return Resources.Load<Sprite>($"Items/{file}");
        }

        // Idle breath phase offset (unique per item instance, avoids synchronized breathing).
        private float _breathPhase;

        private void Awake()
        {
            _breathPhase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            // Smooth scale back to 1 after merge punch
            if (transform.localScale.x > 1.01f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 8f);
            }
            else
            {
                // 🟢 Idle bounce — subtle breathing (±2% scaleY, phased per item)
                float breath = 1f + Mathf.Sin(Time.time * 1.8f + _breathPhase) * 0.018f;
                transform.localScale = new Vector3(1f, breath, 1f);
            }

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
