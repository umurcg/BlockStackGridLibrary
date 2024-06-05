using System;
using BoardCoordinateSystem;
using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine;
using IPoolable = Lean.Pool.IPoolable;

namespace BlockStackGridLibrary
{
    //Select this object when any child collider is clicked
    [SelectionBase]
    public class LevelCell : MonoBehaviour, IPoolable
    {
        public Coordinate coordinate;
        [HideInInspector] public CellTypes cellType;

        public Slot slot { get; private set; }

        private bool IsInitialized => slot != null;

        public CellTypeObject[] cellTypeObjects;

        public bool Pooled;

        public Action OnCellSet;
        public Action OnCellDespawned;

        private void Initialize()
        {
            slot = GetComponentInChildren<Slot>(true);
         
        }

        private void Start()
        {
            Initialize();
        }

        public bool IsEmpty()
        {
            if (cellType == CellTypes.Empty) return true;
            if (cellType == CellTypes.Slot)
            {
                return slot.IsEmpty;
            }

            return false;
        }

        public void Reset()
        {
            if (!IsInitialized) Initialize();
            if (cellType == CellTypes.Slot)
            {
                slot.DestroyAndClearBlockStack();
            }

            cellType = CellTypes.Empty;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void UpdateTypeObjects()
        {
            if (!IsInitialized) Initialize();
            foreach (var cellTypeObject in cellTypeObjects)
                if (cellTypeObject.Object) cellTypeObject.Object.SetActive(cellTypeObject.Type == cellType);
        }

        public void SetCell(LevelCellData cellData)
        {
            if (!IsInitialized) Initialize();
            
            cellType = cellData.cellType;
            UpdateTypeObjects();

            if (slot.BlockCount > 0)
                slot.DestroyAndClearBlockStack();

            if (cellType == CellTypes.Slot)
            {
                slot.SpawnStack(cellData.blockStackData);
            }
      
            if(Application.isPlaying)
                OnCellSet?.Invoke();
        }

        

        public void Destroy()
        {
            if (Pooled)
            {
                LeanPool.Despawn(gameObject);
            }
            else
            {
                if(Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        public static LevelCell Spawn()
        {
            var cellPrefab = GameData.Find().cellPrefab;
            if (!cellPrefab)
            {
                Debug.LogError("Cell prefab is not found");
                return null;
            }

            LevelCell spawnedCell;

            if (Application.isPlaying)
            {
                spawnedCell = LeanPool.Spawn(cellPrefab).GetComponent<LevelCell>();
                spawnedCell.Pooled = true;
            }
            else
            {
#if UNITY_EDITOR
                spawnedCell = UnityEditor.PrefabUtility.InstantiatePrefab(cellPrefab).GetComponent<LevelCell>();
#else
                spawnedCell = Instantiate(cellPrefab).GetComponent<LevelCell>();
#endif
            }
            
            return spawnedCell;
        }

        public void SetPositionToCoordinate()
        {
            LevelBoard board = FindObjectOfType<LevelBoard>();
            if (board)
            {
                transform.position = board.ConvertCoordinateToPosition(coordinate);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void OnSpawn()
        {
            
        }

        public void OnDespawn()
        {
            OnCellDespawned?.Invoke();
        }
    }
}