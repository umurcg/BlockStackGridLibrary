using System;
using DG.Tweening;

namespace BlockStackGridLibrary
{
    [Serializable]public struct BlockMovementDefinition
    {
        public MovementType movementType;
        public bool shouldFlip;
        public float tweenDuration;
        public float tweenDelayPerBlock;
        public Ease tweenEase;
        public bool jump;
    }
}