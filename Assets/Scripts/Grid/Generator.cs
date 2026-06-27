using UnityEngine;

namespace PocketGarden.Grid
{
    public class Generator : MonoBehaviour
    {
        [SerializeField] private string producesItemId = "garden_1";
        [SerializeField] private float cooldown = 30f;
        [SerializeField] private int maxUses = 20;

        private float _timer;
        private int _usesLeft;
        private bool _ready;
        private MergeGrid _grid;
        private GridCell _cell;
        private SpriteRenderer _indicator;
        private float _moveTimer;
        private const float MoveInterval = 15f; // przemieszaj generator co 15 sekund

        public bool IsReady => _ready;
        public int UsesLeft => _usesLeft;

        /// <summary>True if the generator is on cooldown (not ready) but still has uses — eligible for a gem skip.</summary>
        public bool CanSkip => !_ready && _usesLeft > 0;

        /// <summary>Instantly finish the current cooldown (used by the gem skip).</summary>
        public void SkipCooldown()
        {
            if (_usesLeft <= 0) return;
            _timer = 0f;
            _ready = true;
            if (_indicator != null) _indicator.enabled = true;
        }

        public void Init(GridCell cell, MergeGrid grid, string itemId, float cd, int uses)
        {
            _cell = cell;
            _grid = grid;
            producesItemId = itemId;
            cooldown = cd;
            maxUses = uses;
            _usesLeft = uses;
            _timer = cooldown;

            // Visual indicator (pulsing dot)
            var go = new GameObject("Indicator");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0.3f, 0.3f, -0.1f);
            _indicator = go.AddComponent<SpriteRenderer>();
            _indicator.sprite = MergeGridItem.CreateSquareSprite_Static();
            _indicator.color = Color.yellow;
            _indicator.sortingOrder = 15;
            go.transform.localScale = Vector3.one * 0.2f;
            _indicator.enabled = false;
        }

        private void Update()
        {
            if (_usesLeft <= 0) return;
            if (_ready) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _ready = true;
                if (_indicator != null) _indicator.enabled = true;
            }

            // Periodically move to a random empty cell
            _moveTimer += Time.deltaTime;
            if (_moveTimer >= MoveInterval)
            {
                _moveTimer = 0f;
                MoveToRandomCell();
            }
        }

        private void MoveToRandomCell()
        {
            var newCell = _grid.FindEmptyCell();
            if (newCell == null) return;

            _cell.SetHighlight(false); // unhighlight old cell
            _cell = newCell;
            transform.position = _cell.transform.position;
            _cell.SetHighlight(true); // highlight new cell
        }

        /// <summary>Tap generator to produce item on nearest empty cell.</summary>
        public bool TryProduce()
        {
            if (!_ready || _usesLeft <= 0) return false;

            var emptyCell = _grid.FindEmptyCell();
            if (emptyCell == null) return false;

            _grid.SpawnItem(producesItemId, emptyCell.Row, emptyCell.Col);
            _usesLeft--;
            _ready = false;
            _timer = cooldown;
            if (_indicator != null) _indicator.enabled = false;
            return true;
        }
    }
}
