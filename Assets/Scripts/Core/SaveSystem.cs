using UnityEngine;
using System.Collections.Generic;
using PocketGarden.Grid;

namespace PocketGarden.Core
{
    [System.Serializable]
    public class GridSaveData
    {
        public List<CellSave> cells = new();
    }

    [System.Serializable]
    public class CellSave
    {
        public int row;
        public int col;
        public string itemId;
    }

    public static class SaveSystem
    {
        private const string GridKey = "PG_GridState";

        public static void SaveGrid(MergeGrid grid)
        {
            var data = new GridSaveData();
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Cols; c++)
                {
                    var cell = grid.Cells[r, c];
                    if (!cell.IsEmpty)
                    {
                        data.cells.Add(new CellSave
                        {
                            row = r,
                            col = c,
                            itemId = cell.Item.Data.id
                        });
                    }
                }
            }
            PlayerPrefs.SetString(GridKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static GridSaveData LoadGrid()
        {
            string json = PlayerPrefs.GetString(GridKey, "");
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<GridSaveData>(json);
        }

        public static bool HasSave() => !string.IsNullOrEmpty(PlayerPrefs.GetString(GridKey, ""));
    }
}
