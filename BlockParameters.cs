using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlockStackGridLibrary
{
    [CreateAssetMenu(fileName = "BlockParameters", menuName = "HexJam/BlockParameters", order = 0)]
    public class BlockParameters: ScriptableObject
    {
        public BlockMovementDefinition[] movementDefinitions;
        
        private static BlockParameters Cached;
        
        public float gapBetweenBricks = .05f;
        public float maximumCumulativeDelayCap = 2f;
        
        public static BlockParameters Find()
        {
            if (Cached == null)
            {
                Cached = Resources.Load<BlockParameters>("BlockParameters");
                if (Cached == null)
                {
                    Debug.LogError("BlockParameters not found in Resources");
                }
            }

            return Cached;
        }
        
        [Button]
        public void LogJson()
        {
            Debug.Log(JsonUtility.ToJson(this));
        }

        public BlockMovementDefinition GetMovementDefinition(MovementType spawner)
        {
            foreach (var definition in movementDefinitions)
            {
                if (definition.movementType == spawner)
                {
                    return definition;
                }
            }

            return new BlockMovementDefinition();
        }
    }
}