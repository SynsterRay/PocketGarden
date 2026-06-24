using UnityEngine;

namespace PocketGarden.Grid
{
    public class GridCell : MonoBehaviour
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public MergeGridItem Item { get; set; }

        public bool IsEmpty => Item == null;

        private SpriteRenderer _bg;

        public void Init(int row, int col)
        {
            Row = row;
            Col = col;
            _bg = GetComponent<SpriteRenderer>();
            if (_bg != null)
                _bg.color = (row + col) % 2 == 0
                    ? new Color(0.85f, 0.95f, 0.75f)
                    : new Color(0.80f, 0.92f, 0.70f);
        }

        public void SetHighlight(bool on)
        {
            if (_bg != null)
                _bg.color = on
                    ? new Color(0.7f, 1f, 0.5f)
                    : ((Row + Col) % 2 == 0
                        ? new Color(0.85f, 0.95f, 0.75f)
                        : new Color(0.80f, 0.92f, 0.70f));
        }
    }
}
