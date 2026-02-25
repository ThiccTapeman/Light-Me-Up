using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Components;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    public abstract class ClimbAbility : Ability
    {
        protected ClimbingComponent Climbing { get; private set; }
        protected TweenerComponent Tweener { get; private set; }
        protected IAudioPlayer AudioPlayer { get; private set; }

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            Climbing = ownerSystem.GameObject.GetComponent<ClimbingComponent>();
            Tweener = ownerSystem.GameObject.GetComponent<TweenerComponent>();
            AudioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            Movement.SetCapsuleSize(Movement.CapsuleHeight, Climbing.CapsuleRadiusOnClimbing);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            Movement.ResetCapsuleSize();
        }

        public void DoLerpToTarget(ClimbCastData castData, ClimbJumpAnimation climbJumpAnimation)
        {
            Vector3 targetPosition = Climbing.GetCharacterPositionOnLedge(castData);
            Quaternion targetRotation = Climbing.GetCharacterRotationOnLedge(castData);
            
            Tweener.DoBestLerp(targetPosition, targetRotation, climbJumpAnimation.JumpDuration,parent:castData.Ledge.transform, curve: climbJumpAnimation.Curve);
            AnimationController.SetAnimationState(climbJumpAnimation.AnimationStateName);

            Movement.SetGravityEnabled(false);
            Movement.StopMovement();
        }
    }
}