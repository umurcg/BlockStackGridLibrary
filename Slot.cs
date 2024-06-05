using System;
using System.Collections.Generic;
using System.Linq;
using ObjectType;
using UnityEngine;
using UnityEngine.Events;

namespace BlockStackGridLibrary
{
    public class Slot : MonoBehaviour
    {
        private List<Block> _spawnedBlocks = new List<Block>();
        private float GapBetweenBricks => BlockParameters.Find().gapBetweenBricks;
        public bool IsEmpty => _spawnedBlocks.Count == 0;
        public int BlockCount => _spawnedBlocks.Count;
        public float SlotHeight => BlockCount * GapBetweenBricks;
        
        public Action<int> OnBlockCountChanged;
        [SerializeField] public Transform blockParent;

        public UnityEvent<int> OnBlockTransferStarted;
        public UnityEvent<int> OnBlockTransferEnded;
        public Action OnCleared;

        public bool IsLocked { get; private set; }
        
        protected Vector3 _originalParentPosition;

        protected void Awake()
        {
            if (blockParent == null)
            {
                blockParent = transform;
            }

            _originalParentPosition = blockParent.localPosition;
        }

        protected void Start()
        {
            ReAssignBlocks();
        }

        public void SetLock(bool locked)
        {
            IsLocked = locked;
        }
        
        public Block[] GetBlocks()
        {
            return _spawnedBlocks.ToArray();
        }
        
        public void ReAssignBlocks()
        {
            _spawnedBlocks=GetComponentsInChildren<Block>().ToList();
        }


        public void AddBlockDirect(Block money)
        {
            var aimPosition = new Vector3(0, _spawnedBlocks.Count * GapBetweenBricks, 0);
            money.transform.SetParent(blockParent);
            money.transform.localPosition = aimPosition;
            money.FixRotation();
            _spawnedBlocks.Add(money);
            OnBlockCountChanged?.Invoke(_spawnedBlocks.Count);
        }

        public void TransferBlock(Block block, BlockMovementDefinition definition)
        {
            block.MoveToSlot(this, definition, 0, 1, OnBlockTransferStarted.Invoke, OnBlockTransferEnded.Invoke);
            _spawnedBlocks.Add(block);
            OnBlockCountChanged?.Invoke(_spawnedBlocks.Count);
        }
        
        
        public void TransferBlockStack(Block[] blockStack, BlockMovementDefinition definition)
        {
            for (int index = 0; index < blockStack.Length; index++)
            {
                var block = blockStack[index];
                block.MoveToSlot(this, definition, index, blockStack.Length, OnBlockTransferStarted.Invoke,
                    OnBlockTransferEnded.Invoke);
                _spawnedBlocks.Add(block);
                OnBlockCountChanged?.Invoke(_spawnedBlocks.Count);
            }
        }
        
        public void SpawnStack(BlockStackData stackData)
        {
            if(stackData.subStacks==null) return;
            foreach (var subStack in stackData.subStacks)
            {
                SpawnSubStack(subStack);
            }
        }

        private void SpawnSubStack(BlockSubStack subStack)
        {
            for (int i = 0; i < subStack.numberOfStack; i++)
            {
                var type = subStack.type;
                var block = Block.Spawn(type.typeName);
                if (block == null)
                {
                    Debug.LogError("Block not found");
                    return;
                }
                block.transform.SetParent(blockParent);
                block.MoveTo(Vector3.zero);
                
                block.transform.localPosition = new Vector3(0, _spawnedBlocks.Count * GapBetweenBricks, 0);
                _spawnedBlocks.Add(block);
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void DestroyAndClearBlockStack()
        {
            //Clear all null blocks
            for (int i = _spawnedBlocks.Count - 1; i >= 0; i--)
            {
                if (_spawnedBlocks[i] == null)
                {
                    _spawnedBlocks.RemoveAt(i);
                }
            }
            
            foreach (Block block in _spawnedBlocks)
                block.Destroy();
            ClearBlocks();
        }

        public void ClearBlocks()
        {
            _spawnedBlocks.Clear();
            OnBlockCountChanged?.Invoke(_spawnedBlocks.Count);
            OnCleared?.Invoke();
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public Block GetTopBrick()
        {
            if (_spawnedBlocks.Count == 0)
            {
                return null;
            }

            return _spawnedBlocks[^1];
        }

        public List<Block> GetTopTypeBlocks()
        {
            if (_spawnedBlocks.Count == 0)
            {
                Debug.LogError("There is no brick in the slot");
                return null;
            }

            List<Block> topBricks = new List<Block>();
            var typeName = _spawnedBlocks[^1].TypeName;

            for (int i = _spawnedBlocks.Count - 1; i >= 0; i--)
            {
                if (_spawnedBlocks[i].TypeName == typeName)
                {
                    topBricks.Add(_spawnedBlocks[i]);
                }
                else
                {
                    break;
                }
            }

            return topBricks;
        }

        public void TransferBlocks(List<Block> bricks)
        {
            foreach (Block brick in bricks)
            {
                if (!this._spawnedBlocks.Contains(brick))
                {
                    Debug.LogError("Brick is not in the slot");
                    return;
                }

                _spawnedBlocks.Remove(brick);
            }

            OnBlockCountChanged?.Invoke(this._spawnedBlocks.Count);
            if (_spawnedBlocks.Count == 0)
            {
                OnCleared?.Invoke();
            }
        }

        public int CountType(string type)
        {
            int count = 0;
            foreach (Block brick in _spawnedBlocks)
            {
                if (brick.TypeName == type)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetNumberOfType()
        {
            List<string> types = new List<string>();

            foreach (Block brick in _spawnedBlocks)
            {
                if (!types.Contains(brick.TypeName))
                {
                    types.Add(brick.TypeName);
                }
            }

            return types.Count;
        }

        public bool IsStatic()
        {
            foreach (Block brick in _spawnedBlocks)
            {
                if (brick.IsMoving)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsAnyBlockMoving()
        {
            foreach (Block block in _spawnedBlocks)
            {
                if (block.IsMoving)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsLastBlock(Block block)
        {
            if (_spawnedBlocks.Count == 0) return false;
            return _spawnedBlocks[^1] == block;
        }


        public List<string> GetTypes()
        {
            List<string> types = new List<string>();
            for (int i = _spawnedBlocks.Count - 1; i >= 0; i--)
            {
                if (types.Count == 0)
                {
                    types.Add(_spawnedBlocks[i].TypeName);
                    continue;
                }

                var lastType = types[^1];
                if (lastType != _spawnedBlocks[i].TypeName)
                {
                    types.Add(_spawnedBlocks[i].TypeName);
                }
            }

            return types;
        }

        public BlockSubStack[] GetSubStackData()
        {
            List<string> blockTypes = new List<string>();
            List<int> moneyCounts = new List<int>();

            foreach (Block money in _spawnedBlocks)
            {
                if (blockTypes.Count == 0 || blockTypes[^1] != money.TypeName)
                {
                    blockTypes.Add(money.TypeName);
                    moneyCounts.Add(1);
                }
                else
                {
                    moneyCounts[^1]++;
                }
            }

            var subStacks = new BlockSubStack[blockTypes.Count];
            for (int i = 0; i < blockTypes.Count; i++)
            {
                subStacks[i] = new BlockSubStack();
                subStacks[i].type = ObjectTypeEnum.GetEnum(blockTypes[i]);
                subStacks[i].numberOfStack = moneyCounts[i];
            }

            return subStacks;
        }

        public void FixParentPosition()
        {
            blockParent.localPosition = _originalParentPosition;
        }

        public int GetBlockIndex(Block block)
        {
            return _spawnedBlocks.IndexOf(block);
        }
    }
}