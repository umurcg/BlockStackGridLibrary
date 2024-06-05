using System;
using System.Collections;
using DG.Tweening;
using ObjectType;
using UnityEngine;
using UnityEngine.Events;

namespace BlockStackGridLibrary
{
    public class Block : ObjectTypeController
    {
        
        public bool IsMoving{get; private set;}
        
        public UnityEvent OnMovementStarted;
        public UnityEvent OnMovementCompleted;

        [SerializeField] private float yScale = .3f;
        
        public void MoveTo(Vector3 aimPosition)
        {
            var aimScale=new Vector3(1, yScale ,1);
            transform.localPosition = aimPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = aimScale;
        }
        
        
        public void MoveToSlot(Slot slot,BlockMovementDefinition definition, int index, int totalMoneyCount, Action<int> onStarted, Action<int> onEnded)
        {
            StartCoroutine(MoveToSlotCoroutine(slot,definition, index, totalMoneyCount, onStarted, onEnded));
        }
        
        private IEnumerator MoveToSlotCoroutine(Slot slot,BlockMovementDefinition definition, int moveIndex, int totalMoneyCount,
            Action<int> onStarted, Action<int> onEnded)
        {
            
            transform.SetParent(slot.blockParent);
            
            IsMoving = true;
            float duration = definition.tweenDuration;
            Ease ease = definition.tweenEase;

            float slotDelay = definition.tweenDelayPerBlock;
            float totalDelay = totalMoneyCount * slotDelay;
            if (totalDelay > BlockParameters.Find().maximumCumulativeDelayCap)
                slotDelay = BlockParameters.Find().maximumCumulativeDelayCap / totalMoneyCount;

            float delay = moveIndex * slotDelay;

            yield return new WaitForSeconds(delay);

            int indexOnSlot = Array.IndexOf(slot.GetBlocks(), this);
            if (indexOnSlot == -1)
            {
                Debug.LogError("Money is not in the slot");
                yield break;
            }

            Vector3 aimLocalPosition = new Vector3(0, indexOnSlot * BlockParameters.Find().gapBetweenBricks, 0);
            Tween tween = null;

            if (definition.jump)
            {
                tween = transform.DOLocalJump(aimLocalPosition, 1f, 1, duration).SetEase(ease);
            }
            else 
            {
                tween = transform.DOLocalMove(aimLocalPosition, duration).SetEase(ease);
            }

            // transform.DOScale(Vector3.one,definition.scaleDuration).SetEase(ease);
             
            if (onStarted != null) tween.OnStart(() =>
            {
                
                onStarted.Invoke(moveIndex);
                
                if(definition.movementType==MovementType.Default)
                    OnMovementCompleted?.Invoke();
            });

            tween.OnComplete(() =>
            {
                if (onEnded != null) onEnded.Invoke(moveIndex);
                IsMoving = false;
                
  
                OnMovementStarted?.Invoke();
            });


            if (definition.shouldFlip)
            {
                FlipRotation(aimLocalPosition, duration, ease);
            }
            else
            {
                float deltaRotationY = transform.localRotation.eulerAngles.y;
                if (deltaRotationY < 0) deltaRotationY += 360;

                if (deltaRotationY < 45 || deltaRotationY > 225)
                {
                    transform.DOLocalRotate(Vector3.zero, duration).SetEase(ease);
                }
                else
                {
                    transform.DOLocalRotate(Vector3.up * 180, duration).SetEase(ease);
                }
            }
        }
        
        private void FlipRotation(Vector3 aimPosition, float tweenDuration, Ease ease)
        {
            var selfToAim = aimPosition - transform.localPosition;

            Vector3 rotationEuler = Vector3.zero;
            bool horizontalMovement = Mathf.Abs(selfToAim.x) > Mathf.Abs(selfToAim.z);

            if (Mathf.Abs(selfToAim.x) - Mathf.Abs(selfToAim.z) < 0.05f) horizontalMovement = false;


            if (horizontalMovement)
            {
                rotationEuler = selfToAim.x < 0 ? Vector3.forward : Vector3.back;
            }
            else
            {
                rotationEuler = selfToAim.z > 0 ? Vector3.right : Vector3.left;
            }

            rotationEuler *= 180;
            transform.DOLocalRotate(rotationEuler, tweenDuration, RotateMode.WorldAxisAdd)
                .SetEase(ease).OnComplete(FixRotation);
        }

        
        public void FixRotation()
        {
            transform.localRotation = Quaternion.identity;
        }

        public override void Destroy()
        {
            DOTween.Kill(transform);
            base.Destroy();
        }
        
        public static Block Spawn(string type)
        {
            var objectType = ObjectTypeController.Spawn(type, 0);
            if (objectType == null) return null;
            var block= objectType.GetComponent<Block>();
            return block;
        }
    }
}