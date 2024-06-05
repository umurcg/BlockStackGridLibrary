using System;
using Puzzle;
using Sirenix.OdinInspector;
using BlockStackGridLibrary;
using UnityEngine;

namespace BlockStackGridLibrary
{
    [CreateAssetMenu(fileName = "Level", menuName = "MoneyRectSort/LevelData", order = 0)]
    [Serializable]public class LevelData: PuzzleLevelData
    {
        public Vector2Int boardSize;
        public LevelCellData[] cellData;
        public float xOffset;
        
#if UNITY_EDITOR
        [Button]
        public void LoadLevel()
        {
            var levelEditor= FindObjectOfType<LevelCreator>();
            if (levelEditor == null)
            {
                Debug.LogError("Level editor is not found");
                return;
            }
            
            levelEditor.level = this;
            UnityEditor.EditorUtility.SetDirty(levelEditor);
            levelEditor.LoadLevel();
        }
        
#endif
    }
}