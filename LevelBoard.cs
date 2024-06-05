using System;
using System.Collections;
using System.Collections.Generic;
using BoardCoordinateSystem;
using Helpers;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlockStackGridLibrary
{
    public class LevelBoard : Singleton<LevelBoard>
    {
        public int rowCount = 5;
        public int columnCount = 5;
        [SerializeField]private CoordinatePositionUtility coordinatePositionUtility;

        public LevelCell[] LevelCells { get; private set;}
        Dictionary<Coordinate, LevelCell> LevelCellsCoordinateDictionary = new Dictionary<Coordinate, LevelCell>();
    
        public Action OnBoardInitialized;

        // Start is called before the first frame update
        private void Start()
        {
            GlobalActions.OnNewLevelLoaded += OnNewLevelLoaded;
        }

        private void OnNewLevelLoaded()
        {
            StartCoroutine(InitializeBoard());
        }
    
        private IEnumerator InitializeBoard()
        {
            var activeLevel=LevelManager.Instance.ActiveLevelData as LevelData;
            SetBoard(activeLevel);
        
            yield return null;
        
            LevelCells = GetComponentsInChildren<LevelCell>();
            LevelCellsCoordinateDictionary.Clear();
            foreach (var cell in LevelCells)
            {
                LevelCellsCoordinateDictionary[cell.coordinate] = cell;
            }
        
            OnBoardInitialized?.Invoke();
        }

    
        public Vector3 ConvertCoordinateToPosition(Coordinate coordinate)
        {
            return coordinatePositionUtility.ConvertCoordinateToPosition(coordinate,transform,rowCount,columnCount);
        }

        public Coordinate ConvertPositionToCoordinate(Vector3 worldPosition)
        {
            return coordinatePositionUtility.ConvertPositionToCoordinate(worldPosition,transform,rowCount,columnCount);
        }
    
        public LevelCell GetCell(Coordinate coordinate)
        {
            return LevelCellsCoordinateDictionary.GetValueOrDefault(coordinate);
        }

        public bool IsValidCoordinate(Coordinate coordinate)
        {
            return coordinate.row >= 0 && coordinate.row < rowCount &&
                   coordinate.column >= 0 && coordinate.column < columnCount;
        }
    
        private bool IsValidPathCoordinate(Coordinate coordinate, HashSet<Coordinate> visitedCoordinates)
        {
            return IsValidCoordinate(coordinate) &&
                   LevelCellsCoordinateDictionary.ContainsKey(coordinate) &&
                   LevelCellsCoordinateDictionary[coordinate].IsEmpty() &&
                   !visitedCoordinates.Contains(coordinate);
        }

        [Button]
        public void UpdateBoard()
        {
            ClearBoard();
        
            int numberOfCells = rowCount * columnCount;
            for (int i = 0; i < numberOfCells; i++)
            {
                var coordinate = ConvertIndexToCoordinate(i);
                var cell = LevelCell.Spawn();
                cell.transform.parent = transform;
                cell.coordinate = coordinate;
                cell.SetPositionToCoordinate();
            }
        }
    
        public void UpdateBoardPartial()
        {
            int numberOfCells = rowCount * columnCount;
            var currentCells=GetComponentsInChildren<LevelCell>();
            List<Coordinate> existingCoordinates = new List<Coordinate>();
            foreach (var cell in currentCells)
            {
                existingCoordinates.Add(cell.coordinate);
                cell.SetPositionToCoordinate();
            }
        
            for (int i = 0; i < numberOfCells; i++)
            {
                var coordinate = ConvertIndexToCoordinate(i);
                if (!existingCoordinates.Contains(coordinate))
                {
                    var cell = LevelCell.Spawn();
                    cell.transform.parent = transform;
                    cell.coordinate = coordinate;
                    cell.SetPositionToCoordinate();
                    cell.transform.SetSiblingIndex(ConvertCoordinateToIndex(coordinate));
                }
            }
        }
    

        public Coordinate ConvertIndexToCoordinate(int index)
        {
            return new Coordinate(index / columnCount, index % columnCount);
        }
    
        public int ConvertCoordinateToIndex(Coordinate coordinate)
        {
            return coordinate.row * columnCount + coordinate.column;
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    var position = ConvertCoordinateToPosition(new Coordinate(i, j));
                    Gizmos.DrawWireCube(position,
                        new Vector3(coordinatePositionUtility.cellSize.x, .1f, coordinatePositionUtility.cellSize.y));
                }
            }
        }

        public int GetCellCount()
        {
            return rowCount * columnCount;
        }

        public void ClearBoard()
        {
            var cells = GetComponentsInChildren<LevelCell>();
            foreach (var cell in cells)
            {
                cell.Destroy();
            }
        }

        public void SetBoard(LevelData level)
        {
            rowCount = level.boardSize.x;
            columnCount = level.boardSize.y;
            UpdateBoard();

            var position = transform.position;
            position.x = level.xOffset;
            transform.position = position;

#if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

        }
    
    

        public void AddColumn()
        {
            columnCount++;
            UpdateBoardPartial();
        }


        public void AddRow()
        {
            rowCount++;
            UpdateBoardPartial();
        }
    
        public void RemoveColumn()
        {
            columnCount--;
            UpdateBoardPartial();
        }
    
        public void RemoveRow()
        {
            rowCount--;
            UpdateBoardPartial();
        }

        public int CountBlocks()
        {
            int count = 0;
            if(LevelCells==null) return count;
            foreach (var cell in LevelCells)
            {
                if (!cell.IsEmpty())
                {
                    count += cell.slot.BlockCount;
                }
            }
        
            return count;
        }
    }
}