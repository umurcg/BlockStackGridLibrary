using System;
using Sirenix.OdinInspector;

namespace BlockStackGridLibrary
{
    [Serializable]public class LevelCellData
    {
        public CellTypes cellType;
        [ShowIf(nameof(cellType) , CellTypes.Slot)] public BlockStackData blockStackData;

        [ShowIf(nameof(cellType), CellTypes.Slot)]
        public bool isCovered;
        
        
        public LevelCellData Clone()
        {
            var newCellData = new LevelCellData();
            newCellData.cellType = cellType;
            if (cellType == CellTypes.Slot)
            {
                newCellData.blockStackData = blockStackData;
                newCellData.isCovered = isCovered;
            }
            
        
            return newCellData;
        }

        public bool Compare(LevelCellData levelCellData)
        {
            if (cellType != levelCellData.cellType) return false;
            if (cellType == CellTypes.Slot)
            {
                if (!blockStackData.Compare(levelCellData.blockStackData)) return false;
                if (isCovered != levelCellData.isCovered) return false;
            }
   

            return true;
        }
    }
}