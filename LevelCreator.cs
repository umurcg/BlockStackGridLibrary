using System.Collections.Generic;
using BoardCoordinateSystem;
using UnityEditor;
using UnityEngine;

namespace BlockStackGridLibrary
{
    [RequireComponent(typeof(LevelBoard))]
    public class LevelCreator : MonoBehaviour
    {
        void Start()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
        }

#if UNITY_EDITOR
        public LevelData level;


        public void LoadLevel()
        {
            if (level == null)
            {
                return;
            }

        
            ClearBoard();

            var levelBoard = GetComponent<LevelBoard>();
            if (levelBoard == null)
            {
                Debug.LogError("Level board is null");
                return;
            }

            levelBoard.SetBoard(level);


            var slotEditors = GetComponentsInChildren<LevelCellEditor>();
            LevelCellData[] slotData = level.cellData;
            if (level.cellData.Length != slotEditors.Length)
            {
                Debug.LogError("Slot count is not equal to the level data");
                return;
            }

            for (int i = 0; i < slotEditors.Length; i++)
            {
                slotEditors[i].LoadCellData(slotData[i]);
            }
        }


        public void SaveLevel()
        {
            if (level == null)
            {
                Debug.LogError("Level is null");
                return;
            }

            LevelBoard levelBoard = GetComponent<LevelBoard>();
            if (levelBoard == null)
            {
                Debug.LogError("Level board is null");
                return;
            }


            var slotEditors = GetComponentsInChildren<LevelCellEditor>();
            int boardCellCount = levelBoard.GetCellCount();
            if (slotEditors.Length != boardCellCount)
            {
                Debug.LogError("Board cell count is not equal to the level data count");
                return;
            }

            LevelCellData[] slotData = new LevelCellData[boardCellCount];

            for (int i = 0; i < slotEditors.Length; i++)
            {
                var coordinate = slotEditors[i].coordinate;
                int index = levelBoard.ConvertCoordinateToIndex(coordinate);
                slotData[index] = slotEditors[i].levelCellData.Clone();
            }

            level.cellData = slotData;
            level.boardSize = new Vector2Int(levelBoard.rowCount, levelBoard.columnCount);
            UnityEditor.EditorUtility.SetDirty(level);
            //Save level
            AssetDatabase.SaveAssets();
        }


        public void ClearBoard()
        {
            var slotEditors = GetComponentsInChildren<LevelCellEditor>();
            foreach (var slotEditor in slotEditors)
            {
                slotEditor.levelCellData = new LevelCellData();
                slotEditor.UpdateSlot();
            }

            var levelBoard = GetComponent<LevelBoard>();
            if (levelBoard)
            {
                levelBoard.ClearBoard();
            }
        }


        //Adds a column to the board
        [ContextMenu("Add Column")]
        public void AddColumn()
        {
            var levelBoard = GetComponent<LevelBoard>();
            if (levelBoard)
            {
                levelBoard.AddColumn();
            }
        }

        //Adds a row to the board
        [ContextMenu("Add Row")]
        public void AddRow()
        {
            var levelBoard = GetComponent<LevelBoard>();
            if (levelBoard)
            {
                levelBoard.AddRow();
            }
        }
        
        
        public void ShiftLevel(int rowShift, int columShift)
        {
            var cells = GetComponentsInChildren<LevelCellEditor>();
            Dictionary<Coordinate, LevelCellData> newCoordinateDictionary = new Dictionary<Coordinate, LevelCellData>();

            foreach (LevelCellEditor cell in cells)
            {
                var coordinate = cell.coordinate;
                var newCoordinate = new Coordinate(coordinate.row + rowShift, coordinate.column + columShift);
                newCoordinateDictionary.Add(newCoordinate, cell.levelCellData);
            }

            foreach (LevelCellEditor cell in cells)
            {
                var coordinate = cell.coordinate;
                if (!newCoordinateDictionary.ContainsKey(coordinate)) cell.levelCellData = new LevelCellData();
                else cell.levelCellData = newCoordinateDictionary[coordinate].Clone();
                cell.UpdateSlot();
            }
        }

        public void SetAllCellTypes(CellTypes cellType)
        {
            foreach (var slotEditor in GetComponentsInChildren<LevelCellEditor>())
            {
                slotEditor.levelCellData.cellType = cellType;
                slotEditor.UpdateSlot();
            }
        }

        public bool IsDirty()
        {
            if (level == null) return true;

            //Compare level data
            var levelData = level.cellData;
            var slotEditors = GetComponentsInChildren<LevelCellEditor>();
            if (levelData.Length != slotEditors.Length) return true;

            for (int i = 0; i < levelData.Length; i++)
            {
                if (!levelData[i].Compare(slotEditors[i].levelCellData)) return true;
            }

            return false;
        }

        public void PlayLevel()
        {
            if (level)
            {
                level.PlayLevel();
            }
        }

#endif
    }
}


