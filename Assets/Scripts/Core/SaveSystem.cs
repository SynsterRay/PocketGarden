using UnityEngine;
using System.Collections.Generic;
using PocketGarden.Grid;

namespace PocketGarden.Core
{
    [System.Serializable]
    public class GridSaveData
    {
        public int saveVersion = CurrentVersion;
        public List<CellSave> cells = new();

        /// <summary>
        /// Bump this when quest ladder / item IDs change.
        /// The migrator will handle upgrades from older versions.
        /// </summary>
        public const int CurrentVersion = 2;
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
            data.saveVersion = GridSaveData.CurrentVersion;
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

            var data = JsonUtility.FromJson<GridSaveData>(json);
            if (data == null) return null;

            // Migrate from older versions.
            data = Migrate(data);
            return data;
        }

        public static bool HasSave() => !string.IsNullOrEmpty(PlayerPrefs.GetString(GridKey, ""));

        /// <summary>
        /// Applies sequential migrations so old saves don't break when item IDs or
        /// the quest ladder change. Each version bump gets a migration step.
        /// </summary>
        private static GridSaveData Migrate(GridSaveData data)
        {
            // Version 0/1 → 2: "twig" items renamed to "wood_1" (Dead Tree).
            // Old saves might still have "twig_1" or similar — map them.
            if (data.saveVersion < 2)
            {
                foreach (var cell in data.cells)
                {
                    if (cell.itemId == "twig_1" || cell.itemId == "twig")
                        cell.itemId = "wood_1";
                }
                data.saveVersion = 2;
            }

            // Future migrations go here:
            // if (data.saveVersion < 3) { ... data.saveVersion = 3; }

            return data;
        }
    }
}
