using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    public enum MovementStyle
    {
        HoldToWalk, HoldToRun, DoNothing
    }

    [CreateAssetMenu(fileName = "Locomotion", menuName = "Dias Games/Abilities/Locomotion", order = 0)]
    public class Locomotion : Ability
    {
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float sprintSpeed = 5.3f;
        [Tooltip("Determine how to use extra key button to handle movement. If shift is hold, tells system if it should walk, run, or do nothing")]
        [SerializeField] private MovementStyle movementByKey = MovementStyle.HoldToWalk;
        [SerializeField] private string groundedAnimBlendState = "Grounded";

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            
            Movement = OwnerSystem.GameObject.GetComponent<IMovement>();
        }

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            return !Movement.IsFalling;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(groundedAnimBlendState, transitionDuration: 0.25f);
            Movement.SetMaxMoveSpeed(sprintSpeed);
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (Movement.IsFalling)
            {
                StopAbility(OwnerSystem.GameObject);
            }

            Movement.MoveByInput();
            Movement.RotateToMovementDirection();
        }

        protected override void OnStopAbility(GameObject instigator)
        {
        }

        public void WalkButtonPressed(bool pressed)
        {
            if (!IsRunning)
            {
                return;
            }

            switch (movementByKey)
            {
                case MovementStyle.HoldToWalk:
                    Movement.SetMaxMoveSpeed(pressed ? walkSpeed : sprintSpeed);
                    break;
                case MovementStyle.HoldToRun:
                    Movement.SetMaxMoveSpeed(pressed ? sprintSpeed : walkSpeed);
                    break;
                case MovementStyle.DoNothing:
                    Movement.SetMaxMoveSpeed(sprintSpeed);
                    break;
            }
        }
    }
}