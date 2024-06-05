using BoardCoordinateSystem;
using UnityEngine;

namespace BlockStackGridLibrary
{
    [RequireComponent(typeof(LevelCell))]
    public class LevelCellEditor : MonoBehaviour
    {
        public LevelCell Cell => GetComponent<LevelCell>();
        public LevelCellData levelCellData;
        public Coordinate coordinate => Cell.coordinate;

        public void Start()
        {
            Destroy(this);
        }

#if UNITY_EDITOR

        [ContextMenu("Update Slot")]
        public void UpdateSlot()
        {
            Cell.SetCell(levelCellData);
        }

        [ContextMenu("Reset Slot")]
        public void ResetSlot()
        {
            levelCellData = new LevelCellData();
            UpdateSlot();
        }

        public void LoadCellData(LevelCellData data)
        {
            levelCellData = data.Clone();
            UpdateSlot();
        }
        

        [ContextMenu("Copy Component")]
        public void CopyComponentToClipboard()
        {
            GUIUtility.systemCopyBuffer = JsonUtility.ToJson(levelCellData);
        }

        [ContextMenu("Paste Component")]
        public void PasteComponentFromClipboard()
        {
            levelCellData = JsonUtility.FromJson<LevelCellData>(GUIUtility.systemCopyBuffer);
            UpdateSlot();
        }

        [ContextMenu("Fix Name")]
        public void FixName()
        {
            gameObject.name = "Cell_" + transform.GetSiblingIndex();
            UnityEditor.EditorUtility.SetDirty(this);
        }


#endif
    }
}