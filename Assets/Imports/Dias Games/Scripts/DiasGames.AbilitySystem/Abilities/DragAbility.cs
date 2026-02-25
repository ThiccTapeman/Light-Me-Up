using DiasGames.AbilitySystem.Core;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "DragAbility", menuName = "Dias Games/Abilities/DragAbility", order = 0)]
    public class DragAbility : Ability
    {
        private const float LerpDuration = 0.15f;

        [SerializeField] private string _dragAnimState = "Push Blend";
        [SerializeField] private float _maxMoveSpeed = 1.4f;
        [SerializeField] private string _charRelativeHorParam = "CharRelativeHorizontal";
        [SerializeField] private string _charRelativeVerParam = "CharRelativeVertical";

        private IDraggable _draggable;
        private TweenerComponent _tweener;
        private IKScheduler _ikScheduler;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _tweener = ownerSystem.GameObject.GetComponent<TweenerComponent>();
            _ikScheduler = ownerSystem.GameObject.GetComponent<IKScheduler>();
        }

        public override bool CanStart()
        {
            bool canStart =  base.CanStart() && !Movement.IsFalling;
            if (!canStart)
            {
                OwnerSystem.RemoveAbilityByName(AbilityName, OwnerSystem.GameObject);
            }

            return canStart;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            _draggable = instigator.GetComponent<IDraggable>();
            if (_draggable == null)
            {
                StopAbility(instigator);
                return;
            }

            if (_ikScheduler != null)
            {
                _ikScheduler.ApplyIK(AvatarIKGoal.LeftHand, _draggable.GetLeftHandTarget());
                _ikScheduler.ApplyIK(AvatarIKGoal.RightHand, _draggable.GetRightHandTarget());
            }
            
            AnimationController.SetAnimationState(_dragAnimState);
            Movement.StopMovement();
            Movement.SetMaxMoveSpeed(_maxMoveSpeed);

            Vector3 position = _draggable.InteractionPoint.position;
            position.y = transform.position.y;
            _tweener.DoLerp(position, _draggable.InteractionPoint.rotation, LerpDuration);
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (_tweener.IsRunningTween)
            {
                return;
            }

            _draggable.StartDrag();
            Movement.MoveByInput();

            _draggable.Move(Movement.Velocity);
            UpdateCharacterPosition();

            Vector2 relativeInput = Vector2.zero;
            relativeInput.x = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.right);
            relativeInput.y = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.forward);

            UpdateAnimParameters(relativeInput);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            _draggable.StopDrag();
            
            if (_ikScheduler != null)
            {
                _ikScheduler.StopIK(AvatarIKGoal.LeftHand);
                _ikScheduler.StopIK(AvatarIKGoal.RightHand);
            }

            OwnerSystem.RemoveAbilityByName(AbilityName, OwnerSystem.GameObject);
        }

        private void UpdateCharacterPosition()
        {
            Vector3 position = _draggable.InteractionPoint.position;
            position.y = transform.position.y;

            Movement.SetPosition(position);
        }
        
        private void UpdateAnimParameters(Vector2 relativeInput, float dampTime = 0.1f)
        {
            AnimationController.Animator.SetFloat(_charRelativeHorParam,
                relativeInput.x,
                dampTime, Time.deltaTime);

            AnimationController.Animator.SetFloat(_charRelativeVerParam,
                relativeInput.y,
                dampTime, Time.deltaTime);
        }
    }
}