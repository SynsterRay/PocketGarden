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
            // Garden generator at cell [0,4]
            AddGenerator(0, 4, "garden_1", 30f, 50);
            // Wood generator at cell [3,4] (unlocked after quest 3)
            AddGenerator(3, 4, "wood_1", 45f, 40);
            // Stone generator at cell [6,4]
            AddGenerator(6, 4, "stone_1", 60f, 30);
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

        private void BuildGrid()
        {
            _cells = new GridCell[rows, cols];
            float startX = -(cols - 1) * cellSize * 0.5f;
            float startY = (rows - 1) * cellSize * 0.5f - 1f; // offset down for UI

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
            // Spawn a few starter items for prototype
            SpawnItem("garden_1", 0, 0);
            SpawnItem("garden_1", 0, 1);
            SpawnItem("garden_1", 1, 0);
            SpawnItem("wood_1", 2, 2);
            SpawnItem("wood_1", 3, 2);
            SpawnItem("stone_1", 4, 4);
            SpawnItem("stone_1", 5, 4);
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
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (_cells[r, c].IsEmpty) return _cells[r, c];
            OnBoardFull?.Invoke();
            return null;
        }

        public MergeDatabase Database => _db;
    }
}
