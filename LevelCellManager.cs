using System;
using Helpers;
using Managers;
using UnityEngine;

namespace BlockStackGridLibrary
{
    public class LevelCellManager : Singleton<LevelCellManager>
    {
        private LevelBoard levelBoard;
        public Action OnSlotsInitialized;

        public void Start()
        {
            levelBoard = LevelBoard.Instance;
            levelBoard.OnBoardInitialized += OnBoardInitialized;
        }

        private void OnBoardInitialized()
        {
            InitializeSlots();   
        }
        
        private void ClearSlots()
        {
            var cells = levelBoard.LevelCells;
            foreach (var slot in cells)
            {
                slot.Reset();
            }
        }

        private void InitializeSlots()
        {
            ClearSlots();
            var activeLevel = LevelManager.Instance.ActiveLevelData as LevelData;

            var cellData = activeLevel.cellData;
            var cells = levelBoard.LevelCells;
            if (cells.Length != cellData.Length)
            {
                Debug.LogError("Slot count is not equal to the level data count "+cells.Length+" "+cellData.Length);
            }
            
            for (int i = 0; i < cellData.Length; i++)
            {
                cells[i].SetCell(cellData[i]);
            }
            
            OnSlotsInitialized?.Invoke();
        }
    }
}