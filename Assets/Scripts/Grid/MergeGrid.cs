using UnityEngine;
using System.Collections.Generic;
using PocketGarden.Core;

namespace PocketGarden.Grid
{
    public class MergeGrid : MonoBehaviour
    {
        [SerializeField] private int rows = 7;
        [SerializeField] private int cols = 5;
        [SerializeField] private float cellSize = 1.1f;

        private GridCell[,] _cells;
        private MergeDatabase _db;

        public int Rows => rows;
        public int Cols => cols;
        public GridCell[,] Cells => _cells;

        public static event System.Action<MergeItemData> OnMergeCompleted;
        public static event System.Action OnBoardFull;

        private List<Generator> _generators = new();

        private void Start()
        {
            _db = CreatePrototypeDB();
            BuildGrid();

            if (SaveSystem.HasSave())
                LoadFromSave();
            else
                SpawnStarterItems();

            SpawnGenerators();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveSystem.SaveGrid(this);
        }

        private void OnApplicationQuit()
        {
            SaveSystem.SaveGrid(this);
        }

        private void LoadFromSave()
        {
            var data = SaveSystem.LoadGrid();
            if (data == null) { SpawnStarterItems(); return; }
            foreach (var cell in data.cells)
                SpawnItem(cell.itemId, cell.row, cell.col);
        }

        private void SpawnGenerators()
        {
            // Only spawn generators for chains the player has unlocked.
            // Garden is always available; Wood/Stone unlock via quest progression.
            TrySpawnGenerator(MergeChain.Garden);
            TrySpawnGenerator(MergeChain.Wood);
            TrySpawnGenerator(MergeChain.Stone);

            // React to chains unlocked later (during play) by dropping in their generator.
            Progression.OnChainUnlocked += OnChainUnlocked;
        }

        private void OnDestroy()
        {
            Progression.OnChainUnlocked -= OnChainUnlocked;
        }

        private void OnChainUnlocked(MergeChain chain) => TrySpawnGenerator(chain);

        private void TrySpawnGenerator(MergeChain chain)
        {
            if (!Progression.IsChainUnlocked(chain)) return;

            // Avoid duplicates if called again after a runtime unlock.
            string itemId = chain switch
            {
                MergeChain.Wood => "wood_1",
                MergeChain.Stone => "stone_1",
                _ => "garden_1"
            };
            foreach (var g in _generators)
                if (g != null && g.name == $"Gen_{itemId}") return;

            var emptyCell = FindEmptyCell();
            if (emptyCell == null) return; // No space for generator

            AddGenerator(emptyCell.Row, emptyCell.Col, itemId,
                Progression.GeneratorCooldown(chain),
                Progression.GeneratorUses(chain));
        }

        private void AddGenerator(int row, int col, string itemId, float cooldown, int uses)
        {
            var cell = _cells[row, col];
            var go = new GameObject($"Gen_{itemId}");
            go.transform.SetParent(transform);
            go.transform.position = cell.transform.position;

            // Visual: darker cell
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MergeGridItem.CreateSquareSprite_Static();
            sr.color = new Color(0.5f, 0.7f, 0.4f, 0.5f);
            sr.sortingOrder = 1;

            var gen = go.AddComponent<Generator>();
            gen.Init(cell, this, itemId, cooldown, uses);
            _generators.Add(gen);

            // Collider for tapping
            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one * cellSize * 0.95f;
        }

        public List<Generator> Generators => _generators;

        private MergeDatabase CreatePrototypeDB()
        {
            var db = ScriptableObject.CreateInstance<MergeDatabase>();
            db.items = MergeDatabase.CreateDefaults();
            db.Init();
            return db;
        }

        // Screen fractions reserved by UI (so the board never hides behind them):
        //   top    → HudBar sits at 0.925–1.0; reserve to ~0.90 (small margin).
        //   bottom → QuestCard (0.095–0.205) + DropZone (0.015–0.085); reserve to ~0.23.
        private const float TopReserveFrac = 0.90f;
        private const float BottomReserveFrac = 0.23f;

        private void BuildGrid()
        {
            _cells = new GridCell[rows, cols];

            // Vertically center the grid inside the visible play band (between HUD and quests),
            // instead of centering on the whole screen, so the bottom rows don't hide behind UI.
            var cam = UnityEngine.Camera.main;
            float halfH = cam != null ? cam.orthographicSize : 5.5f;
            float bandTop = halfH * (2f * TopReserveFrac - 1f);
            float bandBottom = halfH * (2f * BottomReserveFrac - 1f);
            float bandCenterY = (bandTop + bandBottom) * 0.5f;

            float startX = -(cols - 1) * cellSize * 0.5f;
            float startY = bandCenterY + (rows - 1) * cellSize * 0.5f; // top row, centered in band

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var go = new GameObject($"Cell_{r}_{c}");
                    go.transform.SetParent(transform);
                    go.transform.position = new Vector3(startX + c * cellSize, startY - r * cellSize, 0);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = MergeGridItem.CreateSquareSprite_Static();
                    sr.sortingOrder = 0;
                    sr.drawMode = SpriteDrawMode.Simple;

                    var cell = go.AddComponent<GridCell>();
                    cell.Init(r, c);
                    _cells[r, c] = cell;

                    // Collider for raycasting
                    var box = go.AddComponent<BoxCollider2D>();
                    box.size = Vector2.one * cellSize * 0.95f;
                }
            }
        }

        private void SpawnStarterItems()
        {
            // Front-loaded Garden start so the player can merge immediately and feel progress.
            // Wood/Stone are locked at first, so we don't seed items the player can't yet sustain.
            SpawnItem("garden_2", 0, 0); // a couple of Sprouts for a near-instant first merge
            SpawnItem("garden_2", 0, 1);
            SpawnItem("garden_1", 1, 0);
            SpawnItem("garden_1", 1, 1);
            SpawnItem("garden_1", 1, 2);
            SpawnItem("garden_1", 2, 0);
        }

        public MergeGridItem SpawnItem(string itemId, int row, int col)
        {
            if (row < 0 || row >= rows || col < 0 || col >= cols) return null;
            var cell = _cells[row, col];
            if (!cell.IsEmpty) return null;

            var data = _db.Get(itemId);
            if (data == null) return null;

            var go = new GameObject($"Item_{data.displayName}");
            go.transform.SetParent(transform);
            var item = go.AddComponent<MergeGridItem>();
            item.Init(data, cell);
            return item;
        }

        public bool TryMerge(MergeGridItem dragged, GridCell targetCell)
        {
            if (targetCell.IsEmpty)
            {
                // Move to empty cell
                MoveItem(dragged, targetCell);
                return true;
            }

            var target = targetCell.Item;
            if (target == dragged) return false;

            if (_db.CanMerge(dragged.Data, target.Data))
            {
                // Merge!
                var result = _db.GetMergeResult(dragged.Data);
                dragged.Cell.Item = null;
                Destroy(dragged.gameObject);
                target.UpdateVisual(result);
                OnMergeCompleted?.Invoke(result);
                EnergySystem.Spend(1);
                return true;
            }

            return false;
        }

        public void MoveItem(MergeGridItem item, GridCell newCell)
        {
            item.Cell.Item = null;
            item.Cell = newCell;
            newCell.Item = item;
            item.transform.position = newCell.transform.position;
        }

        public GridCell GetCellAt(Vector3 worldPos)
        {
            float minDist = float.MaxValue;
            GridCell closest = null;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float dist = Vector2.Distance(worldPos, _cells[r, c].transform.position);
                    if (dist < minDist && dist < cellSize * 0.6f)
                    {
                        minDist = dist;
                        closest = _cells[r, c];
                    }
                }
            }
            return closest;
        }

        public GridCell FindEmptyCell()
        {
            var empty = new System.Collections.Generic.List<GridCell>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (_cells[r, c].IsEmpty) empty.Add(_cells[r, c]);
            
            if (empty.Count == 0) { OnBoardFull?.Invoke(); return null; }
            return empty[UnityEngine.Random.Range(0, empty.Count)];
        }

        public MergeDatabase Database => _db;
    }
}
